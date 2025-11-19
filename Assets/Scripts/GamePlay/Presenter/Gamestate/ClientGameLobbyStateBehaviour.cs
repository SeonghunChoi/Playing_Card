using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.View.UI;
using PlayingCard.LobbyManagement;
using PlayingCard.Utilities.Net;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientGameLobbyStateBehaviour : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks netcodeHooks;
        [SerializeField]
        LobbyManager lobbyManager;

        [SerializeField]
        UILobby uiLobby;

        public override GameState ActiveState => GameState.Lobby;

        [Inject]
        ConnectionManager connectionManager;

        int lastSeatSelected = -1;
        bool hasLocalPlayerLockedIn = false;

        protected override void Awake()
        {
            base.Awake();

            netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (netcodeHooks)
            {
                netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
        }

        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
            }
            else
            {
                lobbyManager.IsLobbyClosed.OnValueChanged += OnLobbyClosedChanged;
                lobbyManager.LobbyPlayers.OnListChanged += OnLobbyPlayersChanged;
                Debug.Log("Client Lobby");
            }

            uiLobby.InitializeSeat();
        }

        void OnNetworkDespawn()
        {
            if (lobbyManager)
            {
                lobbyManager.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
                lobbyManager.LobbyPlayers.OnListChanged -= OnLobbyPlayersChanged;
            }
        }

        protected override void Start()
        {
            base.Start();
            uiLobby.ConfigureLobbyMode(LobbyMode.Unready);
            uiLobby.onClickRaady = OnPlayerClickedReady;

            UpdateSeatState(LobbyManager.SeatState.Inactive);
        }

        private void OnLobbyPlayersChanged(NetworkListEvent<LobbyManager.LobbyPlayerState> changeEvent)
        {
            UpdateSeats();

            int localPlayerIdx = -1;
            for (int i = 0; i < lobbyManager.LobbyPlayers.Count; i++)
            {
                if (lobbyManager.LobbyPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    localPlayerIdx = i;
                    break;
                }
            }

            if (localPlayerIdx == -1)
            {
                UpdateSeatState(LobbyManager.SeatState.Inactive);
            }
            else if (lobbyManager.LobbyPlayers[localPlayerIdx].SeatState == LobbyManager.SeatState.Inactive)
            {
                UpdateSeatState(LobbyManager.SeatState.Inactive);
            }
            else
            {
                UpdateSeatState(lobbyManager.LobbyPlayers[localPlayerIdx].SeatState, lobbyManager.LobbyPlayers[localPlayerIdx].SeatIdx);
            }
        }

        /// <summary>
        /// 로비에서 각 플에이어의 자리(Seat)를 갱신한다.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="seatIdx"></param>
        private void UpdateSeatState(LobbyManager.SeatState state, int seatIdx = -1)
        {
            if (state != LobbyManager.SeatState.Inactive)
            {
                if (state == LobbyManager.SeatState.LockedIn)
                {
                    uiLobby.ConfigureLobbyMode(lobbyManager.IsLobbyClosed.Value ? LobbyMode.LobbyEnding : LobbyMode.Ready);
                }
                else if (state == LobbyManager.SeatState.Active)
                {
                    if (seatIdx == -1) // 신규유저
                    {
                        seatIdx = uiLobby.GetAvailableSeatIdx();
                        lobbyManager.ChangeReadyRpc(NetworkManager.Singleton.LocalClientId, seatIdx, false);
                        hasLocalPlayerLockedIn = false;
                    }
                    else
                    {
                        uiLobby.ConfigureLobbyMode(LobbyMode.Unready);
                    }
                }
            }
            else
            {
                uiLobby.ConfigureLobbyMode(LobbyMode.Unready);
                hasLocalPlayerLockedIn = false;
            }
        }

        private void UpdateSeats()
        {
            LobbyManager.LobbyPlayerState[] curSeats = new LobbyManager.LobbyPlayerState[uiLobby.SeatsCount];
            foreach (var playerState in lobbyManager.LobbyPlayers)
            {
                if (playerState.SeatIdx == -1 || playerState.SeatState == LobbyManager.SeatState.Inactive)
                    continue;
                if (curSeats[playerState.SeatIdx].SeatState == LobbyManager.SeatState.Inactive ||
                    (curSeats[playerState.SeatIdx].SeatState == LobbyManager.SeatState.Active && curSeats[playerState.SeatIdx].LastChangeTime < playerState.LastChangeTime))
                {
                    curSeats[playerState.SeatIdx] = playerState;
                }
            }

            uiLobby.UpdateSeats(curSeats);
        }

        private void OnLobbyClosedChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                uiLobby.ConfigureLobbyMode(LobbyMode.LobbyEnding);
            }
            else
            {
                if (lastSeatSelected == -1)
                {
                    uiLobby.ConfigureLobbyMode(LobbyMode.Unready);
                }
                else
                {
                    uiLobby.ConfigureLobbyMode(LobbyMode.Ready);
                }
            }
        }

        public void OnPlayerClickedReady()
        {
            if (lobbyManager.IsSpawned)
            {
                int localPlayerIdx = -1;
                for (int i = 0; i < lobbyManager.LobbyPlayers.Count; i++)
                {
                    if (lobbyManager.LobbyPlayers[i].ClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        localPlayerIdx = i;
                        break;
                    }
                }

                lobbyManager.ChangeReadyRpc(NetworkManager.Singleton.LocalClientId, lobbyManager.LobbyPlayers[localPlayerIdx].SeatIdx, !hasLocalPlayerLockedIn);
                hasLocalPlayerLockedIn = !hasLocalPlayerLockedIn;
            }
        }
    }
}
