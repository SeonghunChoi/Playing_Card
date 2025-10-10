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
        }

        protected override void Start()
        {
            base.Start();

            startGamePublisher = Container.Resolve<IPublisher<StartGameMessage>>();
            startGamePublisher.Publish(new StartGameMessage());
        }
    }
}
