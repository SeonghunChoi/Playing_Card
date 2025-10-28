using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard.GamePlay.Model.Configuration
{

    /// <summary>
    /// 게임에서 사용할 덱 정보
    /// </summary>
    [CreateAssetMenu(menuName = "GameData/Deck", order = 2)]
    public class GameDeck : ScriptableObject
    {
        public int WildCardCount = 0;
        public List<GameSuits> SuitList;
    }
}
