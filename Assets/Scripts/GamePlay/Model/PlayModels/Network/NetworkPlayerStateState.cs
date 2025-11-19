using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    public class NetworkPlayerStateState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<PlayerState> PlayerState = new NetworkVariable<PlayerState>();

        public event Action<PlayerState> OnPlayerStateChanged;

        private void OnEnable()
        {
            PlayerState.OnValueChanged += PlayerStateChanged;
        }

        private void OnDisable()
        {
            PlayerState.OnValueChanged -= PlayerStateChanged;
        }

        private void PlayerStateChanged(PlayerState previousValue, PlayerState newValue)
        {
            OnPlayerStateChanged?.Invoke(newValue);
        }
    }
}
