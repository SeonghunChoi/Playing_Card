using MessagePipe;
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

        private IPublisher<MainMenuMessage> mainMenuMessagePublisher;

        protected void Start()
        {
            buttonExit.AddOnClickEvent(GameQuit);
            buttonStart.AddOnClickEvent(GameStart);
        }

        [Inject]
        public void Set(IPublisher<MainMenuMessage> mainMenuMessagePublisher)
        {
            this.mainMenuMessagePublisher = mainMenuMessagePublisher;
        }

        void GameStart()
        {
            mainMenuMessagePublisher.Publish(new MainMenuMessage(MainMenuMessageType.Start, -1));
        }

        void GameQuit()
        {
            mainMenuMessagePublisher.Publish(new MainMenuMessage(MainMenuMessageType.Exit, -1));
        }
    }
}
