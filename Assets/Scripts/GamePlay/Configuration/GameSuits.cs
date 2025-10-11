using PlayingCard.GamePlay.PlayModels;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
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
#if ODIN_INSPECTOR
        [MinValue(1)]
#else
        [Min(1)]
#endif
        public int multiply = 1;

        /// <summary>
        /// 수트의 카드 범위 중 가장 작은 값의 카드
        /// </summary>
#if ODIN_INSPECTOR
        [MinValue(1)]
#else
        [Min(1)]
#endif
        public int minValue = 1;

        /// <summary>
        /// 수트의 카드 범위 중 가장 큰 값의 카드
        /// </summary>
#if ODIN_INSPECTOR
        [MaxValue(14)]
#else
        [Min(14)]
#endif
        public int maxValue = 14;
    }
}
