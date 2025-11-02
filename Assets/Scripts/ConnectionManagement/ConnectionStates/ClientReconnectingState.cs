using MessagePipe;
using PlayingCard.ConnectionManagement.Message;
using System.Collections;
using UnityEngine;
using VContainer;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// 서버에 재접속을 시도하는 클라이언트에 해당하는 연결 상태입니다.
    /// ConnectionManager의 NbReconnectAttempts 속성에 정의된 횟수만큼 재접속을 시도합니다.
    /// 성공하면 ClientConnected 상태로 전환되며, 실패하면 Offline 상태로 전환됩니다.
    /// 먼저 연결 해제 사유가 주어진 경우, 해당 사유에 따라 재접속을 시도하지 않고 바로 Offline 상태로 전환될 수 있습니다.
    /// </summary>
    public class ClientReconnectingState : ClientConnectingState
    {
        [Inject]
        IPublisher<ReconnectMessage> reconnectPublisher;

        Coroutine reconnectCoroutine;
        int NbAttempts;

        const float TimeBeforeFirstAttempt = 1.0f;
        const float TimeBetweenAttempts = 5.0f;

        public override void Enter()
        {
            NbAttempts = 0;
            reconnectCoroutine = connectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (reconnectCoroutine != null)
            {
                connectionManager.StopCoroutine(reconnectCoroutine);
                reconnectCoroutine = null;
            }
            var reconnectMessage = new ReconnectMessage(connectionManager.NbReconnectAttempts, connectionManager.NbReconnectAttempts);
            reconnectPublisher.Publish(reconnectMessage);
        }

        public override void OnClientConnected(ulong clientId)
        {
            connectionManager.ChangeState(connectionManager.clientConnectedState);
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            var disconnectReason = connectionManager.NetworkManager.DisconnectReason;
            if (NbAttempts < connectionManager.NbReconnectAttempts)
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    reconnectCoroutine = connectionManager.StartCoroutine(ReconnectCoroutine());
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    connectStatusPublisher.Publish(connectStatus);
                    switch (connectStatus)
                    {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            connectionManager.ChangeState(connectionManager.offlineState);
                            break;
                        default:
                            reconnectCoroutine = connectionManager.StartCoroutine(ReconnectCoroutine());
                            break;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    connectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    connectStatusPublisher.Publish(connectStatus);
                }
                connectionManager.ChangeState(connectionManager.offlineState);
            }
        }

        IEnumerator ReconnectCoroutine()
        {
            // 첫 번째 시도가 아닌 경우, 다시 시도하기 전에 잠시 대기합니다.
            // 이는 연결 끊김을 유발한 문제가 일시적인 경우, 다시 시도하기 전에 해결될 시간을 주기 위함입니다.
            if (NbAttempts > 0)
            {
                yield return new WaitForSeconds(TimeBetweenAttempts);
            }

            Debug.Log("Lost connection to host, trying to reconnect...");

            connectionManager.NetworkManager.Shutdown();

            yield return new WaitWhile(() => connectionManager.NetworkManager.ShutdownInProgress);
            Debug.Log($"Reconnecting attempt {NbAttempts + 1}/{connectionManager.NbReconnectAttempts}...");
            var reconnectMessage = new ReconnectMessage(NbAttempts, connectionManager.NbReconnectAttempts);
            reconnectPublisher.Publish(reconnectMessage);

            // 첫 번째 시도인 경우, 재접속을 시도하기 전에 잠시 대기하여 서비스가 갱신될 시간을 줍니다.
            if (NbAttempts == 0)
            {
                yield return new WaitForSeconds(TimeBeforeFirstAttempt);
            }

            NbAttempts++;
            var reconnectingSetupTask = connectionMethod.SetupClientReconnectionAsync();
            yield return new WaitUntil(() => reconnectingSetupTask.IsCompleted);

            if (!reconnectingSetupTask.IsFaulted && reconnectingSetupTask.Result.success)
            {
                // 이 작업이 실패하면 Netcode에 의해 OnClientDisconnect 콜백이 호출됩니다.
                var connectingTask = ConnectClientAsync();
                yield return new WaitUntil(() => connectingTask.IsCompleted);
            }
            else
            {
                if (!reconnectingSetupTask.Result.shouldTryAgain)
                {
                    // 더 이상 재시도를 하지 않도록 시도 횟수를 최대값으로 설정합니다.
                    NbAttempts = connectionManager.NbReconnectAttempts;
                }
                // 이 시도를 실패로 표시하기 위해 OnClientDisconnect를 호출하며,
                // 새로운 시도를 시작하거나 포기하고 Offline 상태로 전환합니다.
                OnClientDisconnect(0);
            }
        }
    }
}
