using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.Utilities;
using System;

namespace PlayingCard.GamePlay
{
    public interface IGameManager : IDisposable
    {
        void StartGame();
        void StopGame();
        void QuitGame();
    }

    [Serializable]
    public class GameManager : IGameManager
    {
        public Game Game { get; private set; }

        private readonly PlayTable table;

        private readonly IPublisher<QuitGameMessage> quitGamePublisher;
        private readonly IDisposable selectGameDisposable;

        public GameManager(
            PlayTable table,
            IPublisher<QuitGameMessage> quitGamePublisher, 
            ISubscriber<SelectGameMessage> selectGameSubscriber)
        {
            this.table = table;
            this.quitGamePublisher = quitGamePublisher;
            selectGameDisposable = selectGameSubscriber.Subscribe(SetGame);
        }

        void SetGame(SelectGameMessage message)
        {
            this.Game = message.game;
        }

        public void StartGame()
        {
            table.SetGame(Game);
            SceneLoaderWarpper.Instance.LoadScene("GameRoom");
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
