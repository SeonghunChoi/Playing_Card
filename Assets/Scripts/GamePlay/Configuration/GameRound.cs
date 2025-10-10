using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
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

    /// <summary>
    /// 게임 규칙에서 각 라운드에 할 행동
    /// </summary>
    [CreateAssetMenu(menuName = "GameData/Round", order = 5)]
    public class GameRound : ScriptableObject
    {
        public DealTarget DealTarget = DealTarget.Player;
        public DealFace DealFace = DealFace.FaceUp;
        public int DealCardCount = 1;
    }
}
