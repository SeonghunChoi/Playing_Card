using PlayingCard.ConnectionManagement.ConnectionStates;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace PlayingCard.ConnectionManagement
{
    public enum ConnectStatus
    {
        Undefined,
        Success,                    //클라이언트가 성공적으로 연결됨. 재연결 성공도 포함
        ServerFull,                 //서버가 이미 최대 인원에 도달하여 접속할 수 없음
        LoggedInAgain,              //다른 클라이언트에서 다시 로그인하여 이 클라이언트가 강제 종료됨
        UserRequestedDisconnect,    //사용자가 의도적으로 연결을 끊음
        GenericDisconnect,          //서버가 연결을 끊었지만 구체적인 이유는 제공되지 않음
        Reconnecting,               //클라이언트가 연결을 잃고 다시 연결을 시도 중임
        IncompatibleBuildType,      //클라이언트의 빌드 타입이 서버와 호환되지 않음
        HostEndedSession,           //호스트가 세션을 의도적으로 종료함.
        StartHostFailed,            //서버가 바인딩에 실패함
        StartClientFailed           //서버에 연결하지 못했거나 네트워크 엔드포인트가 잘못됨
    }

    /// <summary>
    /// 이 상태 머신은 NetworkManager를 통한 연결을 처리한다.
    /// NetworkManager의 콜백 및 외부 호출을 수신하고, 이를 현재 ConnectionState 객체로 전달하는 역할을 한다.
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        private ConnectionState currentState;

        [Inject]
        NetworkManager networkManager;
        public NetworkManager NetworkManager => networkManager;

        [SerializeField]
        int nbReconnectAttempts = 2;

        public int NbReconnectAttempts => nbReconnectAttempts;

        [Inject]
        IObjectResolver resolver;

        public int MaxConnectedPlayers = 8;

        internal readonly OfflineState offlineState = new OfflineState();
        internal readonly ClientConnectedState clientConnectedState = new ClientConnectedState();
        internal readonly ClientConnectingState clientConnectingState = new ClientConnectingState();
        internal readonly ClientReconnectingState clientReconnectingState = new ClientReconnectingState();
        internal readonly StartingHostState startingHostState = new StartingHostState();
        internal readonly HostingState hostingState = new HostingState();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            List<ConnectionState> states = new List<ConnectionState>();
            foreach (var connectionState in states)
            {
                resolver.Inject(connectionState);
            }

            currentState = offlineState;

            networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            networkManager.OnServerStarted += OnServerStarted;
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnTransportFailure += OnTransportFailure;
            networkManager.OnServerStopped += OnServerStopped;
        }

        private void OnDestroy()
        {
            networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            networkManager.OnServerStarted -= OnServerStarted;
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnTransportFailure -= OnTransportFailure;
            networkManager.OnServerStopped -= OnServerStopped;
        }

        internal void ChangeState(ConnectionState nextState)
        {
            Debug.Log($"{name}: Changed connection state from [{currentState.GetType().Name}] to [{nextState.GetType().Name}]");

            if (currentState != null)
            {
                currentState.Exit();
            }
            currentState = nextState;
            currentState.Enter();
        }

        void OnClientDisconnectCallback(ulong clientId)
        {
            currentState.OnClientDisconnect(clientId);
        }

        void OnClientConnectedCallback(ulong clientId)
        {
            currentState.OnClientConnected(clientId);
        }

        void OnServerStarted()
        {
            currentState.OnServerStarted();
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            currentState.ApprovalCheck(request, response);
        }

        void OnTransportFailure()
        {
            currentState.OnTransportFailure();
        }

        void OnServerStopped(bool _) // 이 매개변수는 필요하지 않습니다. ConnectionState가 이미 관련 정보를 포함하고 있기 때문.
        {
            currentState.OnServerStopped();
        }

        public void StartClientIp(string playerName, string ipaddress, int port)
        {
            currentState.StartClientIP(playerName, ipaddress, (ushort)port);
        }

        public void StartHostIp(string playerName, string ipaddress, int port)
        {
            currentState.StartHostIP(playerName, ipaddress, (ushort)port);
        }

        public void RequestShutdown()
        {
            currentState.OnUserRequestedShutdown();
        }
    }
}
