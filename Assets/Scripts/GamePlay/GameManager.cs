using PlayingCard.GamePlay.Configuration;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PlayingCard.GamePlay
{
    [Serializable]
    public class GameManager : MonoBehaviour
    {
        public Game Game { get; private set; }
        public List<Game> GameList;

        public event UnityAction<Game> onGameChanged;
        public event Action onGameQuit;

        public GameManager(List<Game> games)
        {
            GameList = games;
        }

        public void SetGame(Game game)
        {
            this.Game = game;
            onGameChanged?.Invoke(game);
        }

        public void QuitGame()
        {
            onGameQuit?.Invoke();
        }
    }
}
