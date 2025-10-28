using PlayingCard.GamePlay.Model.PlayModels;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.Configuration
{
    /// <summary>
    /// 카드 수트 별 정보, 게임에 사용할 카드를 설정한다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameData/Suit", order = 3)]
    public class GameSuits : ScriptableObject
    {
        /// <summary>
        /// 수트 종류
        /// </summary>
        public Suit SuitType;

        /// <summary>
        /// 현재 설정한 수트의 중복 벌수
        /// </summary>
        [Min(1)]
        public int multiply = 1;

        /// <summary>
        /// 수트의 카드 범위 중 가장 작은 값의 카드
        /// </summary>
        [Min(1)]
        public int minValue = 1;

        /// <summary>
        /// 수트의 카드 범위 중 가장 큰 값의 카드
        /// </summary>
        [Min(14)]
        public int maxValue = 14;
    }
}
