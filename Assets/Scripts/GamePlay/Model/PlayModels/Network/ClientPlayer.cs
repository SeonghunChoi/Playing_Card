using Cysharp.Threading.Tasks;
using PlayingCard.GamePlay.View.PlayObject;
using Unity.Netcode;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    public class ClientPlayer : NetworkBehaviour
    {

        [SerializeField]
        Transform trCameraPosition;

        [SerializeField]
        Transform trBoardPoint;
        [SerializeField]
        Transform trHandPoint;

        public Transform CameraPosition => trCameraPosition;

        public Transform BoardPoint => trBoardPoint;
        public Transform HandPoint => trHandPoint;

        ServerPlayer serverPlayer;
        public ServerPlayer ServerPlayer
        {
            get { return serverPlayer; }
        }

        private void Awake()
        {
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient) return;

            enabled = true;

            serverPlayer = GetComponentInParent<ServerPlayer>();

            if (serverPlayer != null && serverPlayer.OwnerClientId == NetworkManager.Singleton.LocalClientId )
            {
                Camera.main.transform.SetPositionAndRotation(trCameraPosition.position, trCameraPosition.rotation);
            }
            transform.SetPositionAndRotation(serverPlayer.transform.position, serverPlayer.transform.rotation);
        }

        public override void OnNetworkDespawn()
        {
        }

        public bool HasHands(ObjectCard card)
        {
            bool reslut = false;
            int count = trHandPoint.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = trHandPoint.GetChild(i);
                var objCard = child.GetComponent<ObjectCard>();

                if (objCard == card)
                {
                    reslut = true;
                    break;
                }
            }
            return reslut;
        }

        public async UniTask RemoveDrawAsync()
        {
            int count = trHandPoint.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                var child = trHandPoint.GetChild(i);
                var objCard = child.GetComponent<ObjectCard>();
                if (objCard.IsRim)
                {
                    Destroy(child.gameObject);
                }
            }
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        /// <summary>
        /// 카드들 위치를 지정한다.
        /// </summary>
        public void SetCardPosition()
        {
            NetworkGameTable.SetCardPosition(trBoardPoint);
            NetworkGameTable.SetCardPosition(trHandPoint);
        }

        /// <summary>
        /// 카드 프리팹을 받아 Container 에 등록한다.
        /// </summary>
        /// <param name="objCard"></param>
        /// <param name="isBoard"></param>
        public void AddCard(GameObject objCard, bool isBoard)
        {
            if (isBoard)
                objCard.transform.SetParent(trBoardPoint, false);
            else
                objCard.transform.SetParent(trHandPoint, false);
            objCard.transform.localScale = Vector3.one;
            objCard.transform.localPosition = Vector3.zero;
        }

        public void ClearHands()
        {
            int count = trBoardPoint.childCount;
            for (int i = 0; i < count; i++)
            {
                var card = trBoardPoint.GetChild(i);
                Destroy(card.gameObject);
            }
            count = trHandPoint.childCount;
            for (int i = 0; i < count; i++)
            {
                var hand = trHandPoint.GetChild(i);
                Destroy(hand.gameObject);
            }
        }
    }
}
