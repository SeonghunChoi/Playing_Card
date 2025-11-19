using PlayingCard.Infrastructure;
using PlayingCard.Utilities;
using UnityEngine;

namespace PlayingCard.ConnectionManagement
{
    public struct SessionPlayerData : ISessionData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        public NetworkGuid PlayerGUID;
        public string[] Values;
        public bool IsReady;
        public bool HasSpawn;

        public SessionPlayerData(ulong clientID, string playerName, NetworkGuid playerGuid, string[] values, bool isConnected = false, bool isReady = false)
        {
            ClientID = clientID;
            PlayerName = playerName;
            PlayerNumber = -1;
            PlayerPosition = Vector3.zero;
            PlayerRotation = Quaternion.identity;
            PlayerGUID = playerGuid;
            Values = values;
            IsConnected = isConnected;
            IsReady = isReady;
            HasSpawn = false;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            IsReady = false;
        }
    }
}
