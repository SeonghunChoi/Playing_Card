using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.Utilities
{
    public interface ISessionPlayerData
    {
        bool IsConnected { get; set; }
        ulong ClientID { get; set; }
        void Reinitialize();
    }

    /// <summary>
    /// 이 클래스는 고유한 플레이어 ID를 사용하여 플레이어를 세션에 바인딩.
    /// 플레이어가 호스트에 연결되면, 호스트는 현재 clientID를 해당 플레이어의 고유 ID에 연결.
    /// 플레이어가 연결을 끊었다가 동일한 호스트에 다시 연결하면 세션이 유지.
    /// </summary>
    /// <remarks>
    /// 클라이언트가 생성한 플레이어 ID를 직접 전송하는 것은 문제가 될 수 있다.
    /// 악의적인 사용자가 이를 가로채고 원래 사용자를 가장하는 데 재사용할 수 있기 때문.
    /// 보안에 대한 방법을 고려해야함.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class SessionManager<T> where T : struct, ISessionPlayerData
    {
        SessionManager()
        {
            clientDatas = new Dictionary<string, T>();
            clientIDToplayerID = new Dictionary<ulong, string>();
        }

        private static SessionManager<T> instance;

        public static SessionManager<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SessionManager<T>();
                }

                return instance;
            }
        }

        /// <summary>
        /// 주어진 클라이언트 플레이어 ID를 해당 클라이언트 플레이어의 데이터에 매핑.
        /// </summary>
        Dictionary<string, T> clientDatas;

        /// <summary>
        /// 플레이어 ID를 플레이어 데이터에 저비용으로 매핑할 수 있도록 해주는 매핑.
        /// </summary>
        Dictionary<ulong, string> clientIDToplayerID;

        bool hasSessionStarted;

        /// <summary>
        /// 클라이언트 연결 해제 처리
        /// </summary>
        /// <param name="clientID"></param>
        public void DisconnectClient(ulong clientID)
        {
            if (hasSessionStarted)
            {
                // 클라이언트를 연결 해제된 상태로 표시하지만, 재접속할 수 있도록 데이터를 유지.
                if (clientIDToplayerID.TryGetValue(clientID, out var playerID))
                {
                    var playerData = GetPlayerData(playerID);
                    if (playerData != null && playerData.Value.ClientID == clientID)
                    {
                        var clientData = clientDatas[playerID];
                        clientData.IsConnected = false;
                        clientDatas[playerID] = clientData;
                    }
                }
            }
            else
            {
                // 세션이 시작되지 않았으므로, 데이터를 유지할 필요가 없다.
                if (clientIDToplayerID.TryGetValue(clientID, out var playerID))
                {
                    clientIDToplayerID.Remove(clientID);
                    var playerData = GetPlayerData(playerID);
                    if (playerData != null && playerData.Value.ClientID == clientID)
                    {
                        clientDatas.Remove(playerID);
                    }
                }
            }
        }

        /// <summary>
        /// 지정된 playerID가 이미 연결되어 있는지 확인.
        /// </summary>
        /// <param name="playerID">이 클라이언트에 고유하며 동일한 클라이언트에서 여러 번 로그인해도 유지되는 playerID.</param>
        /// <returns>이 ID를 가진 플레이어가 이미 연결되어 있는 경우 true를 반환.</returns>
        public bool IsDuplicateConnection(string playerID)
        {
            return clientDatas.ContainsKey(playerID) && clientDatas[playerID].IsConnected;
        }

        /// <summary>
        /// 새 연결인 경우 연결 중인 플레이어의 세션 데이터를 추가하고, 재접속인 경우 세션 데이터를 업데이트한다.
        /// </summary>
        /// <param name="clientID">로그인 시 Netcode가 할당한 clientID. 동일한 클라이언트에서 여러 번 로그인해도 유지되지 않는다.</param>
        /// <param name="playerID">이 클라이언트에 고유하며, 동일한 클라이언트에서 여러 번 로그인해도 유지되는 playerID.</param>
        /// <param name="sessionPlayerData">플레이어의 초기 데이터.</param>
        public void SetupConnectoinPlayerSessionData(ulong clientID, string playerID, T sessionPlayerData)
        {
            var isReconnecting = false;

            // 중복 연결 확인
            if (IsDuplicateConnection(playerID))
            {
                Debug.LogError($"Player ID {playerID} already exists. This is a duplicate connection. Rejecting this session data.");
                return;
            }

            // 동일한 playerId를 가진 다른 클라이언트가 존재하는 경우
            if (clientDatas.ContainsKey(playerID))
            {
                if (clientDatas[playerID].IsConnected)
                {
                    // 이 연결 중인 클라이언트가 연결이 끊긴 클라이언트와 동일한 playerId를 가지고 있다면, 이는 재접속으로 판단한다.
                    isReconnecting = true;
                }
            }

            // 재접속 중. 이전 플레이어의 데이터를 새로운 플레이어에게 전달.
            if (isReconnecting)
            {
                // 플레이어 세션 데이터를 업데이트한다.
                sessionPlayerData = clientDatas[playerID];
                sessionPlayerData.ClientID = clientID;
                sessionPlayerData.IsConnected = true;
            }

            // SessionPlayerData를 맵에 매핑한다.
            clientIDToplayerID[clientID] = playerID;
            clientDatas[playerID] = sessionPlayerData;
        }

        /// <summary>
        /// 지정된 clientID에 해당하는 플레이어 ID를 반환한다.
        /// </summary>
        /// <remarks>
        /// 지정된 clientID에 대한 플레이어 ID를 찾을 수 없는 경우 디버그 메시지를 기록합니다.
        /// </remarks>
        /// <param name="clientId">데이터를 요청한 클라이언트의 ID.</param>
        /// <returns>주어진 clientID에 매칭되는 플레이어 ID를 반환하며, 그렇지 않으면 <see langword="null"/>을 반환한다.</returns>
        public string GetPlayerID(ulong clientID)
        {
            if (clientIDToplayerID.TryGetValue(clientID, out var playerID))
            {
                return playerID;
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientID}");
            return null;
        }

        /// <summary>
        /// 지정된 clientID에 해당하는 플레이어 데이터를 반환한다.
        /// </summary>
        /// <remarks>
        /// 지정된 clientID에 대한 플레이어 데이터를 찾을 수 없는 경우 디버그 메시지를 기록합니다.
        /// </remarks>
        /// <param name="clientId">데이터를 요청한 클라이언트의 ID.</param>
        /// <returns>주어진 ID에 매칭되는 플레이어 데이터를 찾은 경우 <typeparamref name="T"/> 형식의 플레이어 데이터를 반환하며, 그렇지 않으면 <see langword="null"/>을 반환한다.</returns>
        public T? GetPlayerData(ulong clientID)
        {
            var playerID = GetPlayerID(clientID);
            if (!string.IsNullOrEmpty(playerID))
            {
                return GetPlayerData(playerID);
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientID}");
            return null;
        }

        /// <summary>
        /// 지정된 playerID에 해당하는 플레이어 데이터를 반환한다.
        /// </summary>
        /// <remarks>
        /// 지정된 playerID에 대한 플레이어 데이터를 찾을 수 없는 경우 디버그 메시지를 기록한다.
        /// </remarks>
        /// <param name="playerID">데이터를 요청한 플레이어의 ID.</param>
        /// <returns>주어진 ID에 매칭되는 플레이어 데이터를 찾은 경우 <typeparamref name="T"/> 형식의 플레이어 데이터를 반환하며, 그렇지 않으면 <see langword="null"/>을 반환한다.</returns>
        public T? GetPlayerData(string playerID)
        {
            if (clientDatas.TryGetValue(playerID, out T sessionPlayerData))
            {
                return sessionPlayerData;
            }

            Debug.Log($"No PlayerData of matching player ID found: {playerID}");
            return null;
        }

        /// <summary>
        /// 지정된 클라이언트 ID에 연결된 플레이어의 세션 데이터를 업데이트한다.
        /// </summary>
        /// <remarks>
        /// 지정된 <paramref name="clientID"/>가 매핑에서 발견되지 않으면 오류 메시지를 기록한다.
        /// </remarks>
        /// <param name="clientID">플레이어 데이터를 업데이트할 클라이언트 ID.</param>
        /// <param name="sessionPlayerData">기존 데이터를 덮어쓸 새로운 세션 데이터.</param>
        public void SetPlayerData(ulong clientID, T sessionPlayerData)
        {
            if (clientIDToplayerID.TryGetValue(clientID, out string playerID))
            {
                clientDatas[playerID] = sessionPlayerData;
            }
            else
            {
                Debug.LogError($"No client player ID found mapped to the given client ID: {clientID}");
            }
        }

        /// <summary>
        /// 현재 세션을 시작된 것으로 표시하여, 이후부터는 연결이 끊긴 플레이어의 데이터를 유지함.
        /// </summary>
        public void OnSessionStarted()
        {
            hasSessionStarted = true;
        }

        /// <summary>
        /// 연결된 플레이어의 세션 데이터를 다시 초기화하고, 연결이 끊긴 플레이어의 데이터를 삭제하여 다음 게임에서 재접속할 경우 새로운 플레이어로 처리되도록 함.
        /// </summary>
        public void OnSessionEnded()
        {
            ClearDisconnectedPlayersData();
            ReinitializePlayersData();
            hasSessionStarted = false;
        }

        /// <summary>
        /// 모든 런타임 상태를 초기화하여 새 서버를 시작할 준비를 한다.
        /// </summary>
        public void OnServerEnded()
        {
            clientDatas.Clear();
            clientIDToplayerID.Clear();
            hasSessionStarted = false;
        }

        /// <summary>
        /// 연결된 플레이어의 세션 데이터를 다시 초기화한다.
        /// </summary>
        void ReinitializePlayersData()
        {
            foreach (var id in clientIDToplayerID.Keys)
            {
                string playerID = clientIDToplayerID[id];
                T sesseionPlayerData = clientDatas[playerID];
                sesseionPlayerData.Reinitialize();
                clientDatas[playerID] = sesseionPlayerData;
            }
        }

        /// <summary>
        /// 연결이 끊긴 플레이어의 데이터를 삭제함.
        /// </summary>
        void ClearDisconnectedPlayersData()
        {
            List<ulong> idsToClear = new List<ulong>();
            foreach (var id in clientIDToplayerID.Keys)
            {
                var data = GetPlayerData(id);
                if (data is { IsConnected: false })
                {
                    idsToClear.Add(id);
                }
            }

            foreach (var id in idsToClear)
            {
                string playerID = clientIDToplayerID[id];
                var playerData = GetPlayerData(playerID);
                if (playerData != null && playerData.Value.ClientID == id)
                {
                    clientDatas.Remove(playerID);
                }

                clientIDToplayerID.Remove(id);
            }
        }
    }
}
