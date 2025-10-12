using PlayingCard.GamePlay.Configuration;

namespace PlayingCard.GamePlay.PlayModels
{
    public enum RoundState
    {
        Deal,
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
        public DealTarget DealTarget => gameRound.DealTarget;
        public DealFace DealFace => gameRound.DealFace;
        public int DealCardCount => gameRound.DealCardCount;
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
