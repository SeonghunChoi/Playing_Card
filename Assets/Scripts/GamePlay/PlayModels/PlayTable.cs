using PlayingCard.GamePlay.Configuration;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.GamePlay.PlayModels
{
    public class PlayTable
    {
        Game game;
        List<Card> cards;
        Dictionary<int, Player> players;
        ulong pot;
        int round;
        GameRound currentRound;

        System.Random random = new System.Random();

        public List<PlayObject.CardObject> ShareCards;

        public void SetGame(Game game)
        {
            this.game = game;

            InitGame(game);
        }

        /// <summary>
        /// Deck 에 맞춰 카드들을 생성한다.
        /// </summary>
        private void InitGame(Game game)
        {
            if (cards.IsNullOrEmpty()) cards = new List<Card>();
            else cards.Clear();
            players = new Dictionary<int, Player>();
            for (int i = 0; i < game.Rule.MaxPlayer; i++)
            {
                players.Add(i, new Player(i, 1000));
            }
            pot = 0;
            round = 0;
            currentRound = game.Rule.Rounds[round];

            var suits = game.Deck.SuitList;

            for (int i = 0; i < suits.Count; i++)
            {
                var suit = suits[i];
                for (int multiply = 0; multiply < suit.multiply; multiply++)
                {
                    for (int value = suit.minValue; value <= suit.maxValue; value++)
                    {
                        if (value == 1 && suit.maxValue >= 14) continue;

                        cards.Add(new Card(suit.SuitType, (Rank)value));
                    }
                }
            }

            for (int wild = 0; wild < game.Deck.WildCardCount; wild++)
            {
                cards.Add(new Card(Suit.Spades, Rank.None, true));
            }

            Shuffle(cards, 100);

#if UNITY_EDITOR
            for (int i = 0; i < cards.Count; i++)
            {
                Debug.Log(cards[i].ToString());
            } 
#endif
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
    }
}
