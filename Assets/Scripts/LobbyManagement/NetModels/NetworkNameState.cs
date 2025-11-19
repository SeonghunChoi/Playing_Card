using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    public class NetworkNameState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<NetworkString> Name = new NetworkVariable<NetworkString>();
    }

    public struct NetworkString : INetworkSerializable
    {
        FixedString32Bytes networkString;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref networkString);
        }

        public override string ToString()
        {
            return networkString.Value.ToString();
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString() { networkString = new FixedString32Bytes(s) };
    }
}
