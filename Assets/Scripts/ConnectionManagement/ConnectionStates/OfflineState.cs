using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.Utilities;
using UnityEngine.SceneManagement;
using VContainer;

namespace PlayingCard.ConnectionManagement.ConnectionStates
{
    /// <summary>
    /// NetworkManager가 종료된 상태에 해당하는 연결 상태입니다. 
    /// 이 상태에서는 클라이언트로 시작할 경우 ClientConnecting 상태로, 
    /// 호스트로 시작할 경우 StartingHost 상태로 전환할 수 있습니다.
    /// </summary>
    public class OfflineState : ConnectionState
    {
        [Inject]
        ProfileManager profileManager;

        public override void Enter()
        {
            connectionManager.NetworkManager.Shutdown();
            if (SceneManager.GetActiveScene().name != DefineScene.MAIN_MENU)
            {
                SceneLoaderWarpper.Instance.LoadScene(DefineScene.MAIN_MENU, useNetworkSceneManager: false);
            }
        }

        public override void Exit() { }

        public override void StartClientIP(string playerName, string ipAddress, ushort port, params string[] values)
        {
            var connectionMethod = new ConnectionMethodIP(ipAddress, port, connectionManager, profileManager, playerName, values);
            connectionManager.clientReconnectingState.Configure(connectionMethod);
            connectionManager.ChangeState(connectionManager.clientConnectingState.Configure(connectionMethod));
        }

        public override void StartHostIP(string playerName, string ipAddress, ushort port, params string[] values)
        {
            var connectionMethod = new ConnectionMethodIP(ipAddress, port, connectionManager, profileManager, playerName, values);
            connectionManager.ChangeState(connectionManager.startingHostState.Configure(connectionMethod));
        }
    }
}
