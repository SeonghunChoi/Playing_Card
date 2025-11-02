using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    /// <summary>
    /// Lobby Scene 에서 한정적으로 사용할 Container 설정
    /// </summary>
    public class LobbyStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.Lobby;
    }
}
