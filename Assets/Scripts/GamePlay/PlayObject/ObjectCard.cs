using UnityEngine;

namespace PlayingCard.GamePlay.PlayObject
{
    public class ObjectCard : MonoBehaviour
    {
        PlayModels.Card card;

        public void Set(PlayModels.Card card)
        {
            this.card = card;
        }
    }
}
