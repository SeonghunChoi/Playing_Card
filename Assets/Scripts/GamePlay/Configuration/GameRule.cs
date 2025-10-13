using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
{
    /// <summary>
    /// 게임의 룰을 정의 한다. 
    /// 게임 인원, 준비 카드 장수, 라운드별 규칙 등을 설정할 수 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "GameData/Rule", order = 4)]
    public class GameRule : ScriptableObject
    {
#if ODIN_INSPECTOR
        [MinValue(1)]
#else
        [Min(1)]
#endif
        public int MinPlayer = 1;
#if ODIN_INSPECTOR
        [MinValue(2), MaxValue(8)]
#else
        [Range(2,8)]
#endif
        public int MaxPlayer = 2;
        public int InitialCardsCount = 0;
        public ulong MinRaise = 20;
        /// <summary>
        /// 라운드별 룰
        /// </summary>
        public List<GameRound> Rounds;
    }
}
