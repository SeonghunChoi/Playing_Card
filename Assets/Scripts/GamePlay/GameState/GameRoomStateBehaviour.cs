using MessagePipe;
using PlayingCard.GamePlay.Message;
using VContainer;

namespace PlayingCard.GamePlay.GameState
{
    /// <summary>
    /// GameRoom Scene에서 한정적으로 사용할 Container 설정
    /// </summary>
    public class GameRoomStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.GameRoom;

        private IPublisher<StartGameMessage> startGamePublisher;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            //GameManager gameManager = Container.Resolve<GameManager>();
            //builder.RegisterInstance(gameManager.Game.Rule);
            //builder.Register<HandRankingManager>(Lifetime.Scoped);
        }

        /// <summary>
        /// GameRoom Scene 시작 시 처리할 설정
        /// </summary>
        protected override void Start()
        {
            base.Start();

            startGamePublisher = Container.Resolve<IPublisher<StartGameMessage>>();
            startGamePublisher.Publish(new StartGameMessage());
        }
    }
}
