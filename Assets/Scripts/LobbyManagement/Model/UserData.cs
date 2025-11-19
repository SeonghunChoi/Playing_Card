namespace PlayingCard.LobbyManagement.Model
{
    public struct UserData
    {
        public bool IsHost { get; set; }
        public string DisplayName { get; set; }
        public string ID { get; set; }

        public UserData(bool isHost, string displayName, string id)
        {
            IsHost = isHost;
            DisplayName = displayName; 
            ID = id;
        }
    }
}
