using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;

namespace PlayingCard.GamePlay.Message
{
    /// <summary>
    /// 게임 종료 메시지
    /// </summary>
	public struct QuitGameMessage { }

    /// <summary>
    /// Main Menu 에서 Game 선택 메시지
    /// </summary>
    public struct SelectGameMessage
    {
        /// <summary>
        /// 선택한 Game
        /// </summary>
        public Game game;

        public SelectGameMessage(Game game)
        {
            this.game = game;
        }
    }

    /// <summary>
    /// Game Room 에서 게임 시작 메시지
    /// </summary>
    public struct StartGameMessage { }

    /// <summary>
    /// Game Room 에서 게임 종료 메시지
    /// </summary>
	public struct EndGameMessage { }

    /// <summary>
    /// Game Room 에서 Main Menu로 빠져 나가는 메시지
    /// </summary>
    public struct ExitGameMessage { }

    /// <summary>
    /// Player Turn 시작 메시지
    /// </summary>
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

    /// <summary>
    /// Player Turn Action 메시지
    /// </summary>
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