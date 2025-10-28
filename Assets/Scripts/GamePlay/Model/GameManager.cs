using PlayingCard.GamePlay.Model.Configuration;

namespace PlayingCard.GamePlay.Model
{
    public interface IGameManager
    {
        void SetGame(Game game);
    }

    public class GameManager : IGameManager
    {
        public Game SelectedGame;

        public void SetGame(Game game)
        {
            SelectedGame = game;
        }
    }
}
