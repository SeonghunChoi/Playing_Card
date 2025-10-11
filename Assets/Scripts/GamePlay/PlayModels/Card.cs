using PlayingCard.GamePlay.Configuration;
using System.Collections.Generic;

namespace PlayingCard.GamePlay.PlayModels
{
    /// <summary>
    /// Suit 종류
    /// </summary>
    public enum Suit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
    }

    /// <summary>
    /// 카드 등급(값)
    /// </summary>
    public enum Rank
    {
        None = 0,
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, 
        Jack, Queen, King, 
        Ace
    }

    /// <summary>
    /// 카드 정보
    /// </summary>
    public struct Card
    {
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }
        
        public bool IsWild { get; private set; }

        public bool IsFaceUp { get; private set; }

        public Card(Suit suit, Rank rank, bool isFaceUp, bool isWild = false)
        {
            Suit = suit;
            Rank = rank;

            IsFaceUp = isFaceUp;

            IsWild = isWild;
        }

        /// <summary>
        /// 카드 뒤집기
        /// </summary>
        public void Flip()
        {
            IsFaceUp = !IsFaceUp;
        }

        /// <summary>
        /// 같은 등급의 카드인지 확인
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public bool IsSameRank(Rank rank)
        {
            if (IsWild) return true;

            return Rank == rank;
        }

        /// <summary>
        /// 같은 Suit 인지 확인
        /// </summary>
        /// <param name="suit"></param>
        /// <returns></returns>
        public bool IsSameSuit(Suit suit)
        {
            if (IsWild) return true;

            return Suit == suit;
        }

        public override string ToString()
        {
            return $"Suit:{Suit}, Rank:{Rank}, IsWild:{IsWild}";
        }
    }
}
