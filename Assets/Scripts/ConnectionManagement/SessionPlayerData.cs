using PlayingCard.Infrastructure;
using PlayingCard.Utilities;

namespace PlayingCard.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public ulong Chips;
        public bool IsReady;
        public NetworkGuid AvatarNetworkGuid;

        public SessionPlayerData(ulong clientID, string playerName, NetworkGuid avatarNetworkGuid, ulong curChips = 0, bool isConnected = false, bool isReady = false)
        {
            ClientID = clientID;
            PlayerName = playerName;
            Chips = curChips;
            IsConnected = isConnected;
            IsReady = isReady;
            AvatarNetworkGuid = avatarNetworkGuid;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            IsReady = false;
        }
    }
}
