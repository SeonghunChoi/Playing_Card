using MessagePipe;
using PlayingCard.ApplicationLifecycle.Message;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.Utilities;
using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly List<Game> gameList;
        private readonly IGameManager gameManager;
        private readonly ISubscriber<MainMenuMessage> messageSubscriber;
        private readonly IPublisher<QuitApplicationMessage> quitGamePublisher;

        private IDisposable subscription;

        private int selectedIdx = -1;

        public MainMenuPresenter(
            List<Game> gameList,
            IGameManager gameManager,
            ISubscriber<MainMenuMessage> messageSubscriber,
            IPublisher<QuitApplicationMessage> quitGamePublisher)
        {
            this.gameList = gameList;
            this.gameManager = gameManager;
            this.messageSubscriber = messageSubscriber;
            this.quitGamePublisher = quitGamePublisher;
        }

        public void Initialize()
        {
            var bagBuilder = DisposableBag.CreateBuilder();

            messageSubscriber.Subscribe(ProcessMainMenuMessage).AddTo(bagBuilder);

            subscription = bagBuilder.Build();
        }

        private void ProcessMainMenuMessage(MainMenuMessage message)
        {
            switch (message.messageType)
            {
                case MainMenuMessageType.Start:
                    {
                        if (selectedIdx != -1)
                        {
                            //gameManager.InitGame(gameList[selectedIdx]);
                            SceneLoaderWarpper.Instance.LoadScene(DefineScene.GAME_ROOM);
                        }
                    }
                    break;
                case MainMenuMessageType.Exit:
                    {
                        quitGamePublisher.Publish(new QuitApplicationMessage());
                    }
                    break;
                case MainMenuMessageType.Menu:
                    {
                        if (message.value < gameList.Count)
                        {
                            selectedIdx = message.value;
                            gameManager.SetGame(gameList[selectedIdx]);
                        }
                        else
                        {
                            selectedIdx = -1;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void Dispose()
        {
            subscription?.Dispose();
        }
    }
}
