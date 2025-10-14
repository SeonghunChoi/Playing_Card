using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.GamePlay.PlayObject;
using System.Collections.Generic;

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
        public string RoundName;
        public ulong Pot;
        public ulong LastMaxBet;
        public Betting LastBetting;
        public Player player;

        public TurnStartMessage(ulong minRaise, string roundName, ulong pot, ulong lastMaxBet, Betting lastBetting, Player player)
        {
            this.MinRaise = minRaise;
            this.RoundName = roundName;
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

    /// <summary>
    /// ObjectCard 를 Target에 지급한다.
    /// </summary>
    public struct DealCardMessage
    {
        public List<Card> dealCards;
        public Player player;

        public DealCardMessage(List<Card> dealCards, Player player)
        {
            this.dealCards = dealCards;
            this.player = player;
        }
    }

    /// <summary>
    /// 승자 표시 메시지
    /// </summary>
    public struct WinnerMessage
    {
        public Dictionary<Player, ulong> winners;

        public WinnerMessage(Dictionary<Player, ulong> winners)
        {
            this.winners = winners;
        }
    }

    /// <summary>
    /// 카메라 플레이어 위치로 이동
    /// </summary>
    public struct SetPlayerCameraMessage
    {
        public Player player;

        public SetPlayerCameraMessage(Player player)
        {
            this.player = player;
        }
    }

    public struct DrawCardSelectMessage
    {
        public ObjectPlayer objectPlayer;
        public ObjectCard objectCard;

        public DrawCardSelectMessage(ObjectPlayer objectPlayer, ObjectCard objectCard)
        {
            this.objectPlayer = objectPlayer;
            this.objectCard = objectCard;
        }
    }

    public struct DrawInfoMessage
    {
        public int DrawCardCount;

        public DrawInfoMessage(int DrawCardCount)
        {
            this.DrawCardCount = DrawCardCount;
        }
    }

    public struct DrawCardsMessage
    {
        public Player player;

        public DrawCardsMessage(Player player)
        {
            this.player = player;
        }
    }

    public struct DrawResultMessage
    {
        public Player player;
        public List<Card> cards;

        public DrawResultMessage(Player player, List<Card> cards)
        {
            this.player = player;
            this.cards = cards;
        }
    }
}