using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.LobbyManagement;
using PlayingCard.Utilities;
using PlayingCard.Utilities.Net;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    /// <summary>
    /// Lobby Scene 에서 한정적으로 사용할 Container 설정
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerGameLobbyStateBehaviour : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks netcodeHooks;
        [SerializeField]
        LobbyManager lobbyManager;

        public override GameState ActiveState => GameState.Lobby;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(lobbyManager);

            var gameManager = Parent.Container.Resolve<IGameManager>() as GameManager;
            builder.RegisterInstance(gameManager.SelectedGame).WithParameter("SelectedGame");
        }

        Coroutine waitToEndLobbyCoroutine;

        [Inject]
        ConnectionManager connectionManager;

        protected override void Awake()
        {
            base.Awake();

            netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }

        protected override void OnDestroy()
        {
            if (netcodeHooks)
            {
                netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
            base.OnDestroy();
        }

        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;

                lobbyManager.OnClientChangedSeat += OnClientChangedSeat;
                Debug.Log("Server Lobby");
            }
        }

        void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
            if (lobbyManager)
            {
                lobbyManager.OnClientChangedSeat -= OnClientChangedSeat;
            }
        }

        void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
        {
            int idx = FindLobbyPlayerIdx(clientId);

            if (idx == -1)
            {
                throw new Exception($"OnClientChangedSeat: Client ID {clientId}는 로비 플레이어가 아니므로 자리를 변경할 수 없다!");
            }

            if (lobbyManager.IsLobbyClosed.Value)
            {
                return;
            }

            if (newSeatIdx == -1)
            {
                lockedIn = false;
            }
            else
            {
                foreach (var playerInfo in lobbyManager.LobbyPlayers)
                {
                    if (playerInfo.ClientId != clientId && playerInfo.SeatIdx == newSeatIdx && playerInfo.SeatState == LobbyManager.SeatState.LockedIn)
                    {
                        lobbyManager.LobbyPlayers[idx] = new LobbyManager.LobbyPlayerState(
                            clientId,
                            lobbyManager.LobbyPlayers[idx].UserName,
                            lobbyManager.LobbyPlayers[idx].PlayerNumber,
                            LobbyManager.SeatState.Inactive);

                        return;
                    }
                }
            }

            lobbyManager.LobbyPlayers[idx] = new LobbyManager.LobbyPlayerState(
                clientId,
                lobbyManager.LobbyPlayers[idx].UserName,
                lobbyManager.LobbyPlayers[idx].PlayerNumber,
                lockedIn ? LobbyManager.SeatState.LockedIn : LobbyManager.SeatState.Active,
                newSeatIdx,
                Time.time);

            if (lockedIn)
            {
                for (int i = 0; i < lobbyManager.LobbyPlayers.Count; i++)
                {
                    if (lobbyManager.LobbyPlayers[i].SeatIdx == newSeatIdx && i != idx)
                    {
                        lobbyManager.LobbyPlayers[i] = new LobbyManager.LobbyPlayerState(
                            lobbyManager.LobbyPlayers[i].ClientId,
                            lobbyManager.LobbyPlayers[i].UserName,
                            lobbyManager.LobbyPlayers[i].PlayerNumber,
                            LobbyManager.SeatState.Inactive);
                    }
                }
            }

            CloseLobbyIfReady();
        }

        private int FindLobbyPlayerIdx(ulong clientId)
        {
            for (int i = 0; i < lobbyManager.LobbyPlayers.Count; i++)
            {
                if (lobbyManager.LobbyPlayers[i].ClientId == clientId) return i;
            }
            return -1;
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            for (int i = 0; i < lobbyManager.LobbyPlayers.Count; i++)
            {
                if (lobbyManager.LobbyPlayers[i].ClientId == clientId)
                {
                    lobbyManager.LobbyPlayers.RemoveAt(i);
                    break;
                }
            }

            if (!lobbyManager.IsLobbyClosed.Value)
            {
                CloseLobbyIfReady();
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            SeatNewPlayer(sceneEvent.ClientId);
        }

        private void CloseLobbyIfReady()
        {
            // 로비에 한 명 뿐이라면 리턴
            //if (lobbyManager.LobbyPlayers.Count == 1) return;

            foreach (var playerInfo in lobbyManager.LobbyPlayers)
            {
                if (playerInfo.SeatState != LobbyManager.SeatState.LockedIn) return;
            }

            lobbyManager.IsLobbyClosed.Value = true;

            waitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
        }

        IEnumerator WaitToEndLobby()
        {
            yield return new WaitForSeconds(1f);
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.GAME_ROOM, useNetworkSceneManager: true);
        }

        // 새로 들어온 유저를 앉히자.
        void SeatNewPlayer(ulong clientId) 
        {
            if (lobbyManager.IsLobbyClosed.Value)
            {
                CancelCloseLobby();
            }

            SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (sessionPlayerData.HasValue)
            {
                var playerData = sessionPlayerData.Value;
                if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                {
                    // 플레이어 번호가 아직 지정되지 않았거나, 기존 플레이어 번호를 더 이상 사용할 수 없는 경우 사용 가능한 번호를 가져온다.
                    playerData.PlayerNumber = GetAvailablePlayerNumber();
                }
                if (playerData.PlayerNumber == -1)
                {
                    // 점검용. 자리가 모두 찼습니다... 더 이상 공간이 없습니다!
                    throw new Exception($"// 여기에 올 수 없다. Client ID {clientId}와 Player Number {playerData.PlayerNumber}에 대한 연결은 이미 승인 단계(Connection Approval)에서 거부되었어야 한다.");
                }

                lobbyManager.LobbyPlayers.Add(new LobbyManager.LobbyPlayerState(clientId, playerData.PlayerName, playerData.PlayerNumber, LobbyManager.SeatState.Active));
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, playerData);
            }
        }

        private void CancelCloseLobby()
        {
            if (waitToEndLobbyCoroutine != null)
            {
                StopCoroutine(waitToEndLobbyCoroutine);
            }
            lobbyManager.IsLobbyClosed.Value = false;
        }

        private int GetAvailablePlayerNumber()
        {
            for (int playerNumber = 0; playerNumber < connectionManager.MaxConnectedPlayers; playerNumber++)
            {
                if (IsPlayerNumberAvailable(playerNumber))
                {
                    return playerNumber;
                }
            }

            // Lobby 가 가득 찼다.
            return -1;
        }

        private bool IsPlayerNumberAvailable(int playerNumber)
        {
            bool found = false;

            foreach (var playerState in lobbyManager.LobbyPlayers)
            {
                if (playerState.PlayerNumber == playerNumber)
                {
                    found = true;
                    break;
                }
            }

            return !found;
        }
    }
}
