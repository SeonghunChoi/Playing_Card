using PlayingCard.GamePlay.Configuration;

namespace PlayingCard.GamePlay.PlayModels
{
    public enum RoundState
    {
        Deal,
        Bet,
        Complete
    }

    public class PlayRound
    {
        public RoundState RoundState { get; private set; }

        public DealTarget DealTarget => gameRound.DealTarget;
        public DealFace DealFace => gameRound.DealFace;
        public int DealCardCount => gameRound.DealCardCount;

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
