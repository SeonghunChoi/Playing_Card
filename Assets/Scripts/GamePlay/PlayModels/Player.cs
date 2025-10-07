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

    public class Player
    {
        public int Id;
        public ulong Money;

        public Player(int id, ulong money)
        {
            Id = id;
            Money = money;
        }

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

        //내가 가지고 있는 카드 중 비공개 카드
        public List<Card> Hands;
        //내가 가지고 있는 카드 중 공개한 카드
        public List<Card> Board;

        public void ReceiveCard(List<Card> cards, bool isHands = true)
        {
            if (isHands) Hands.AddRange(cards);
            else Board.AddRange(cards);
        }
    }
}
