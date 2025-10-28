using PlayingCard.GamePlay.Model.Configuration;
using System.Collections.Generic;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    public enum RoundState
    {
        Deal,
        Blind,
        Bet,
        Complete
    }

    /// <summary>
    /// 라운드 정보를 상태별로 처리함
    /// </summary>
    public class PlayRound
    {
        public RoundState RoundState { get; private set; }

        public string RoundName => gameRound.RoundName;
        public ulong Blind => gameRound.Blind;
        public ulong Ante => gameRound.Ante;
        public List<DealCardInfo> DealCards => gameRound.DealCards;
        public int BurnCardCount => gameRound.BurnCardCount;
        public int DrawCardCount => gameRound.DrawCardCount;

        GameRound gameRound;

        public PlayRound(GameRound gameRound)
        {
            this.gameRound = gameRound;

            RoundState = RoundState.Deal;
        }

        public void NextState()
        {
            switch (RoundState)
            {
                case RoundState.Deal:
                    RoundState = RoundState.Blind;
                    break;
                case RoundState.Blind:
                    RoundState = RoundState.Bet;
                    break;
                case RoundState.Bet:
                    RoundState = RoundState.Complete;
                    break;
                default:
                    break;
            }
        }
    }
}
