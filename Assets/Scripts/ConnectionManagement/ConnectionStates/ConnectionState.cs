using MessagePipe;
using Unity.Netcode;
using VContainer;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// 연결 상태를 나타내는 기본 클래스
    /// </summary>
    public abstract class ConnectionState
    {
        [Inject]
        protected ConnectionManager connectionManager;

        [Inject]
        protected IPublisher<ConnectStatus> connectStatusPublisher;

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }

        public virtual void OnServerStarted() { }

        public virtual void StartClientIP(string playerName, string ipAddress, ushort port, params string[] values) { }

        public virtual void StartHostIP(string playerName, string ipAddress, ushort port, params string[] values) { }

        public virtual void OnUserRequestedShutdown() { }

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }

        public virtual void OnTransportFailure() { }

        public virtual void OnServerStopped() { }
    }
}
