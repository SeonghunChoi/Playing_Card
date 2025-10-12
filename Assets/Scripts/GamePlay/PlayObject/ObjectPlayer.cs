using UnityEngine;

namespace PlayingCard.GamePlay.PlayObject
{
    /// <summary>
    /// GameObject 카드 들을 관리
    /// </summary>
    public class ObjectPlayer : MonoBehaviour
    {
        public int Id;

        [SerializeField]
        Transform trCameraPosition;

        [SerializeField]
        Transform trBardPoint;

        [SerializeField]
        Transform trHandPoint;

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
                objCard.transform.SetParent(trBardPoint, false);
            objCard.transform.localScale = Vector3.one;
            objCard.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 카드들 위치를 지정한다.
        /// </summary>
        public void SetCardPosition()
        {
            ObjectTable.SetCardPosition(trBardPoint);
            ObjectTable.SetCardPosition(trHandPoint);
        }
    }
}
