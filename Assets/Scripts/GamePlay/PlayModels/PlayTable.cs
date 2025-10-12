using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Configuration.Define;
using PlayingCard.GamePlay.Message;
using PlayingCard.Utilities;
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
        int firstPlayerIdx;
        int roundTurn;

        Queue<Card> deck;
        List<Card> communityCard;
        ulong lastMaxBet, pot, sidePot;
        int round;
        Betting lastBetting;
        PlayRound currentRound;

        private readonly IDisposable startGameDisposable;
        private readonly IDisposable exitGameDisposable;
        private readonly IPublisher<EndGameMessage> endGamePublisher;
        private readonly IPublisher<TurnStartMessage> turnStartPublisher;
        private readonly IDisposable turnActionDisposable;
        private readonly IPublisher<DealCardMessage> dealCardPublisher;

        System.Random random = new System.Random();

        List<Card> dealBuffer = new List<Card>();

        [Inject]
        public PlayTable(
            HandRankingManager rankingManager,
            ISubscriber<StartGameMessage> startGameSubscriber,
            ISubscriber<ExitGameMessage> exitGameSubscriber,
            IPublisher<EndGameMessage> endGamePublisher,
            IPublisher<TurnStartMessage> turnStartPublisher,
            ISubscriber<TurnActionMessage> turnActionSubscriber,
            IPublisher<DealCardMessage> dealCardPublisher)
        {
            this.rankingManager = rankingManager;
            startGameDisposable = startGameSubscriber.Subscribe(StartGame);
            exitGameDisposable = exitGameSubscriber.Subscribe(ExitGame);
            this.endGamePublisher = endGamePublisher;
            this.turnStartPublisher = turnStartPublisher;
            turnActionDisposable = turnActionSubscriber.Subscribe(TrunAction);
            this.dealCardPublisher = dealCardPublisher;
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

            if (cards == null) cards = new List<Card>();
            else cards.Clear();

            if (deck == null) deck = new Queue<Card>();
            else deck.Clear();
            if (communityCard == null) communityCard = new List<Card>();
            else communityCard.Clear();

            players = new List<Player>();
            for (int i = 0; i < game.Rule.MaxPlayer; i++)
            {
                players.Add(new Player(i + 1, 1000));
            }
            firstPlayerIdx = 0;

            lastMaxBet = 0;
            pot = 0;
            sidePot = 0;
            round = 1;
            lastBetting = Betting.Fold;

            SetCurrentRound();

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
                        // 아무도 Action을 안했으므로
                        roundTurn = 0;
                        DealCard();
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
                        for (int i = 0; i < players.Count; i++)
                        {
                            var player = players[i];
                            player.SetState(PlayerState.Waiting);
                        }
                        lastBetting = Betting.Fold;
                        lastMaxBet = 0;
                        roundTurn = 0;
                        round++;
                        SetCurrentRound();
                        PlayRound();
                    }
                    break;
                default:
                    break;
            }
        }

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
            if (currentRound.DealTarget == DealTarget.Table)
            {
                dealBuffer.Clear();
                for (int i = 0; i < currentRound.DealCardCount; i++)
                {
                    if (deck.Count > 0)
                    {
                        var card = deck.Dequeue();
                        if (currentRound.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                            card.Flip();
                        else if (currentRound.DealFace == DealFace.FaceDown && card.IsFaceUp)
                            card.Flip();

                        dealBuffer.Add(card);
                    }
                    else
                    {
                        // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                        endGamePublisher.Publish(new EndGameMessage());
                    }
                }
                communityCard.AddRange(dealBuffer);
                dealCardPublisher.Publish(new DealCardMessage(dealBuffer, null));
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    Player player = GetTurnPlayer(i);
                    if (player.State.IsPlayable() == false)
                    {
                        continue;
                    }

                    dealBuffer.Clear();
                    for (int j = 0; j < currentRound.DealCardCount; j++)
                    {
                        if (deck.Count > 0)
                        {
                            var card = deck.Dequeue();
                            if (currentRound.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                                card.Flip();
                            else if (currentRound.DealFace == DealFace.FaceDown && card.IsFaceUp)
                                card.Flip();

                            player.ReceiveCard(card);
                            dealBuffer.Add(card);
                        }
                        else
                        {
                            // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                            endGamePublisher.Publish(new EndGameMessage());
                        }
                    }
                    dealCardPublisher.Publish(new DealCardMessage(dealBuffer, player));
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

        Player GetTurnPlayer(int turn)
        {
            int idx = firstPlayerIdx + turn;
            if (idx >= players.Count)
                idx = idx % players.Count;

            return players[idx];
        }

        void RunBetting()
        {
            if (players.Count(p => p.State.IsPlayable()) <= 1)
            {
                var winners = players.FindAll(p => p.State.IsPlayable());
                ResolveWinner(winners);
                return;
            }

            if (players.Count(p => p.State.IsBetable(lastBetting)) == 0)
            {
                currentRound.NextState();
                PlayRound();
                return;
            }

            var player = GetTurnPlayer(roundTurn);
            turnStartPublisher.Publish(new TurnStartMessage(MinRaise, round, pot, lastMaxBet, lastBetting, player));
        }

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
                            lastBetting = Betting.Check;
                        player.SetState(PlayerState.Checked);
                    }
                    break;
                case Betting.Bet:
                case Betting.Call:
                    {
                        if (lastBetting < Betting.Call)
                            lastBetting = Betting.Call;
                        player.SetState(PlayerState.Called);
                    }
                    break;
                case Betting.Raise:
                    {
                        if (lastBetting < Betting.Raise)
                            lastBetting = Betting.Raise;
                        player.SetState(PlayerState.Raised);
                    }
                    break;
                case Betting.AllIn:
                    break;
                default:
                    break;
            }

            roundTurn++;
            RunBetting();
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
            Debug.Log($"split:{split}, remainder:{remainder}");
            for (int i = 0; i < winners.Count; i++)
            {
                winners[i].ApplyWinChips(split);
                if (i < (int)remainder) winners[i].ApplyWinChips(1); // 잔여칩 할당
            }
        }

        public void Dispose()
        {
            startGameDisposable?.Dispose();
            exitGameDisposable?.Dispose();
            turnActionDisposable?.Dispose();
        }
    }
}
