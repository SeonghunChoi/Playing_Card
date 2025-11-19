using MessagePipe;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.Utilities.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    /// <summary>
    /// 메인 메뉴 UI
    /// </summary>
    public class UIMainMenu : MonoBehaviour
    {
        [SerializeField]
        Button buttonExit;
        [SerializeField]
        Button buttonStart;
        [SerializeField]
        GameObject uiMultiplay;

        GameManager gameManager;
        private IPublisher<MainMenuMessage> mainMenuMessagePublisher;

        protected void Start()
        {
            buttonExit.AddOnClickEvent(GameQuit);
            buttonStart.AddOnClickEvent(GameStart);
        }

        [Inject]
        public void Set(IGameManager gameManager, IPublisher<MainMenuMessage> mainMenuMessagePublisher)
        {
            this.gameManager = gameManager as GameManager;
            this.mainMenuMessagePublisher = mainMenuMessagePublisher;
        }

        void GameStart()
        {
            if (gameManager.SelectedGame == null) return;
            uiMultiplay.SetActive(true);
        }

        void GameQuit()
        {
            mainMenuMessagePublisher.Publish(new MainMenuMessage(MainMenuMessageType.Exit, -1));
        }
    }
}
