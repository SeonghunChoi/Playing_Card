using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayingCard.Utilities
{
    public class SceneLoaderWarpper : NetworkBehaviour
    {
        [SerializeField]
        LoadingScreen loadingScreen;

        bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null && NetworkManager.NetworkConfig.EnableSceneManagement;

        bool IsInitialized;

        public static SceneLoaderWarpper Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        public void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (useNetworkSceneManager)
            {
                if (IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
                {
                    if (NetworkManager.IsServer)
                    {
                        NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            }
            else
            {
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    loadingScreen.StartLoadingScreen();
                    loadingScreen.LocalLoadOperation = loadOperation;
                }
            }
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (NetworkManager != null)
            {
                NetworkManager.OnServerStarted += OnNetworkingStarted;
                NetworkManager.OnClientStarted += OnNetworkingStarted;
                NetworkManager.OnServerStopped += OnNetworkingStopped;
                NetworkManager.OnClientStopped += OnNetworkingStopped; 
            }
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (NetworkManager != null)
            {
                NetworkManager.OnServerStarted -= OnNetworkingStarted;
                NetworkManager.OnClientStarted -= OnNetworkingStarted;
                NetworkManager.OnServerStopped -= OnNetworkingStopped;
                NetworkManager.OnClientStopped -= OnNetworkingStopped;
            }
            base.OnDestroy();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress)
            {
                loadingScreen.StopLoadingScreen();
            }
        }

        void OnNetworkingStarted()
        {
            if (!IsInitialized)
            {
                if (IsNetworkSceneManagementEnabled)
                {
                    NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
                }
                IsInitialized = true;
            }
        }

        void OnNetworkingStopped(bool unused)
        {
            if (IsInitialized)
            {
                if (IsNetworkSceneManagementEnabled)
                {
                    NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                }
                IsInitialized = false;
            }
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load:
                    {
                        if (NetworkManager.IsClient)
                        {
                            if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                            {
                                loadingScreen.StartLoadingScreen();
                            }
                            else
                            {
                                loadingScreen.UpdateLoadingScreen();
                            }
                            loadingScreen.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                    }
                    break;
                case SceneEventType.LoadEventCompleted:
                    {
                        if (NetworkManager.IsClient)
                        {
                            loadingScreen.StopLoadingScreen();
                        }
                    }
                    break;
                case SceneEventType.Synchronize:
                    {
                        if (NetworkManager.IsClient && !NetworkManager.IsHost)
                        {
                            if (NetworkManager.SceneManager.ClientSynchronizationMode == LoadSceneMode.Single)
                            {
                                UnloadAdditiveScenes();
                            }
                        }
                    }
                    break;
                case SceneEventType.SynchronizeComplete:
                    {
                        if (NetworkManager.IsServer)
                        {
                            StopLoadingScreenClientRpc(
                                new ClientRpcParams { 
                                    Send = new ClientRpcSendParams { 
                                        TargetClientIds = new[] { sceneEvent.ClientId } 
                                    } 
                                });
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        void UnloadAdditiveScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [ClientRpc]
        void StopLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
        {
            loadingScreen.StopLoadingScreen();
        }
    }
}
