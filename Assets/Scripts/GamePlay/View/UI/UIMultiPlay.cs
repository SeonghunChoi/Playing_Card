using MessagePipe;
using PlayingCard.GamePlay.Model.Message;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    public class UIMultiPlay : MonoBehaviour
    {
        [SerializeField]
        Button buttonHost;
        [SerializeField]
        Button buttonJoin;

        [SerializeField]
        Button buttonBack;

        private IPublisher<MainMenuMessage> mainMenuPublisher;

        [Inject]
        void Construct(IPublisher<MainMenuMessage> mainMenuPublisher)
        {
            this.mainMenuPublisher = mainMenuPublisher;
        }

        private void Start()
        {
            buttonHost.onClick.AddListener(OnClickHost);
            buttonJoin.onClick.AddListener(OnClickJoin);
            buttonBack.onClick.AddListener(OnClickBack);
        }

        private void OnClickBack()
        {
            gameObject.SetActive(false);
        }

        private void OnClickJoin()
        {
            mainMenuPublisher.Publish(new MainMenuMessage(MainMenuMessageType.Network, 2));
        }

        private void OnClickHost()
        {
            mainMenuPublisher.Publish(new MainMenuMessage(MainMenuMessageType.Network, 1));
        }
    }
}
