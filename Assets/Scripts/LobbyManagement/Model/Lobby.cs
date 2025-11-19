using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.LobbyManagement.Model
{
    [Serializable]
    public class Lobby
    {
        public Lobby(LobbyData data)
        {
            this.data = data;
        }

        public event Action<Lobby> onChanged;

        Dictionary<string, LobbyUser> lobbyUsers = new Dictionary<string, LobbyUser>();

        public Dictionary<string, LobbyUser> LobbyUsers => lobbyUsers;

        LobbyData data;

        public LobbyData Data => data;

        public void AddUser(LobbyUser user)
        {
            if (!lobbyUsers.ContainsKey(user.ID))
            {
                DoAddUser(user);
                OnChanged();
            }
        }

        void DoAddUser(LobbyUser user)
        {
            lobbyUsers.Add(user.ID, user);
            user.onChanged += OnChangedUser;
        }

        public void RemoveUser(LobbyUser user)
        {
            DoRemoveUser(user);
            OnChanged();
        }

        void DoRemoveUser(LobbyUser user)
        {
            if (!lobbyUsers.ContainsKey(user.ID))
            {
                Debug.LogWarning($"User {user.DisplayName}({user.ID}) does not exist in lobby: {LobbyID}");
            }

            lobbyUsers.Remove(user.ID);
            user.onChanged -= OnChangedUser;
        }

        void OnChangedUser(LobbyUser user)
        {
            OnChanged();
        }

        void OnChanged()
        {
            onChanged?.Invoke(this);
        }

        public string LobbyID
        {
            get => data.LobbyID;
            set
            {
                data.LobbyID = value;
                OnChanged();
            }
        }

        public string LobbyCode
        {
            get => data.LobbyCode;
            set
            {
                data.LobbyCode = value;
                OnChanged();
            }
        }

        public string LobbyName
        {
            get => data.LobbyName;
            set
            {
                data.LobbyName = value;
                OnChanged();
            }
        }

        public bool Private
        {
            get => data.Private;
            set
            {
                data.Private = value;
                OnChanged();
            }
        }

        public int MaxPlayerCount
        {
            get => data.MaxPlayerCount;
            set
            {
                data.MaxPlayerCount = value;
                OnChanged();
            }
        }

        public int UserCount => lobbyUsers.Count;

        public void CopyDataFrom(LobbyData data, Dictionary<string, LobbyUser> currUsers)
        {
            this.data = data;

            if (currUsers == null)
            {
                lobbyUsers = new Dictionary<string, LobbyUser>();
            }
            else
            {
                List<LobbyUser> toRemove = new List<LobbyUser>();
                foreach (var oldUser in lobbyUsers)
                {
                    if (currUsers.ContainsKey(oldUser.Key))
                    {
                        oldUser.Value.CopyDataFrom(currUsers[oldUser.Key]);
                    }
                    else
                    {
                        toRemove.Add(oldUser.Value);
                    }
                }

                foreach (var remove in toRemove)
                {
                    DoRemoveUser(remove);
                }

                foreach (var currUser in currUsers)
                {
                    if (!lobbyUsers.ContainsKey(currUser.Key))
                    {
                        DoAddUser(currUser.Value);
                    }
                }
            }

            OnChanged();
        }

        public void Reset(LobbyUser user)
        {
            CopyDataFrom(new LobbyData(), new Dictionary<string, LobbyUser>());
            AddUser(user);
        }
    }
}
