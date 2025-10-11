using MessagePipe;
using PlayingCard.GamePlay.Message;
using VContainer;

namespace PlayingCard.GamePlay.GameState
{
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

        protected override void Start()
        {
            base.Start();

            startGamePublisher = Container.Resolve<IPublisher<StartGameMessage>>();
            startGamePublisher.Publish(new StartGameMessage());
        }
    }
}
