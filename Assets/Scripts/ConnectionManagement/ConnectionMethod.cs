using Cysharp.Threading.Tasks;
using PlayingCard.Utilities;
using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace PlayingCard.ConnectionManagement
{
    public abstract class ConnectionMethodBase
    {
        protected ConnectionManager connectionManager;
        readonly ProfileManager profileManager;
        protected readonly string playerName;

        protected const string DtlsConnType = "dtls";

        /// <summary>
        /// NetworkManager를 시작하기 전에 호스트 연결을 설정합니다.
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupHostConnectionAsync();

        /// <summary>
        /// NetworkManager를 시작하기 전에 클라이언트 연결을 설정합니다.
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupClientConnectionAsync();

        public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        public ConnectionMethodBase(ConnectionManager connectionManager, ProfileManager profileManager, string playerName)
        {
            this.connectionManager = connectionManager;
            this.profileManager = profileManager;
            this.playerName = playerName;
        }

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                PlayerId = playerId,
                PlayerName = playerName,
                IsDebug = Debug.isDebugBuild
            });

            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            connectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        protected string GetPlayerId()
        {
            return ClientPrefs.GetGuid() + profileManager.Profile;
        }
    }

    internal class ConnectionMethodIP : ConnectionMethodBase
    {
        string IpAddress;
        ushort Port;

        public ConnectionMethodIP(string ip, ushort port, ConnectionManager connectionManager, ProfileManager profileManager, string playerName) : base(connectionManager, profileManager, playerName)
        {
            IpAddress = ip;
            Port = port;
        }

        public override async Task SetupClientConnectionAsync()
        {
            await Task.Yield();
            SetConnectionPayload(GetPlayerId(), playerName);
            var utp = (UnityTransport)connectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(IpAddress, Port);
        }

        public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
        {
            await Task.Yield();
            return (true, true);
        }

        public override async Task SetupHostConnectionAsync()
        {
            await Task.Yield();
            SetConnectionPayload(GetPlayerId(), playerName); // 호스트도 클라이언트이므로, 호스트에 대해서도 연결 페이로드를 설정해야 합니다.
            var utp = (UnityTransport)connectionManager.NetworkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(IpAddress, Port);
        }
    }
}
