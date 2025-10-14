using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
{
    /// <summary>
    /// 게임 규칙에서 각 라운드에 할 행동
    /// </summary>
    [CreateAssetMenu(menuName = "GameData/Round", order = 5)]
    public class GameRound : ScriptableObject
    {
        public string RoundName;
        public ulong Blind = 10;
        public ulong Ante = 0;
        public int BurnCardCount = 0;
        public int DrawCardCount = 0;
        public List<DealCardInfo> DealCards;
    }
}
