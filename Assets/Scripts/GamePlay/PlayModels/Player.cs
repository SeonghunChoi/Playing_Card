using System.Collections.Generic;

namespace PlayingCard.GamePlay.PlayModels
{
    public enum Betting
    {
        Fold,
        Check,
        Bet,
        Call,
        Raise,
        AllIn
    }

    public enum PlayerState
    {
        Waiting,    // 대기 중
        Playing,    // 현재 게임 중
        Folded,     // 폴드함
        AllIn,      // 올인
        Checked,    // 체크함
        Called,     // 콜함
        Raised,     // 레이즈함
        Out         // 게임에서 탈락
    }

    public enum PlayerJob
    {
        Dealer,
        SmallBlind,
        BigBlind,
        Normal,
    }

    public class Player
    {
        public int Id { get; private set; }
        public ulong Money { get; private set; }
        public ulong Bet { get; private set; }
        public PlayerState State { get; private set; }

        //내가 가진 모든 카드
        public List<Card> AllCards
        {
            get
            {
                List<Card> cards = new List<Card>();
                cards.AddRange(Hands);
                cards.AddRange(Board);

                return cards;
            }
        }

        //player가 가지고 있는 카드 중 비공개 카드
        public List<Card> Hands = new List<Card>();
        //player가 가지고 있는 카드 중 공개한 카드
        public List<Card> Board = new List<Card>();


        public Player(int id, ulong money)
        {
            Id = id;
            Money = money;
            Bet = 0;

            SetState(PlayerState.Waiting);
        }

        public void SetState(PlayerState state)
        {
            if (Money <= 0)
            {
                State = PlayerState.Out;
                return;
            }

            if (state == PlayerState.Waiting)
            {
                Bet = 0;
            }

            State = state;
        }

        public void ApplyBet(ulong bet)
        {
            Money -= bet;
            this.Bet += bet;
        }

        public void ApplyWin(ulong chips)
        {
            Money += chips;
        }

        public void ReceiveCard(Card card)
        {
            if (card.IsFaceUp) Hands.Add(card);
            else Board.Add(card);
        }
    }

    public static class PlayerExtands
    {
        public static bool IsPlayable(this PlayerState state)
        {
            bool result = false;

            switch (state)
            {
                case PlayerState.Waiting:
                case PlayerState.Playing:
                case PlayerState.AllIn:
                case PlayerState.Checked:
                case PlayerState.Called:
                case PlayerState.Raised:
                    result = true;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static bool IsBetable(this PlayerState state, Betting lastBetting)
        {
            bool result = false;

            switch (state)
            {
                case PlayerState.Playing:
                case PlayerState.AllIn:
                    result = true;
                    break;
                case PlayerState.Checked:
                    if (lastBetting > Betting.Check)
                        result = true;
                    break;
                case PlayerState.Called:
                    if (lastBetting > Betting.Call)
                        result = true;
                    break;
                case PlayerState.Raised:
                    result = true;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
