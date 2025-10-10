using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
{
    [CreateAssetMenu(menuName ="GameData/Game", order = 1)]
    public class Game : ScriptableObject
    {
        [MinValue(1)]
        public int GameId = 1;
        public string GameName;
        public GameDeck Deck;
        public GameRule Rule;
    }
}
