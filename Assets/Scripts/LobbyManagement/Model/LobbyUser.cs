using System;

namespace PlayingCard.LobbyManagement.Model
{
    public class LobbyUser
    {
        [Flags]
        public enum UserMemebers
        {
            IsHost = 1,
            DisplayName = 2,
            ID = 4
        }

        public Action<LobbyUser> onChanged;

        public LobbyUser()
        {
            userData = new UserData(isHost: false, displayName: null, id: null);
        }

        UserData userData;

        UserMemebers LastChanged;

        public bool IsHost
        {
            get { return userData.IsHost; }
            set
            {
                if (userData.IsHost != value)
                {
                    userData.IsHost = value;
                    LastChanged = UserMemebers.IsHost;
                    OnChanged();
                }
            }
        }

        public string DisplayName 
        {
            get { return userData.DisplayName; }
            set
            {
                if (userData.DisplayName != value)
                {
                    userData.DisplayName = value;
                    LastChanged = UserMemebers.DisplayName;
                    OnChanged();
                }
            }
        }

        public string ID
        {
            get { return userData.ID; }
            set
            {
                if (userData.ID != value)
                {
                    userData.ID = value;
                    LastChanged = UserMemebers.ID;
                    OnChanged();
                }
            }
        }

        void OnChanged()
        {
            onChanged?.Invoke(this);
        }

        internal void CopyDataFrom(LobbyUser lobbyUser)
        {
            var data = lobbyUser.userData;
            int lastChanged =
                (userData.IsHost == data.IsHost ? 0 : (int)UserMemebers.IsHost) |
                (userData.DisplayName == data.DisplayName ? 0 : (int)UserMemebers.DisplayName) |
                (userData.ID == data.ID ? 0 : (int)UserMemebers.ID);

            if (lastChanged == 0) return;

            userData = data;
            LastChanged = (UserMemebers)lastChanged;

            OnChanged();
        }

        public void ResetState()
        {
            userData = new UserData(false, userData.DisplayName, userData.ID);
        }
    }
}
