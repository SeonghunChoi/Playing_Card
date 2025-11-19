using System;
using Unity.Netcode;

namespace PlayingCard.GamePlay.Model.PlayModels
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
    [Serializable]
    public struct Card : /*INetworkSerializable,*/ IEquatable<Card>
    {
        public Suit Suit;
        public Rank Rank;

        public bool IsFaceUp;

        public bool IsWild;

        public Card(Suit suit, Rank rank, bool isFaceUp, bool isWild = false)
        {
            Suit = suit;
            Rank = rank;

            IsFaceUp = isFaceUp;

            IsWild = isWild;
        }

        //public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        //{
        //    serializer.SerializeValue(ref Suit);
        //    serializer.SerializeValue(ref Rank);
        //    serializer.SerializeValue(ref IsFaceUp);
        //    serializer.SerializeValue(ref IsWild);
        //}

        public bool Equals(Card other)
        {
            return Suit.Equals(other.Suit) 
                && Rank.Equals(other.Rank) 
                && IsWild.Equals(other.IsWild);
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
