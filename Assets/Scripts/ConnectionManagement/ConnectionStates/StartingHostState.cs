using PlayingCard.Infrastructure;
using PlayingCard.Utilities;
using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// 호스트가 시작되는 상태에 해당하는 연결 상태입니다. 이 상태에 진입하면 호스트를 시작합니다.
    /// 성공하면 Hosting 상태로 전환되고, 실패하면 Offline 상태로 되돌아갑니다.
    /// </summary>
    public class StartingHostState : OnlineState
    {
        ConnectionMethodBase connectionMethod;

        public StartingHostState Configure(ConnectionMethodBase connectionMethod)
        {
            this.connectionMethod = connectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit() { }

        public override void OnServerStarted()
        {
            connectStatusPublisher.Publish(ConnectStatus.Success);
            connectionManager.ChangeState(connectionManager.hostingState);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            // 호스트로 시작할 때, StartHost 호출이 끝나기 전에 발생합니다. 이 경우, 단순히 자신을 승인합니다.
            if (clientId == connectionManager.NetworkManager.LocalClientId)
            {
                var payload = Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
                var sessionPlayerData = new SessionPlayerData(clientId, connectionPayload.PlayerName, new NetworkGuid(), 0, true);

                SessionManager<SessionPlayerData>.Instance.SetupConnectoinPlayerSessionData(clientId, connectionPayload.PlayerId, sessionPlayerData);

                // 연결 승인후 플레이어 오브젝트를 생성해줍니다.
                response.Approved = true;
                response.CreatePlayerObject = true;
            }
        }

        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        async void StartHost()
        {
            try
            {
                await connectionMethod.SetupHostConnectionAsync();

                // NGO(Netcode for GameObjects)의 StartHost가 모든 것을 시작합니다.
                if (!connectionManager.NetworkManager.StartHost())
                {
                    StartHostFailed();
                }
            }
            catch (Exception ex)
            {
                StartHostFailed();
                throw ex;
            }
        }

        void StartHostFailed()
        {
            connectStatusPublisher.Publish(ConnectStatus.StartHostFailed);
            connectionManager.ChangeState(connectionManager.offlineState);
        }
    }
}
