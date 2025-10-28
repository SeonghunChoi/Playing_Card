using UnityEngine;

namespace PlayingCard.GamePlay.Model.Configuration
{
    /// <summary>
    /// 라운드에서 카드를 나눠줄 대상
    /// </summary>
    public enum DealTarget
    {
        Player,
        Table
    }

    /// <summary>
    /// 라운드에서 카드를 나눌때 카드 방향
    /// </summary>
    public enum DealFace
    {
        FaceUp,
        FaceDown,
    }

    [CreateAssetMenu(menuName = "GameData/DealCard", order = 6)]
    public class DealCardInfo : ScriptableObject
    {
        public DealTarget DealTarget = DealTarget.Player;
        public DealFace DealFace = DealFace.FaceUp;
    }
}
