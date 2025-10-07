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
        [MinValue(1)]
        public int MinPlayer = 1;
        public int MaxPlayer = 2;
        public int InitialCardsCount = 0;
        public List<GameRound> Rounds;
    }
}
