using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    [RequireComponent(typeof(ClientPlayer))]
    public class ClientPlayerManager : NetworkBehaviour
    {
        [SerializeField]
        ClientPlayer CashedClientPlayer;

        static List<ClientPlayer> activePlayers = new List<ClientPlayer>();

        public static List<ClientPlayer> GetClientPlayers()
        {
            return activePlayers;
        }

        public static ClientPlayer GetClientPlayer(ulong ownerClientId)
        {
            foreach (var clientPlayer in activePlayers)
            {
                if (clientPlayer.OwnerClientId == ownerClientId)
                {
                    return clientPlayer;
                }
            }

            return null;
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                activePlayers.Add(CashedClientPlayer);
            }
            else
            {
                enabled = false;
            }
        }

        private void OnDisable()
        {
            activePlayers.Remove(CashedClientPlayer);
        }
    }
}
