using MessagePipe;
using PlayingCard.ConnectionManagement.Message;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.Utilities;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// 클라이언트 연결을 수신하는 호스트에 해당하는 연결 상태입니다.
    /// 종료되거나 타임아웃될 경우 Offline 상태로 전환됩니다.
    /// </summary>
    public class HostingState : OnlineState
    {
        [Inject]
        IPublisher<ConnectionEventMessage> connectionEventPublisher;

        // ApprovalCheck에서 사용됩니다. 이는 말도 안 되게 큰 쓰레기 버퍼를 보내는 방식의 DOS 공격에 대한 가벼운 보호 장치다.
        const int MaxConnectPayload = 1024;

        public override void Enter()
        {
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.GAME_LOBBY, useNetworkSceneManager: true);
        }

        public override void Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        }

        public override void OnClientConnected(ulong clientID)
        {
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientID);
            if (playerData != null)
            {
                var connectionEventMessage = new ConnectionEventMessage() { ConnectStatus = ConnectStatus.Success, PlayerNickname = playerData.Value.PlayerName };
                connectionEventPublisher.Publish(connectionEventMessage);
            }
            else
            {
                Debug.LogError($"No Player data associated with client {clientID}");
                var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
                connectionManager.NetworkManager.DisconnectClient(clientID, reason);
            }
        }

        public override void OnClientDisconnect(ulong clientID)
        {
            if (clientID != connectionManager.NetworkManager.LocalClientId)
            {
                var playerID = SessionManager<SessionPlayerData>.Instance.GetPlayerID(clientID);
                if (playerID != null)
                {
                    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerID);
                    if (sessionData.HasValue)
                    {
                        var connectionEventMessage = new ConnectionEventMessage() { ConnectStatus = ConnectStatus.GenericDisconnect, PlayerNickname = sessionData.Value.PlayerName };
                        connectionEventPublisher.Publish(connectionEventMessage);
                    }
                    SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientID);
                }
            }
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
            for (var i = connectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                var id = connectionManager.NetworkManager.ConnectedClientsIds[i];
                if (id != connectionManager.NetworkManager.LocalClientId)
                {
                    connectionManager.NetworkManager.DisconnectClient(id, reason);
                }
            }
            connectionManager.ChangeState(connectionManager.offlineState);
        }

        public override void OnServerStopped()
        {
            connectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
            connectionManager.ChangeState(connectionManager.offlineState);
        }

        /// <summary>
        /// 이 로직은 Netcode.NetworkManager에서 제공하는 "ConnectionApprovalResponse"에 연결됨.
        /// 클라이언트가 연결할 때마다 실행.
        /// 클라이언트가 연결을 시작할 때 실행되는 보완 로직은 ClientConnectingState에서 확인.
        /// </summary>
        /// <param name="request">
        /// 클라이언트의 GUID가 포함되며, 이는 게임 설치에 고유한 식별자로 앱을 재시작해도 유지된다.
        /// </param>
        /// <param name="response">
        /// 승인 프로세스에 대한 응답입니다.
        /// 연결이 거부되고 커스텀 메시지를 반환해야 하는 경우, Pending 필드를 사용하여 지연시킬 수 있습니다.
        /// </param>
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientID = request.ClientNetworkId;
            if (connectionData.Length > MaxConnectPayload)
            {
                // connectionData가 너무 크면 서버의 시간을 낭비하지 않도록 즉시 연결을 거부한다.
                response.Approved = false;
                return;
            }

            var payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                var sessionPlayerData = new SessionPlayerData(clientID, connectionPayload.PlayerName, new Infrastructure.NetworkGuid(), 0, true);
                SessionManager<SessionPlayerData>.Instance.SetupConnectoinPlayerSessionData(clientID, connectionPayload.PlayerId, sessionPlayerData);

                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                return;
            }

            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
        }

        ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (connectionManager.NetworkManager.ConnectedClientsIds.Count >= connectionManager.MaxConnectedPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            if (connectionPayload.IsDebug != Debug.isDebugBuild)
            {
                return ConnectStatus.IncompatibleBuildType;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.PlayerId) ?
                ConnectStatus.LoggedInAgain : ConnectStatus.Success;
        }
    }
}
