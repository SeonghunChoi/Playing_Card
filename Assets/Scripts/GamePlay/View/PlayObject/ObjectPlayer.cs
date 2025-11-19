using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayingCard.GamePlay.View.PlayObject
{
    /// <summary>
    /// GameObject 카드 들을 관리
    /// </summary>
    public class ObjectPlayer : MonoBehaviour
    {
        public ulong ClientId;

        public Vector3 camPosition
        {
            get { return trCameraPosition.position; }
        }
        public Quaternion camRotation
        {
            get { return trCameraPosition.rotation; }
        }

        [SerializeField]
        Transform trCameraPosition;

        [SerializeField]
        Transform trBoardPoint;

        [SerializeField]
        Transform trHandPoint;

        public void ResetGame()
        {
            for (int i = 0; i < trBoardPoint.childCount; i++)
            {
                var child = trBoardPoint.GetChild(i);
                Destroy(child.gameObject);
            }
            for (int i = 0; i < trHandPoint.childCount; i++)
            {
                var child = trHandPoint.GetChild(i);
                Destroy(child.gameObject);
            }
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
    }
}
