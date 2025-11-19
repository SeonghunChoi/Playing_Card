using PlayingCard.ConnectionManagement;
using PlayingCard.Utilities;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    [RequireComponent(typeof(ServerPlayer))]
    public class ServerPlayerManager : NetworkBehaviour
    {
        [SerializeField]
        ServerPlayer CashedServerPlayer;

        static List<ServerPlayer> activePlayers = new List<ServerPlayer>();

        public static List<ServerPlayer> GetServerPlayers()
        {
            return activePlayers;
        }

        public static ServerPlayer GetServerPlayer(ulong ownerClientId)
        {
            foreach (var serverPlayer in activePlayers)
            {
                if (serverPlayer.OwnerClientId == ownerClientId)
                {
                    return serverPlayer;
                }
            }

            return null;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                activePlayers.Add(CashedServerPlayer);
            }
            else
            {
                enabled = false;
            }
        }

        private void OnDisable()
        {
            activePlayers.Remove(CashedServerPlayer);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
    }
}
