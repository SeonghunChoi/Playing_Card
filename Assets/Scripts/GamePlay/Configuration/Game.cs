using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
{
    /// <summary>
    /// 메인 메뉴 게임 설정 정보
    /// 게임에 사용하는 카드 정보와
    /// 게임에 적용할 룰 정보를 가진다.
    /// </summary>
    [CreateAssetMenu(menuName ="GameData/Game", order = 1)]
    public class Game : ScriptableObject
    {
        [MinValue(1)]
        public int GameId = 1;
        public string GameName;
        /// <summary>
        /// 카드 정보
        /// </summary>
        public GameDeck Deck;
        /// <summary>
        /// 룰 정보
        /// </summary>
        public GameRule Rule;
    }
}
