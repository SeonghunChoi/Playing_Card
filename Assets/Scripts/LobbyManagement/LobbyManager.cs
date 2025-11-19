using PlayingCard.LobbyManagement.NetModels;
using System;
using Unity.Netcode;

namespace PlayingCard.LobbyManagement
{
    public class LobbyManager : NetworkBehaviour
    {
        public enum SeatState : byte
        {
            Inactive,
            Active,
            LockedIn,
        }

        public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
        {
            public ulong ClientId;

            private NetworkString userName;

            public string UserName
            {
                get => userName;
                private set => userName = value;
            }
            public int PlayerNumber;
            public int SeatIdx;
            public float LastChangeTime;

            public SeatState SeatState;

            public LobbyPlayerState(ulong clientId, string name, int playerNumber, SeatState seatState, int seatIdx = -1, float lastChangeTime = 0)
            {
                ClientId = clientId;
                userName = new NetworkString();
                PlayerNumber = playerNumber;
                SeatState = seatState;
                SeatIdx = seatIdx;
                LastChangeTime = lastChangeTime;

                UserName = name;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref ClientId);
                serializer.SerializeValue(ref userName);
                serializer.SerializeValue(ref PlayerNumber);
                serializer.SerializeValue(ref SeatIdx);
                serializer.SerializeValue(ref LastChangeTime);
                serializer.SerializeValue(ref SeatState);
            }

            public bool Equals(LobbyPlayerState other)
            {
                return ClientId == other.ClientId &&
                       userName.Equals(other.userName) &&
                       PlayerNumber == other.PlayerNumber &&
                       SeatIdx == other.SeatIdx &&
                       LastChangeTime.Equals(other.LastChangeTime) &&
                       SeatState.Equals(other.SeatState);
            }
        }

        private NetworkList<LobbyPlayerState> lobbyPlayers;

        /// <summary>
        /// 현재 로비에 있는 플레이어들
        /// </summary>
        public NetworkList<LobbyPlayerState> LobbyPlayers => lobbyPlayers;
        public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);

        public event Action<ulong, int, bool> OnClientChangedSeat;

        private void Awake()
        {
            lobbyPlayers = new NetworkList<LobbyPlayerState>();
        }

        [Rpc(SendTo.Server)]
        public void ChangeReadyRpc(ulong clientId, int seatIdx, bool isready)
        {
            OnClientChangedSeat?.Invoke(clientId, seatIdx, isready);
        }
    }
}
