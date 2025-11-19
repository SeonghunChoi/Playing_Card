using EnhancedUI.EnhancedScroller;
using PlayingCard.GamePlay.Model.PlayModels;
using TMPro;
using UnityEngine;

namespace PlayingCard.GamePlay.View.UI
{
    public class LobbySeatCellView : EnhancedScrollerCellView
    {
        [SerializeField] TextMeshProUGUI textUserName;
        [SerializeField] TextMeshProUGUI textIsReady;
        [SerializeField] GameObject objHighLight;

        LobbySeat lobbySeat;

        public void Set(LobbySeat lobbySeat)
        {
            this.lobbySeat = lobbySeat;

            if (lobbySeat.SeatState == LobbyManagement.LobbyManager.SeatState.Inactive)
            {
                textUserName.text = "Empty";
                textIsReady.text = string.Empty;
                objHighLight.SetActive(false);
            }
            else
            {
                textUserName.text = lobbySeat.PlayerName;
                textIsReady.text = lobbySeat.SeatState == LobbyManagement.LobbyManager.SeatState.LockedIn ? "READY" : "UNREADY";
                objHighLight.SetActive(lobbySeat.SeatState == LobbyManagement.LobbyManager.SeatState.LockedIn);
            }
        }
    }
}
