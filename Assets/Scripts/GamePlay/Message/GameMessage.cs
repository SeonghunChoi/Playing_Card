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

    public struct TurnStartMessage
    {
        public ulong MinRaise;
        public int Round;
        public ulong Pot;
        public ulong LastMaxBet;
        public Betting LastBetting;
        public Player player;

        public TurnStartMessage(ulong minRaise, int round, ulong pot, ulong lastMaxBet, Betting lastBetting, Player player)
        {
            this.MinRaise = minRaise;
            this.Round = round;
            this.Pot = pot;
            this.LastMaxBet = lastMaxBet;
            this.LastBetting = lastBetting; 
            this.player = player;
        }
    }

    public struct TurnActionMessage
    {
		public Player player;
        public Betting betting;
        public ulong bet;

        public TurnActionMessage(Player player, Betting betting, ulong bet)
        {
            this.player = player;
            this.betting = betting;
            this.bet = bet;
        }
    }
}