using PlayingCard.Infrastructure;
using UnityEngine;

namespace PlayingCard.LobbyManagement.NetModels
{
    [CreateAssetMenu]
    public class NetUserRuntimeCollection : RuntimeCollection<NetUser>
    {
        public bool TryGetPlayer(ulong clientID, out NetUser netUser)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (clientID == Items[i].OwnerClientId)
                {
                    netUser = Items[i];
                    return true;
                }
            }

            netUser = null;
            return false;
        }
    }
}
