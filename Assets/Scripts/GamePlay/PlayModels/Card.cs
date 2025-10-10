using PlayingCard.GamePlay.Configuration;
using System.Collections.Generic;

namespace PlayingCard.GamePlay.PlayModels
{
    public enum Suit
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
    }

    public enum Rank
    {
        None = 0,
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, 
        Jack, Queen, King, 
        Ace
    }

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

        public void Flip()
        {
            IsFaceUp = !IsFaceUp;
        }

        public bool IsSameRank(Rank rank)
        {
            if (IsWild) return true;

            return Rank == rank;
        }

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
