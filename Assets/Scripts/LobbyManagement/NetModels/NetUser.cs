using PlayingCard.ConnectionManagement;
using PlayingCard.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetUser : NetworkBehaviour
    {
        public NetworkNameState NetworkNameState => networkNameState;
        public NetUserGuidState NetworkUserGuidState => networkUserGuidState;
        public NetworkChipsState NetworkChipsState => networkChipsState;
        public int GameId { get; private set; }

        [SerializeField]
        NetUserRuntimeCollection netUserRuntimeCollection;

        [SerializeField]
        NetworkNameState networkNameState;
        [SerializeField]
        NetUserGuidState networkUserGuidState;
        [SerializeField]
        NetworkChipsState networkChipsState;

        public override void OnNetworkSpawn()
        {
            gameObject.name = $"NetUser:{OwnerClientId}";

            netUserRuntimeCollection.Add(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    networkNameState.Name.Value = playerData.PlayerName;
                    networkUserGuidState.NetUserGuid.Value = playerData.PlayerGUID;
                    networkChipsState.Chips.Value = ulong.Parse(playerData.Values[0]);
                    GameId = int.Parse(playerData.Values[1]);
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemoveNetUser();
        }

        public override void OnNetworkDespawn()
        {
            RemoveNetUser();
        }

        void RemoveNetUser()
        {
            netUserRuntimeCollection.Remove(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    networkNameState.Name.Value = playerData.PlayerName;
                    networkUserGuidState.NetUserGuid.Value = playerData.PlayerGUID;
                    networkChipsState.Chips.Value = ulong.Parse(playerData.Values[0]);
                    GameId = int.Parse(playerData.Values[1]);
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
    }
}
