using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Configuration.Define;
using PlayingCard.GamePlay.Message;
using PlayingCard.Utilities;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.PlayModels
{
    public interface IPlayTable
    {
        void InitGame(Game game);
        Player GetPlayer(int id);
    }

    public class PlayTable : IPlayTable, IDisposable
    {
        HandRankingManager rankingManager;

        ulong MinRaise => game.Rule.MinRaise;

        Game game;
        /// <summary>
        /// 게임에서 사용할 모든 카드
        /// </summary>
        List<Card> cards;
        List<Player> players;
        int dealerIdx;
        int smallBlindIdx;
        int bigBlindIdx;
        int currentPlayerIdx;

        Queue<Card> deck;
        List<Card> communityCard;
        ulong lastMaxBet, pot, sidePot;
        int round;
        Betting lastBetting;
        PlayRound currentRound;

        private readonly IDisposable startGameDisposable;
        private readonly IDisposable exitGameDisposable;
        private readonly IDisposable endGameDisposable;
        private readonly IPublisher<TurnStartMessage> turnStartPublisher;
        private readonly IDisposable turnActionDisposable;
        private readonly IPublisher<DealCardMessage> dealCardPublisher;
        private readonly IPublisher<WinnerMessage> winnerPublisher;
        private readonly IPublisher<DrawInfoMessage> drawCardInfoPublisher;
        private readonly IDisposable drawCardSelectDisposable;
        private readonly IDisposable drawCardsDisposable;
        private readonly IPublisher<DrawResultMessage> drawResultPublisher;

        System.Random random = new System.Random();

        List<Card> dealBuffer = new List<Card>();

        [Inject]
        public PlayTable(
            HandRankingManager rankingManager,
            ISubscriber<StartGameMessage> startGameSubscriber,
            ISubscriber<ExitGameMessage> exitGameSubscriber,
            ISubscriber<EndGameMessage> endGameSubscriber,
            IPublisher<TurnStartMessage> turnStartPublisher,
            ISubscriber<TurnActionMessage> turnActionSubscriber,
            IPublisher<DealCardMessage> dealCardPublisher,
            IPublisher<WinnerMessage> winnerPublisher,
            IPublisher<DrawInfoMessage> drawCardInfoPublisher,
            ISubscriber<DrawCardSelectMessage> drawCardSelectSubscriber,
            ISubscriber<DrawCardsMessage> drawCardsSubscriber,
            IPublisher<DrawResultMessage> drawResultPublisher)
        {
            this.rankingManager = rankingManager;
            startGameDisposable = startGameSubscriber.Subscribe(StartGame);
            exitGameDisposable = exitGameSubscriber.Subscribe(ExitGame);
            endGameDisposable = endGameSubscriber.Subscribe(EndGame);
            this.turnStartPublisher = turnStartPublisher;
            turnActionDisposable = turnActionSubscriber.Subscribe(TrunAction);
            this.dealCardPublisher = dealCardPublisher;
            this.winnerPublisher = winnerPublisher;
            this.drawCardInfoPublisher = drawCardInfoPublisher;
            drawCardSelectDisposable = drawCardSelectSubscriber.Subscribe(DrawCardSelect);
            drawCardsDisposable = drawCardsSubscriber.Subscribe(DrawCards);
            this.drawResultPublisher = drawResultPublisher;
        }

        private void StartGame(StartGameMessage message)
        {
            PlayRound();
        }

        public Player GetPlayer(int id)
        {
            var player = players.Find(p => p.Id == id);
            if (player == null) player = players[0];

            return player;
        }

        /// <summary>
        /// Deck 에 맞춰 카드들을 생성한다.
        /// </summary>
        public void InitGame(Game game)
        {
            this.game = game;

            players = new List<Player>();
            for (int i = 0; i < game.Rule.MaxPlayer; i++)
            {
                players.Add(new Player(i + 1, 1000));
            }

            if (cards == null) cards = new List<Card>();
            else cards.Clear();

            var suits = game.Deck.SuitList;

            for (int i = 0; i < suits.Count; i++)
            {
                var suit = suits[i];
                for (int multiply = 0; multiply < suit.multiply; multiply++)
                {
                    for (int value = suit.minValue; value <= suit.maxValue; value++)
                    {
                        if (value == 1 && suit.maxValue >= 14) continue;

                        cards.Add(new Card(suit.SuitType, (Rank)value, false));
                    }
                }
            }

            for (int wild = 0; wild < game.Deck.WildCardCount; wild++)
            {
                cards.Add(new Card(Suit.Spades, Rank.None, false, true));
            }

            ResetTable();
        }

        public void ResetTable()
        {
            dealerIdx = -1; // 첫 플레이 시작은 딜러가 없는 상태

            if (deck == null) deck = new Queue<Card>();
            else deck.Clear();
            if (communityCard == null) communityCard = new List<Card>();
            else communityCard.Clear();

            lastMaxBet = 0;
            pot = 0;
            sidePot = 0;
            round = 1;
            lastBetting = Betting.Fold;

            SetCurrentRound();

            Shuffle(cards, 100);

            for (int i = 0; i < cards.Count; i++)
            {
                deck.Enqueue(cards[i]);
            }
        }

        public void Shuffle(IList<Card> list, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                int idx = list.Count;
                while (idx > 1)
                {
                    idx--;
                    int changeIdx = random.Next(idx + 1);
                    var temp = list[changeIdx];
                    list[changeIdx] = list[idx];
                    list[idx] = temp;
                }
            }
        }

        void ExitGame(ExitGameMessage message)
        {
            game = null;
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.MAIN_MENU);
        }

        void SetCurrentRound()
        {
            if (game.Rule.Rounds.Count > round - 1)
                currentRound = new PlayRound(game.Rule.Rounds[round - 1]);
            else
                currentRound = null;
        }

        public void PlayRound()
        {
            // 모든 라운드가 끝났다.
            if (currentRound == null)
            {
                GameResolve();
                return;
            }

            switch (currentRound.RoundState)
            {
                // 카드를 나눠준다.
                case RoundState.Deal:
                    {
                        SetRole();
                        DealCard();
                    }
                    break;
                case RoundState.Blind:
                    {
                        RunBlind();
                    }
                    break;
                case RoundState.Bet:
                    {
                        RunBetting();
                    }
                    break;
                case RoundState.Complete:
                    {
                        // 라운드 정리.
                        var playables = players.FindAll(p => p.State.IsPlayable());
                        for (int i = 0; i < playables.Count; i++)
                        {
                            var player = playables[i];
                            player.SetState(PlayerState.Waiting);
                        }
                        lastBetting = Betting.Fold;
                        lastMaxBet = 0;
                        round++;
                        SetCurrentRound();
                        PlayRound();
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 다음 베팅 플레이어를 구한다.
        /// </summary>
        /// <param name="turn">현재 플레이어 idx</param>
        /// <returns></returns>
        Player GetNextDealPlayer(int curIdx)
        {
            if (players.FindAll(p => p.State.IsPlayable()).Count == 1)
                return null;
            int idx = (curIdx + 1) % players.Count;
            var player = players[idx];
            if (!player.State.IsPlayable())
                player = GetNextDealPlayer(idx);
            return player;
        }

        /// <summary>
        /// 다음 베팅 플레이어를 구한다.
        /// </summary>
        /// <param name="turn">현재 플레이어 idx</param>
        /// <returns></returns>
        Player GetNextBettingPlayer(int curIdx)
        {
            if (players.FindAll(p => !p.State.HasActed() && p.State.IsPlayable()).Count == 0)
                return null;
            int idx = (curIdx + 1) % players.Count;
            var player = players[idx];
            if (player.State.HasActed())
                player = GetNextBettingPlayer(idx);
            return player;
        }

        /// <summary>
        /// 딜러를 정한다.
        /// </summary>
        void SetRole()
        {
            Player next = null;
            if (dealerIdx == -1) // 첫 시작
            {
                dealerIdx = UnityEngine.Random.Range(0, players.Count);
            }
            else
            {
                next = GetNextDealPlayer(dealerIdx);
                dealerIdx = players.IndexOf(next);
            }

            if (players.FindAll(p => p.State.IsPlayable()).Count == 2)
            {
                smallBlindIdx = dealerIdx;
            }
            else
            {
                next = GetNextDealPlayer(dealerIdx);
                smallBlindIdx = players.IndexOf(next);
            }
            next = GetNextDealPlayer(smallBlindIdx);
            bigBlindIdx = players.IndexOf(next);
            if (currentRound.Blind > 0)
            {
                next = GetNextDealPlayer(bigBlindIdx);
                currentPlayerIdx = players.IndexOf(next);
            }
            else
            {
                currentPlayerIdx = smallBlindIdx;
            }
        }

        /// <summary>
        /// 카드를 나누어 준다.
        /// </summary>
        void DealCard()
        {
            if (currentRound.BurnCardCount > 0)
            {
                for (int i = 0; i < currentRound.BurnCardCount; i++)
                {
                    // Card Burn
                    deck.Dequeue();
                }
            }

            var dealCards = currentRound.DealCards;
            for (int i = 0; i < dealCards.Count; i++)
            {
                var dealCard = dealCards[i];
                if (dealCard.DealTarget == DealTarget.Table)
                {
                    dealBuffer.Clear();
                    if (deck.Count > 0)
                    {
                        var card = deck.Dequeue();
                        if (dealCard.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                            card.Flip();
                        else if (dealCard.DealFace == DealFace.FaceDown && card.IsFaceUp)
                            card.Flip();

                        dealBuffer.Add(card);
                    }
                    else
                    {
                        BreakGame();
                        return;
                    }
                    communityCard.AddRange(dealBuffer);
                    dealCardPublisher.Publish(new DealCardMessage(dealBuffer, null));
                }
                else
                {
                    var playables = players.FindAll(p => p.State.IsPlayable());
                    int playerCount = playables.Count;
                    int dealIdx = dealerIdx;
                    for (int j = 0; j < playerCount; j++)
                    {
                        Player player = GetNextDealPlayer(dealIdx);
                        dealBuffer.Clear();
                        if (deck.Count > 0)
                        {
                            var card = deck.Dequeue();
                            if (dealCard.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                                card.Flip();
                            else if (dealCard.DealFace == DealFace.FaceDown && card.IsFaceUp)
                                card.Flip();

                            player.ReceiveCard(card);
                            dealBuffer.Add(card);
                        }
                        else
                        {
                            // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                            BreakGame();
                            return;
                        }
                        dealCardPublisher.Publish(new DealCardMessage(dealBuffer, player));
                        // 다음 플레이어로 변경
                        player = GetNextDealPlayer(dealIdx);
                        dealIdx = players.IndexOf(player);
                    }
                }
            }
            // 카드를 모두 나누어 주었으므로 플레이어들 상태를 변경해 준다.
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                player.SetState(PlayerState.Playing);
            }
            currentRound.NextState();
            PlayRound();
        }

        /// <summary>
        /// blind를 처리한다.
        /// </summary>
        void RunBlind()
        {
            ulong blind = currentRound.Blind;
            ulong samllBlind = (blind / 2) + (blind % 2);
            if (blind > 0)
            {
                var sb = players[smallBlindIdx];
                pot += samllBlind;
                sb.ApplyBet(samllBlind);
                sb.SetState(PlayerState.Playing);

                var bb = players[bigBlindIdx];
                pot += blind;
                bb.ApplyBet(blind);
                bb.SetState(PlayerState.Playing);

                lastMaxBet = blind;
                lastBetting = Betting.Raise;
            }

            ulong ante = currentRound.Ante;
            if (ante > 0)
            {
                var playables = players.FindAll(p => p.Chips > ante);
                for (int i = 0; i < playables.Count; i++)
                {
                    var player = playables[i];
                    pot += ante;
                    player.ApplyBet(ante);
                    player.SetState(PlayerState.Checked);
                }
                lastMaxBet = ante;
            }

            currentRound.NextState();
            PlayRound();
        }

        /// <summary>
        /// 베팅을 시작한다.
        /// </summary>
        void RunBetting()
        {
            if (players.Count(p => p.State.IsPlayable()) <= 1)
            {
                var winners = players.FindAll(p => p.State.IsPlayable());
                ResolveWinner(winners);
                return;
            }

            if (players.Count(p => !p.State.HasActed()) == 0)
            {
                currentRound.NextState();
                PlayRound();
                return;
            }

            var player = players[currentPlayerIdx];
            string roundName = currentRound.RoundName.IsNullOrWhitespace() ? $"Round {round}" : currentRound.RoundName; 
            turnStartPublisher.Publish(new TurnStartMessage(MinRaise, roundName, pot, lastMaxBet, lastBetting, player));
        }

        /// <summary>
        /// 플레이어가 Turn 에 취한 행동을 처리한다.
        /// </summary>
        /// <param name="message"></param>
        void TrunAction(TurnActionMessage message)
        {
            var player = message.player;

            if (message.bet > 0)
            {
                if (sidePot > 0)
                {
                    sidePot += message.bet;
                }
                else
                {
                    pot += message.bet;
                    if (message.bet > lastMaxBet)
                        lastMaxBet = message.bet;
                }
                player.ApplyBet(message.bet);
            }

            switch (message.betting)
            {
                case Betting.Fold:
                    {
                        player.SetState(PlayerState.Folded);
                    }
                    break;
                case Betting.Check:
                    {
                        if (lastBetting < Betting.Check)
                        {
                            lastBetting = Betting.Check;
                        }
                        player.SetState(PlayerState.Checked);
                    }
                    break;
                case Betting.Bet:
                    {
                        if (lastBetting < Betting.Bet)
                        {
                            lastBetting = Betting.Bet;
                        }
                        var checkPlayers = players.FindAll(p => p.State.HasActed() && p.State < PlayerState.Betted);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].SetState(PlayerState.Playing);
                        }  
                        player.SetState(PlayerState.Betted);
                    }
                    break;
                case Betting.Call:
                    {
                        if (lastBetting < Betting.Call)
                        {
                            lastBetting = Betting.Call;
                        }
                        var checkPlayers = players.FindAll(p => p.State.HasActed() && p.State < PlayerState.Called);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].SetState(PlayerState.Playing);
                        }
                        player.SetState(PlayerState.Called);
                    }
                    break;
                case Betting.Raise:
                    {
                        if (lastBetting < Betting.Raise)
                        {
                            lastBetting = Betting.Raise;
                        }
                        var checkPlayers = players.FindAll(p => p.State.HasActed() && p.State < PlayerState.Raised);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].SetState(PlayerState.Playing);
                        }
                        player.SetState(PlayerState.Raised);
                    }
                    break;
                case Betting.AllIn:
                    {
                        player.SetState(PlayerState.AllIn);
                    }
                    break;
                default:
                    break;
            }

            var next = GetNextBettingPlayer(currentPlayerIdx);
            if (next != null)
                currentPlayerIdx = players.IndexOf(next);
            RunBetting();
        }

        void DrawCardSelect(DrawCardSelectMessage message)
        {
            if (currentRound.DrawCardCount > 0)
            {
                var player = players.Find(p => p.Id == message.objectPlayer.Id);
                if (player != null && !player.IsDraw)
                {
                    player.SelectDrawCard(message.objectCard, currentRound.DrawCardCount);
                    drawCardInfoPublisher.Publish(new DrawInfoMessage(player.DrawsCount));
                }
            }
        }

        void DrawCards(DrawCardsMessage message)
        {
            var player = message.player;
            List<Card> cards = new List<Card>();
            if (player.DrawsCount > deck.Count)
            {
                // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                BreakGame();
                return;
            }

            for (int i = 0; i < player.DrawsCount; i++)
            {
                cards.Add(deck.Dequeue());
            }

            player.DrawCards(cards);

            drawResultPublisher.Publish(new DrawResultMessage(player, cards));
        }

        /// <summary>
        /// 여러명이 남은 상태에서 게임이 종료되었을 때 승자를 결정한다.
        /// </summary>
        void GameResolve()
        {
            var playables = players.FindAll(p => p.State.IsPlayable());

            Dictionary<Player, HandRanking> topPlayers = new Dictionary<Player, HandRanking>();
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                var hands = player.AllCards;

                var HandRanking = rankingManager.GetHandRankingType(hands);

                if (topPlayers.Count == 0)
                {
                    topPlayers.Add(player, HandRanking);
                }
                else
                {
                    var topHand = topPlayers.Values.FirstOrDefault();
                    int cmp = topHand.CompareHandRanking(HandRanking);
                    if (cmp == 0)
                    {
                        topPlayers.Add(player, HandRanking);
                    }
                    else if (cmp == 1)
                    {
                        topPlayers.Clear();
                        topPlayers.Add(player, HandRanking);
                    }
                }
            }
            ResolveWinner(topPlayers.Keys.ToList());
        }

        /// <summary>
        /// 승자 처리
        /// </summary>
        /// <param name="winners"></param>
        void ResolveWinner(List<Player> winners)
        {
#if UNITY_EDITOR
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ResolveWinner");
            for (int i = 0; i < winners.Count; i++)
            {
                var player = winners[i];
                var hands = player.AllCards;
                var handRanking = rankingManager.GetHandRankingType(hands);
                sb.AppendLine($"Player:{player.Id}, {handRanking.ToString()}");
                sb.AppendLine("--------------------------------------------------------------------------------");
            }
            Debug.Log(sb.ToString());
#endif
            ulong split = pot / (ulong)winners.Count;
            ulong remainder = pot % (ulong)winners.Count;
            Dictionary<Player, ulong> winnerChips = new Dictionary<Player, ulong>();
            for (int i = 0; i < winners.Count; i++)
            {
                ulong winChips = 0;
                winChips += split;
                if (i < (int)remainder) winChips++; // 잔여칩 할당
                winners[i].ApplyWinChips(winChips);
                winnerChips.Add(winners[i], winChips);
            }
            winnerPublisher.Publish(new WinnerMessage(winnerChips));
        }

        private void BreakGame()
        {
            var playables = players.FindAll(p => p.Chips > 0);
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                player.BreakGame();
            }

            ResetTable();
            PlayRound();
        }

        private void EndGame(EndGameMessage message)
        {
            var playables = players.FindAll(p => p.Chips > 0);
            if (playables.Count <= 1)
            {
                Debug.Log("Win");
                return;
            }
            // 플레이어들 상태 정리
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                player.ResetGame();
            }

            ResetTable();
            PlayRound();
        }

        public void Dispose()
        {
            startGameDisposable?.Dispose();
            exitGameDisposable?.Dispose();
            endGameDisposable?.Dispose();
            turnActionDisposable?.Dispose();
            drawCardSelectDisposable?.Dispose();
            drawCardsDisposable?.Dispose();
        }
    }
}
