using Unity.Collections;
using Unity.Netcode;

namespace PlayingCard.ConnectionManagement.Message
{
    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;
        public FixedNickname PlayerNickname;
    }

    public struct FixedNickname : INetworkSerializable
    {
        FixedString32Bytes nickname;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref nickname);
        }

        public override string ToString()
        {
            return nickname.Value.ToString();
        }

        public static implicit operator string(FixedNickname s) => s.ToString();
        public static implicit operator FixedNickname(string s) => new FixedNickname() { nickname = new FixedString32Bytes(s) };
    }
}
