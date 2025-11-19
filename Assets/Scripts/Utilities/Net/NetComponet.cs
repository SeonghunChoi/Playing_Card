using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.Utilities.Net
{
    public static class NetworkComponet
    {
        public static T GetComponentByClientId<T>(ulong clientId) where T : Component
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                var playerObject = client.PlayerObject;
                if (playerObject != null)
                {
                    return playerObject.GetComponent<T>();
                }
            }
            return null;
        }
    }
}
