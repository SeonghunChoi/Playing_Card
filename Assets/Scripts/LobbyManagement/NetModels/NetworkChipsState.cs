using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    public class NetworkChipsState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<ulong> Chips = new NetworkVariable<ulong>();

        public event Action chipsDepleted;
        //public event Action chipsReplenished;

        private void OnEnable()
        {
            Chips.OnValueChanged += ChipsChanged;
        }

        private void OnDisable()
        {
            Chips.OnValueChanged -= ChipsChanged;
        }

        void ChipsChanged(ulong previousValue, ulong newValue)
        {
            if (previousValue > 0 && newValue <= 0)
            {
                // Player Out
                chipsDepleted?.Invoke();
            }
        }
    }
}
