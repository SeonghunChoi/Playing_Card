using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Configuration.Define;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.Utilities;
using System;
using VContainer;

namespace PlayingCard.GamePlay
{
    public interface IGameManager : IDisposable
    {
        void StartGame();
        void StopGame();
        void QuitGame();
    }

    /// <summary>
    /// 게임 전반을 관리한다.
    /// </summary>
    [Serializable]
    public class GameManager : IGameManager
    {
        public Game Game { get; private set; }

        private readonly IPlayTable playTable;
        private readonly IPublisher<QuitGameMessage> quitGamePublisher;
        private readonly IDisposable selectGameDisposable;

        [Inject]
        public GameManager(
            IPlayTable playTable,
            IPublisher<QuitGameMessage> quitGamePublisher,
            ISubscriber<SelectGameMessage> selectGameSubscriber)
        {
            this.playTable = playTable;
            this.quitGamePublisher = quitGamePublisher;
            selectGameDisposable = selectGameSubscriber.Subscribe(SetGame);
        }

        void SetGame(SelectGameMessage message)
        {
            this.Game = message.game;
        }

        public void StartGame()
        {
            if (Game == null) return;

            playTable.InitGame(Game);
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.GAME_ROOM);
        }

        public void StopGame()
        {
            this.Game = null;
        }

        public void QuitGame()
        {
            this.Game = null;
            quitGamePublisher.Publish(new QuitGameMessage());
        }

        public void Dispose()
        {
            selectGameDisposable?.Dispose();
        }
    }
}
