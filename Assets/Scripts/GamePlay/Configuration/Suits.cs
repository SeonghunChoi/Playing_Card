using PlayingCard.GamePlay.PlayModels;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayingCard.GamePlay.Configuration
{
    [CreateAssetMenu(menuName = "GameData/Suit", order = 3)]
    public class Suits : ScriptableObject
    {
        public Suit SuitType;
        [MinValue(1)]
        public int multiply = 1;
        [MinValue(1)]
        public int minValue = 1;
        [MaxValue(14)]
        public int maxValue = 14;
    }
}
