using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    public class NetworkPlayerBetState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<ulong> Bet = new NetworkVariable<ulong>();

        //public event Action chipsDepleted;
        public event Action betReplenished;

        private void OnEnable()
        {
            Bet.OnValueChanged += ChipsChanged;
        }

        private void OnDisable()
        {
            Bet.OnValueChanged -= ChipsChanged;
        }

        void ChipsChanged(ulong previousValue, ulong newValue)
        {
            if (newValue > 0 && newValue > previousValue)
            {
                betReplenished?.Invoke();
            }
        }
    }
}
