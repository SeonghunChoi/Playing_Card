namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// 온라인 연결 상태를 나타내는 기본 클래스.
    /// </summary>
    public abstract class OnlineState : ConnectionState
    {
        public override void OnUserRequestedShutdown()
        {
            connectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            connectionManager.ChangeState(connectionManager.offlineState);
        }

        public override void OnTransportFailure()
        {
            connectionManager.ChangeState(connectionManager.offlineState);
        }
    }
}
