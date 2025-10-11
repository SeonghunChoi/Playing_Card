using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;
using System.Collections.Generic;
using System.Linq;

namespace PlayingCard.GamePlay
{
    public enum HandRank
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalStraightFlush,
        FiveOfAKind,
    }

    public enum ComboType
    {
        SameSuit,
        SameNumber,
        Straight
    }

    public class HandRankingManager
    {
        //public HandRankingManager(GameRule rule)
        //{
        //    this.rule = rule;
        //}

        //GameRule rule;

        public HandRanking GetHandRankingType(List<Card> hand)
        {
            hand.Sort((x, y) => x.Rank.CompareTo(y.Rank));
            var allCombos = GetFiveCardCombinations(hand);

            Rank bestRank = hand[hand.Count - 1].Rank;
            HandRank bestHandRank = HandRank.HighCard;

            foreach (var combo in allCombos)
            {
                var handRank = EvaluateHand(combo);
                if (handRank > bestHandRank)
                    bestHandRank = handRank;
            }

            return new HandRanking(bestHandRank, bestRank);
        }

        public static bool IsFlush(List<Card> hand)
        {
            var suit = hand[0].Suit;
            return hand.All(card => card.IsSameSuit(suit));
        }
        
        public static bool IsStraight(List<Card> hand)
        {
            var ranks = hand.OrderBy(c => c.Rank).ToList();

            var royalStraight = new List<Rank> { Rank.Two, Rank.Three, Rank.Four, Rank.Five, Rank.Ace };
            bool isSequenceEqual = true;
            for (int i = 0; i < 5; i++)
            {
                var rank = ranks[i];
                var royal = royalStraight[i];
                if (!rank.IsSameRank(royal))
                {
                    isSequenceEqual = false;
                    break;
                }
            }
            if (isSequenceEqual) return true;

            for (int i = 0; i < ranks.Count - 1; i++)
            {
                if ((int)ranks[i + 1].Rank - (int)ranks[i].Rank != 1)
                    return false;
            }
            return true;
        }

        public static HandRank EvaluateHand(List<Card> hand)
        {
            var rankCounts = GetRankCounts(hand);
            bool isFlush = IsFlush(hand);
            bool isStraight = IsStraight(hand);

            if (isFlush && isStraight && hand.Any(c => c.Rank == Rank.Ace))
                return HandRank.RoyalStraightFlush;
            if (isFlush && isStraight)
                return HandRank.StraightFlush;
            if (rankCounts.ContainsValue(4))
                return HandRank.FourOfAKind;
            if (rankCounts.ContainsValue(3) && rankCounts.ContainsValue(2))
                return HandRank.FullHouse;
            if (isFlush)
                return HandRank.Flush;
            if (isStraight)
                return HandRank.Straight;
            if (rankCounts.ContainsValue(3))
                return HandRank.ThreeOfAKind;
            if (rankCounts.Values.Count(v => v == 2) == 2)
                return HandRank.TwoPair;
            if (rankCounts.ContainsValue(2))
                return HandRank.OnePair;

            return HandRank.HighCard;

        }

        public static Dictionary<Rank, int> GetRankCounts(List<Card> hand)
        {
            return hand.GroupBy(card => card.Rank).ToDictionary(group => group.Key, group => group.Count());
        }

        List<List<Card>> GetFiveCardCombinations(List<Card> hands)
        {
            var combinations = new List<List<Card>>();
            int handCount = hands.Count;

            for (int i = 0; i < handCount - 4; i++)
            {
                for (int j = i + 1; j < handCount - 3; j++)
                {
                    for (int k = j + 1; k < handCount - 2; k++)
                    {
                        for (int l = k + 1; l < handCount - 1; l++)
                        {
                            for (int m = l + 1; m < handCount; m++)
                            {
                                combinations.Add(new List<Card>()
                                {
                                    hands[i], hands[j], hands[k], hands[l], hands[m]
                                });
                            }
                        }
                    }
                }
            }

            return combinations;
        }
    }

    public class HandRanking
    {
        public HandRanking(HandRank bestHandRank, Rank bestRank)
        {
            HandRank = bestHandRank;
            Rank = bestRank;
        }

        public HandRank HandRank;

        public Rank Rank;

        public override string ToString()
        {
            return $"HandRank:{HandRank}, Rank:{Rank}";
        }
    }
}
