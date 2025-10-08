using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.UI;
using PlayingCard.Utilities.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace PlayingCard.GamePlay.GameState
{
    internal class MainMenuStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainMenu;

        [SerializeField]
        MainMenuScroller scroller;
        [SerializeField]
        Button buttonExit;
        [SerializeField]
        Button buttonStart;

        IGameManager gameManager;
        private IDisposable selectGameDisposable;

        protected override void Start()
        {
            base.Start();

            buttonExit.AddOnClickEvent(GameQuit);
            buttonStart.AddOnClickEvent(GameStart);
        }

        public void Set(IGameManager gameManager, ISubscriber<SelectGameMessage> selectGameSubscriber)
        {
            this.gameManager = gameManager;
            selectGameDisposable = selectGameSubscriber.Subscribe(SelectGame);
        }

        void SelectGame(SelectGameMessage message)
        {
            bool isEmpty = message.game == null;
            buttonStart.interactable = !isEmpty;
        }

        void GameStart()
        {
            gameManager.StartGame();
        }

        void GameQuit()
        {
            gameManager.QuitGame();
        }
    }
}
