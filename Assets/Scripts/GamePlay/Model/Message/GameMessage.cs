using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.GamePlay.View.PlayObject;
using System.Collections.Generic;

namespace PlayingCard.GamePlay.Message
{
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
    /// Game Room UI용 갱신 정보
    /// </summary>
    public struct GameRoomInfoMessage
    {
        public ulong ClientId;
        public string NickName;
        public string RoundName;
        public Betting LastBetting;
        public ulong MinRaise;
        public ulong Pot;
        public ulong LastMaxBet;

        public GameRoomInfoMessage(ulong clientId, string nickName, string roundName, Betting lastBetting, ulong minRaise, ulong pot, ulong lastMaxBet)
        {
            this.ClientId = clientId;
            this.NickName = nickName;
            this.RoundName = roundName;
            this.LastBetting = lastBetting;
            this.MinRaise = minRaise;
            this.Pot = pot;
            this.LastMaxBet = lastMaxBet;
        }
    }

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
        public ulong clientId;

        public TurnStartMessage(ulong minRaise, string roundName, ulong pot, ulong lastMaxBet, Betting lastBetting, ulong clientId)
        {
            this.MinRaise = minRaise;
            this.RoundName = roundName;
            this.Pot = pot;
            this.LastMaxBet = lastMaxBet;
            this.LastBetting = lastBetting; 
            this.clientId = clientId;
        }
    }

    public struct RoundCompleteMessage { }

    /// <summary>
    /// Player Turn Action 메시지
    /// </summary>
    public struct TurnActionMessage
    {
        public ulong ClientId;
        public Betting betting;
        public ulong bet;

        public TurnActionMessage(ulong clientId, Betting betting, ulong bet)
        {
            ClientId = clientId;
            this.betting = betting;
            this.bet = bet;
        }
    }

    /// <summary>
    /// Card 를 Target에 지급한다.
    /// </summary>
    public struct DealCardMessage
    {
        public List<Card> dealCards;
        public ulong? clientId;

        public DealCardMessage(List<Card> dealCards, ulong? clinetId)
        {
            this.dealCards = dealCards;
            this.clientId = clinetId;
        }
    }

    /// <summary>
    /// 승자 표시 메시지
    /// </summary>
    public struct WinnerMessage
    {
        //public Dictionary<ulong, ulong> winners;

        //public WinnerMessage(Dictionary<ulong, ulong> winners)
        //{
        //    this.winners = winners;
        //}
        public ulong clientId;
        public ulong winChips;

        public WinnerMessage(ulong clientId, ulong winnerChips)
        {
            this.clientId = clientId;
            this.winChips = winnerChips;
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
        public ServerPlayer serverPlayer;
        public ObjectCard objectCard;

        public DrawCardSelectMessage(ServerPlayer serverPlayer, ObjectCard objectCard)
        {
            this.serverPlayer = serverPlayer;
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
        public ulong ClientId;

        public DrawCardsMessage(ulong clientId)
        {
            ClientId = clientId;
        }
    }

    public struct DrawResultMessage
    {
        public ulong ClientId;
        public List<Card> cards;

        public DrawResultMessage(ulong clientId, List<Card> cards)
        {
            ClientId = clientId;
            this.cards = cards;
        }
    }
}