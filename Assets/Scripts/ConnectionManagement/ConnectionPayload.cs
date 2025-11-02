using System;

namespace PlayingCard.ConnectionManagement
{
    [Serializable]
    public class ConnectionPayload
    {
        public string PlayerId;
        public string PlayerName;
        public bool IsDebug;
    }
}
