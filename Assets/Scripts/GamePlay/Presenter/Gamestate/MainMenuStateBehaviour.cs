using MessagePipe;
using PlayingCard.GamePlay.Model.Message;
using VContainer;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    /// <summary>
    /// MainMenu Scene에서 한정적으로 사용할 Container 설정
    /// </summary>
    internal class MainMenuStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainMenu;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            MessagePipeOptions options = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<MainMenuMessage>(options);

            builder.Register<MainMenuPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
