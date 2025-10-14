using PlayingCard.GamePlay.PlayModels;
using UnityEngine;

namespace PlayingCard.GamePlay.PlayObject
{
    public class ObjectCard : MonoBehaviour
    {
        public Suit Suit;
        public Rank Rank;
        public bool IsWild;

        public bool IsRim => rimEfect.activeSelf;

        [SerializeField]
        GameObject rimEfect;

        public void SetRim(bool isOn)
        {
            rimEfect.SetActive(isOn);
        }
    }
}
