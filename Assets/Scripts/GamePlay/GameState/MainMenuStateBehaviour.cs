using VContainer;

namespace PlayingCard.GamePlay.GameState
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
        }
    }
}
