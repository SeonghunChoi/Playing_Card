using UnityEngine;

namespace PlayingCard.GamePlay.PlayObject
{
    /// <summary>
    /// GameObject 카드 들을 관리
    /// </summary>
    public class ObjectPlayer : MonoBehaviour
    {
        public int Id;

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
        /// <param name="isHand"></param>
        public void AddCard(GameObject objCard, bool isHand)
        {
            if (isHand)
                objCard.transform.SetParent(trHandPoint, false);
            else
                objCard.transform.SetParent(trBoardPoint, false);
            objCard.transform.localScale = Vector3.one;
            objCard.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 카드들 위치를 지정한다.
        /// </summary>
        public void SetCardPosition()
        {
            ObjectTable.SetCardPosition(trBoardPoint);
            ObjectTable.SetCardPosition(trHandPoint);
        }
    }
}
