using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Configuration.Define;
using PlayingCard.GamePlay.Message;
using PlayingCard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.PlayModels
{
    public interface IPlayTable
    {
        ulong MinRiase { get; }
        ulong Pot { get; }
        ulong SidePot { get; }
        ulong MaxBet { get; }
        int Round { get; }
        void InitGame(Game game);
        void ExitGame();
        Player GetPlayer(int id);
    }

    public class PlayTable : IPlayTable, IDisposable
    {
        public ulong MinRiase => game.Rule.MinRaise;
        public ulong Pot { get { return pot; } }
        public ulong SidePot { get { return sidePot; } }
        public ulong MaxBet { get { return maxBet; } }
        public int Round { get { return round; } }

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
        ulong maxBet, pot, sidePot;
        int round;
        PlayRound currentRound;

        private readonly IDisposable startGameDisposable;
        private readonly IPublisher<EndGameMessage> endGamePublisher;
        private readonly IPublisher<TrunStartMessage> turnStartPublisher;

        System.Random random = new System.Random();

        public List<PlayObject.CardObject> ShareCards;

        [Inject]
        public PlayTable(
            ISubscriber<StartGameMessage> startGameSubscriber,
            IPublisher<EndGameMessage> endGamePublisher,
            IPublisher<TrunStartMessage> turnStartPublisher)
        {
            startGameDisposable = startGameSubscriber.Subscribe(StartGame);
            this.endGamePublisher = endGamePublisher;
            this.turnStartPublisher = turnStartPublisher;
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

            pot = 0;
            sidePot = 0;
            round = 1;

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
#if UNITY_EDITOR
                Debug.Log(cards[i].ToString());
#endif
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

        public void ExitGame()
        {
            game = null;
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.MAIN_MENU);
        }

        void SetCurrentRound()
        {
            if (game.Rule.Rounds.Count > Round - 1)
                currentRound = new PlayRound(game.Rule.Rounds[round - 1]);
            else
                currentRound = null;
        }

        public void PlayRound()
        {
            if (currentRound == null)
            {
                endGamePublisher.Publish(new EndGameMessage());
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
                    break;
                default:
                    break;
            }
        }

        void DealCard()
        {
            if (currentRound.DealTarget == DealTarget.Table)
            {
                for (int i = 0; i < currentRound.DealCardCount; i++)
                {
                    if (deck.Count > 0)
                    {
                        communityCard.Add(deck.Dequeue());
                    }
                    else
                    {
                        // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                        endGamePublisher.Publish(new EndGameMessage());
                    }
                }
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
                        }
                        else
                        {
                            // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                            endGamePublisher.Publish(new EndGameMessage());
                        }
                    }
                    player.SetState(PlayerState.Playing);
                }
            }
            currentRound.NextState();
            PlayRound();
        }

        Player GetTurnPlayer(int turn)
        {
            int idx = firstPlayerIdx + turn;
            if (idx >= players.Count)
                idx -= players.Count;

            return players[idx];
        }

        void RunBetting()
        {
            if (players.Count(p => p.State.IsPlayable()) <= 1)
            {
                ResolveWinner();
                return;
            }

            if (players.Count(p => p.State.IsBetable()) == 0)
            {
                currentRound.NextState();
                return;
            }

            var player = GetTurnPlayer(roundTurn);
            turnStartPublisher.Publish(new TrunStartMessage(player));
        }

        void ResolveWinner()
        {

        }

        public void Dispose()
        {
            startGameDisposable?.Dispose();
        }
    }
}
