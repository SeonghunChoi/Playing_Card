using static PlayingCard.LobbyManagement.LobbyManager;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    public class LobbySeat
    {
        public int SeatIdx;

        public SeatState SeatState { get; private set; }
        public int PlayerNumber { get; private set; }
        public string PlayerName { get; private set; }

        public LobbySeat(int seatIdx)
        {
            SeatIdx = seatIdx;
        }

        public void SetInfo(SeatState seatState, int playerNumber, string playerName)
        {
            SeatState = seatState;
            PlayerNumber = playerNumber;
            PlayerName = playerName;
        }
    }
}
