using MessagePipe;
using PlayingCard.ApplicationLifecycle.Message;
using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Message;
using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter
{
    public class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly ConnectionManager connectionManager;
        private readonly string nickname;
        private readonly List<Game> gameList;
        private readonly IGameManager gameManager;
        private readonly ISubscriber<MainMenuMessage> messageSubscriber;
        private readonly IPublisher<QuitApplicationMessage> quitGamePublisher;

        private IDisposable subscription;

        private int selectedIdx = -1;

        public MainMenuPresenter(
            ConnectionManager connectionManager,
            string nickname,
            List<Game> gameList,
            IGameManager gameManager,
            ISubscriber<MainMenuMessage> messageSubscriber,
            IPublisher<QuitApplicationMessage> quitGamePublisher,
            ISubscriber<ConnectStatus> connectSubscriber)
        {
            this.connectionManager = connectionManager;
            this.nickname = nickname;
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
                            // 게임 룰에 따른 최대 인원수를 수정해 준다.
                            connectionManager.MaxConnectedPlayers = gameList[selectedIdx].Rule.MaxPlayer;
                            //gameManager.InitGame(gameList[selectedIdx]);
                            //SceneLoaderWarpper.Instance.LoadScene(DefineScene.GAME_ROOM);
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
                case MainMenuMessageType.Network:
                    {
                        if (message.value == 1)
                        {
                            connectionManager.StartHostIp(nickname, "127.0.0.1", 9997, "100", $"{selectedIdx}");
                        }
                        else if (message.value == 2)
                        {
                            connectionManager.StartClientIp(nickname, "127.0.0.1", 9997, "100", $"{selectedIdx}");
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
