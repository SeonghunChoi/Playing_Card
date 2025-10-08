using PlayingCard.GamePlay.PlayModels;
using TMPro;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.GameState
{
    public class GameRoomStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.GameRoom;

        [SerializeField]
        TextMeshProUGUI textPlayerId;
        [SerializeField]
        TextMeshProUGUI textMoney;
        [SerializeField]
        TextMeshProUGUI textBet;
        [SerializeField]
        TextMeshProUGUI textPot;
        [SerializeField]
        TextMeshProUGUI textRound;

        [Inject]
        PlayTable table;

        protected override void Start()
        {
            base.Start();

            
        }
    }
}
