using MessagePipe;
using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.GamePlay.View.PlayObject;
using PlayingCard.GamePlay.View.UI;
using PlayingCard.LobbyManagement.NetModels;
using PlayingCard.Utilities;
using PlayingCard.Utilities.Net;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    /// <summary>
    /// GameRoom Scene에서 한정적으로 사용할 Container 설정
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerGameRoomStateBehaviour : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks netcodeHooks;

        [SerializeField]
        private Transform[] playerSpawnPoints;
        private List<Transform> playerSpawnPointsList = null;

        [SerializeField]
        NetworkObject userPrefab;

        public override GameState ActiveState => GameState.GameRoom;

        [Inject]
        ConnectionManager connectionManager;

        public bool InitialSpawnDone { get; private set; }

        private const float LoseDelay = 2.5f;

        private IPublisher<TableStateMessage> tableStateMessagePublisher;
        private Func<NetworkObject> playerFactory;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            MessagePipeOptions options = Parent.Container.Resolve<MessagePipeOptions>();
            //builder.RegisterMessageBroker<TableStateMessage>(options);

            builder.RegisterMessageBroker<GameRoomInfoMessage>(options);
            builder.RegisterMessageBroker<TurnStartMessage>(options);
            builder.RegisterMessageBroker<TurnActionMessage>(options);
            builder.RegisterMessageBroker<RoundCompleteMessage>(options);
            builder.RegisterMessageBroker<DealCardMessage>(options);
            builder.RegisterMessageBroker<WinnerMessage>(options);
            builder.RegisterMessageBroker<DrawInfoMessage>(options);
            builder.RegisterMessageBroker<DrawCardSelectMessage>(options);
            builder.RegisterMessageBroker<DrawCardsMessage>(options);
            builder.RegisterMessageBroker<DrawResultMessage>(options);


            builder.RegisterFactory<NetworkObject>(container =>
            {
                NetworkObject InstantiateServerPlayer()
                {
                    return container.Instantiate(userPrefab);
                }

                return InstantiateServerPlayer;
            }, Lifetime.Scoped);
        }

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

        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                tableStateMessagePublisher = Container.Resolve<IPublisher<TableStateMessage>>();
                playerFactory = Container.Resolve<Func<NetworkObject>>();

                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
                NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;

                SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
            }
        }

        private void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }

        ///// <summary>
        ///// GameRoom Scene 시작 시 처리할 설정
        ///// </summary>
        //protected override void Start()
        //{
        //    base.Start();

        //    startGamePublisher = Container.Resolve<IPublisher<TableStateMessage>>();
        //    startGamePublisher.Publish(new TableStateMessage(TableStateType.Start));
        //}

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // 클라이언트 중 하나가 연결이 끊겼다. 다른 플레이어들이 플레이 가능한 상태인지 체크
                StartCoroutine(WaitToCheckForGameOver());
            }
        }

        private IEnumerator WaitToCheckForGameOver()
        {
            // 클라이언트가 Despaned 하도록 다음 프레임까지 기다린다.
            yield return null;
            CheckForGameOver();
        }

        private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var networkClient in NetworkManager.Singleton.ConnectedClients)
                {
                    SpawnUser(networkClient.Key, false);
                }
            }
            CheckForGameStart();
        }

        private void OnSynchronizeComplete(ulong clientId)
        {
            if (InitialSpawnDone && ServerPlayerManager.GetServerPlayer(clientId))
            {
                SpawnUser(clientId, true);
            }
        }

        private void SpawnUser(ulong clientId, bool lateJoin)
        {
            Transform spawnPoint = null;

            if (playerSpawnPointsList.IsNullOrEmpty())
            {
                playerSpawnPointsList = new List<Transform>(playerSpawnPoints);
            }

            spawnPoint = playerSpawnPointsList[0];
            playerSpawnPointsList.RemoveAt(0);

            var netUserObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            var netUserExist = netUserObject.TryGetComponent(out NetUser netUser);
            Assert.IsTrue(netUserExist, $"NetUser 매칭 오류, Client {clientId}");
            var netUserGuidExist = netUserObject.TryGetComponent(out NetUserGuidState netUserGuidState);
            Assert.IsTrue(netUserGuidExist, $"netUserGuidState 매칭 오류, Client {clientId}");
            var netUserNameExist = netUserObject.TryGetComponent(out NetworkNameState netUserNameState);
            Assert.IsTrue(netUserNameExist, $"NetworkNameState 매칭 오류, Client {clientId}");
            var netUserChipsExist = netUserObject.TryGetComponent(out NetworkChipsState netUserChipsState);
            Assert.IsTrue(netUserChipsExist, $"NetworkChipsState 매칭 오류, Client {clientId}");

            //var newServerPlayerObject = Instantiate(userPrefab, Vector3.zero, Quaternion.identity);
            var newServerPlayerObject = playerFactory.Invoke();
            var newServerePlayer = newServerPlayerObject.GetComponent<ServerPlayer>();

            if (spawnPoint != null)
            {
                newServerePlayer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
                if (sessionPlayerData is { HasSpawn: true })
                {
                    newServerePlayer.transform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition, sessionPlayerData.Value.PlayerRotation);
                }
            }

            newServerePlayer.NickName.Value = netUserNameState.Name.Value;
            newServerePlayer.Chips.Value = netUserChipsState.Chips.Value;

            newServerPlayerObject.SpawnWithOwnership(clientId);
        }

        private void CheckForGameStart()
        {
            StartCoroutine(ProcessGameStart());
        }

        private IEnumerator ProcessGameStart()
        {
            int spawnedPlayerCount = ServerPlayerManager.GetServerPlayers().Count;
            int joinPlayerCount = NetworkManager.Singleton.ConnectedClientsIds.Count; // 모든 연결된 유저는 게임에 참여한다.

            Debug.Log($"Check Process Start: spawnedPlayerCount:{spawnedPlayerCount},  joinPlayerCount:{joinPlayerCount}");
            while (spawnedPlayerCount != joinPlayerCount)
            {
                yield return null;
            }

            tableStateMessagePublisher = Container.Resolve<IPublisher<TableStateMessage>>();
            tableStateMessagePublisher.Publish(new TableStateMessage(TableStateType.Start));
        }

        private void CheckForGameOver()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            foreach (var serverPlayer in serverPlayers)
            {
                if (serverPlayer && serverPlayer.State.Value != PlayerState.Out)
                {
                    return;
                }
            }

            StartCoroutine(ProcessGameOver(LoseDelay, false));
        }

        private IEnumerator ProcessGameOver(float wait, bool isWin)
        {
            // 게임 관리자에 승패 지정

            yield return new WaitForSeconds(wait);

            SceneLoaderWarpper.Instance.LoadScene(DefineScene.MAIN_MENU, useNetworkSceneManager: false);
        }
    }
}
