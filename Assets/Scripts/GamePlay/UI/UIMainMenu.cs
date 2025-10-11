using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.Utilities.UI;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.UI
{
    /// <summary>
    /// 메인 메뉴 UI
    /// </summary>
    public class UIMainMenu : MonoBehaviour, IDisposable
    {
        [SerializeField]
        Button buttonExit;
        [SerializeField]
        Button buttonStart;

        IGameManager gameManager;
        private IDisposable selectGameDisposable;

        protected void Start()
        {
            buttonExit.AddOnClickEvent(GameQuit);
            buttonStart.AddOnClickEvent(GameStart);
        }

        [Inject]
        public void Set(IGameManager gameManager, ISubscriber<SelectGameMessage> selectGameSubscriber)
        {
            this.gameManager = gameManager;
            selectGameDisposable = selectGameSubscriber.Subscribe(SelectGame);
        }

        /// <summary>
        /// 게임 선택 메시지 처리
        /// </summary>
        /// <param name="message"></param>
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

        public void Dispose()
        {
            selectGameDisposable?.Dispose();
        }
    }
}
