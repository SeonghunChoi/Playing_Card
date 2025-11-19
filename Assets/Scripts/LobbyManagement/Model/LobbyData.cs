namespace PlayingCard.LobbyManagement.Model
{
    public struct LobbyData
    {
        public string LobbyID { get; set; }
        public string LobbyCode { get; set; }
        public string RelayJoinCode { get; set; }
        public string LobbyName { get; set; }
        public bool Private { get; set; }
        public int MaxPlayerCount { get; set; }

        public int GameId { get; set; }

        public LobbyData(LobbyData existing)
        {
            LobbyID = existing.LobbyID;
            LobbyCode = existing.LobbyCode;
            RelayJoinCode = existing.RelayJoinCode;
            LobbyName = existing.LobbyName;
            Private = existing.Private;
            MaxPlayerCount = existing.MaxPlayerCount;
            GameId = existing.GameId;
        }

        public LobbyData(string lobbyCode)
        {
            LobbyID = null;
            LobbyCode = lobbyCode;
            RelayJoinCode = null;
            LobbyName = null;
            Private = false;
            MaxPlayerCount = -1;
            GameId = 0;
        }
    }
}
