using NUnit.Framework;
using PlayingCard.LobbyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace PlayingCard.GamePlay.Presenter
{
    public class LobbyPresenter
    {
        private readonly LobbyManager lobbyManager;
        private readonly NetworkManager networkManager;

        public LobbyPresenter(LobbyManager lobbyManager, NetworkManager networkManager)
        {
            this.lobbyManager = lobbyManager;
            this.networkManager = networkManager;
        }


    }
}
