using UnityEngine;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    public class ClientConnectedState : OnlineState
    {
        public override void Enter() { }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong clientId)
        {
            var disconnectReason = connectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                connectStatusPublisher.Publish(ConnectStatus.Reconnecting);
                connectionManager.ChangeState(connectionManager.clientReconnectingState);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                connectStatusPublisher.Publish(connectStatus);
                connectionManager.ChangeState(connectionManager.offlineState);
            }
        }
    }
}
