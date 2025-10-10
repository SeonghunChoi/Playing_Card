using VContainer;

namespace PlayingCard.GamePlay.GameState
{
    internal class MainMenuStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainMenu;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
        }
    }
}
