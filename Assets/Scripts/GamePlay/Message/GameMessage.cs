using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;

namespace PlayingCard.GamePlay.Message
{
	public struct QuitGameMessage { }

    public struct SelectGameMessage
    {
        public Game game;

        public SelectGameMessage(Game game)
        {
            this.game = game;
        }
    }

    public struct StartGameMessage { }

	public struct EndGameMessage { }

    public struct ExitGameMessage { }

    public struct TrunStartMessage
    {
        public Player player;

        public TrunStartMessage(Player player)
        {
            this.player = player;
        }
    }

    public struct TurnActionMessage
    {
		public Player player;

        public TurnActionMessage(Player player)
        {
            this.player = player;
        }
    }
}