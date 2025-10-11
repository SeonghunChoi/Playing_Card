using System.Collections.Generic;

namespace PlayingCard.GamePlay.PlayModels
{
    /// <summary>
    /// Betting 액션 종류
    /// </summary>
    public enum Betting
    {
        Fold,
        Check,
        Bet,
        Call,
        Raise,
        AllIn
    }

    /// <summary>
    /// Player 의 상태
    /// </summary>
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

    /// <summary>
    /// Player 역할
    /// </summary>
    public enum PlayerJob
    {
        Dealer,
        SmallBlind,
        BigBlind,
        Normal,
    }

    /// <summary>
    /// 게임 플레이어
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 고유 번호
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 플레이어가 가지고 있는 칩 개수
        /// </summary>
        public ulong Chips { get; private set; }

        /// <summary>
        /// 게임 동안 Betting 한 칩 개수
        /// </summary>
        public ulong Bet { get; private set; }

        /// <summary>
        /// 플레이어 상태
        /// </summary>
        public PlayerState State { get; private set; }

        /// <summary>
        /// 플레이어가 가진 모든 카드
        /// </summary>
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

        /// <summary>
        /// player가 가지고 있는 카드 중 비공개 카드
        /// </summary>
        public List<Card> Hands = new List<Card>();
        /// <summary>
        /// player가 가지고 있는 카드 중 공개한 카드
        /// </summary>
        public List<Card> Board = new List<Card>();


        public Player(int id, ulong chips)
        {
            Id = id;
            Chips = chips;
            Bet = 0;

            SetState(PlayerState.Waiting);
        }

        /// <summary>
        /// 플레이어의 상태를 바꾼다.
        /// </summary>
        /// <param name="state"></param>
        public void SetState(PlayerState state)
        {
            if (Chips <= 0)
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

        /// <summary>
        /// 배팅한 Chip 개수를 적용한다.
        /// </summary>
        /// <param name="bet"></param>
        public void ApplyBet(ulong bet)
        {
            Chips -= bet;
            this.Bet += bet;
        }

        /// <summary>
        /// 승리로 획득한 Chip 개수를 적용한다.
        /// </summary>
        /// <param name="chips"></param>
        public void ApplyWinChips(ulong chips)
        {
            Chips += chips;
        }

        /// <summary>
        /// 카드를 받는다.
        /// </summary>
        /// <param name="card"></param>
        public void ReceiveCard(Card card)
        {
            if (card.IsFaceUp) Hands.Add(card);
            else Board.Add(card);
        }
    }

    /// <summary>
    /// Player class 의 확장 기능
    /// </summary>
    public static class PlayerExtands
    {
        /// <summary>
        /// 현재 플레이 가능한지 여부
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
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

        /// <summary>
        /// player 가 배팅 가능한 상태인지 여부 보유한 Chip 개수는 고려하지 않는다.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="lastBetting"></param>
        /// <returns></returns>
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
