using System;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    public class ClientConnectingState : OnlineState
    {
        protected ConnectionMethodBase connectionMethod;

        public ClientConnectingState Configure(ConnectionMethodBase connectionMethod)
        {
            this.connectionMethod = connectionMethod;
            return this;
        }

        public override void Enter()
        {
#pragma warning disable 4014
            ConnectClientAsync();
#pragma warning restore 4014
        }

        public override void Exit() { }

        public override void OnClientConnected(ulong clientId)
        {
            connectStatusPublisher.Publish(ConnectStatus.Success);
            connectionManager.ChangeState(connectionManager.clientConnectedState);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            StartingClientFailed();
        }

        void StartingClientFailed()
        {
            var disconnectReason = connectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                connectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                connectStatusPublisher.Publish(connectStatus);
            }
            connectionManager.ChangeState(connectionManager.offlineState);
        }

        internal async Task ConnectClientAsync()
        {
            try
            {
                // 현재 연결 방식으로 NGO(Netcode for GameObjects)를 설정합니다.
                await connectionMethod.SetupClientConnectionAsync();

                // NGO(Netcode for GameObjects)의 StartClient가 모든 것을 시작합니다.
                if (!connectionManager.NetworkManager.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailed();
                throw;
            }
        }
    }
}
