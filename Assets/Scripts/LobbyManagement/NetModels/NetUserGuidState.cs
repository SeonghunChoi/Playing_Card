using PlayingCard.Infrastructure;
using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    public class NetUserGuidState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<NetworkGuid> NetUserGuid = new NetworkVariable<NetworkGuid>();

        public void SetRandomGuid()
        {
            NetUserGuid.Value = new Guid().ToNetworkGuid();
        }
    }
}
