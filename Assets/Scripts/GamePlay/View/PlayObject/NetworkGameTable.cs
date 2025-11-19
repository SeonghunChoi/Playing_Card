using Cysharp.Threading.Tasks;
using MessagePipe;
using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.View.PlayObject
{
    /// <summary>
    /// Game Room 의 GameObject 들을 관리
    /// </summary>
    public class NetworkGameTable : NetworkBehaviour, IPointerClickHandler
    {
        const float CardSpace = 1.8f;

        /// <summary>
        /// 테이블에 커뮤니티 카드들을 놓을 컨테이너
        /// </summary>
        [SerializeField]
        Transform trComunityPoint;

        /// <summary>
        /// 최소 Raise 금액
        /// </summary>
        ulong MinRaise => game.Rule.MinRaise;

        /// <summary>
        /// 선택한 게임 정보
        /// </summary>
        Game game;
        ConnectionManager connectionManager;

        /// <summary>
        /// 현재 라운드 딜러를 맏을 유저의 idx
        /// </summary>
        NetworkVariable<int> dealerIdx = new NetworkVariable<int>();
        /// <summary>
        /// 이번 게임 smallBlind 유저의 idx
        /// </summary>
        NetworkVariable<int> smallBlindIdx = new NetworkVariable<int>();
        /// <summary>
        /// 이번 게임 bigBlind 유저의 idx
        /// </summary>
        NetworkVariable<int> bigBlindIdx = new NetworkVariable<int>();
        /// <summary>
        /// 현재 라운드 턴이 돌아온 유저의 idx
        /// </summary>
        NetworkVariable<int> currentPlayerIdx = new NetworkVariable<int>();
        /// <summary>
        /// 최고 높은 bet 금액
        /// </summary>
        NetworkVariable<ulong> lastMaxBet = new NetworkVariable<ulong>();
        /// <summary>
        /// Pot 에 놓인 금액
        /// </summary>
        NetworkVariable<ulong> pot = new NetworkVariable<ulong>();
        /// <summary>
        /// All in 발생 시 SidePot 금액
        /// </summary>
        NetworkVariable<ulong> sidePot = new NetworkVariable<ulong>();
        /// <summary>
        /// 현재 라운드
        /// </summary>
        NetworkVariable<int> round = new NetworkVariable<int>();
        /// <summary>
        /// 직전 플레이어의 Turn Action 의 Betting 종류
        /// </summary>
        NetworkVariable<Betting> lastBetting = new NetworkVariable<Betting>();

        /// <summary>
        /// 게임에서 사용할 모든 카드
        /// </summary>
        List<Card> cards = new List<Card>();
        /// <summary>
        /// 게임에서 사용할 모든 카드를 섞어서 카드들을 넣어놓은 큐, 순석대로 뽑아 쓴다.
        /// </summary>
        Queue<Card> deck = new Queue<Card>();
        /// <summary>
        /// 홀덤에서 공유하는 커뮤니티 카드 정보
        /// </summary>
        List<Card> communityCard;
        PlayRound currentRound;

        private ISubscriber<TableStateMessage> tableStateSubscriber;
        private IPublisher<GameRoomInfoMessage> gameRoomInfoPublisher;
        private IPublisher<TurnStartMessage> turnStartPublisher;
        private IPublisher<RoundCompleteMessage> roundCompletePublisher;

        private ISubscriber<TurnActionMessage> turnActionSubscriber;

        private IPublisher<WinnerMessage> winnerPublisher;
        private IPublisher<DrawInfoMessage> drawCardInfoPublisher;
        private ISubscriber<DrawCardSelectMessage> drawCardSelectSubscriber;
        private ISubscriber<DrawCardsMessage> drawCardsSubscriber;
        private IPublisher<DrawResultMessage> drawResultPublisher;

        private IObjectResolver resolver;
        private IDisposable turnStartDisposable;
        private ISubscriber<TurnStartMessage> turnStartSubscriber;
        private IDisposable drawResultDisposable;
        private IPublisher<DrawCardSelectMessage> drawCardSelectPublisher;
        private Dictionary<string, GameObject> cardDict;

        private IDisposable subscription;

        System.Random random = new System.Random();
        List<Card> dealBuffer = new List<Card>();

        private ServerPlayer curPlayr;

        [Inject]
        public void Set(
            Game SelectedGame,
            ConnectionManager connectionManager,
            ISubscriber<TableStateMessage> tableStateSubscriber,
            IPublisher<GameRoomInfoMessage> gameRoomInfoPublisher,
            IPublisher<TurnStartMessage> turnStartPublisher,
            IPublisher<RoundCompleteMessage> roundCompletePublisher,

            ISubscriber<TurnActionMessage> turnActionSubscriber,

            IPublisher<WinnerMessage> winnerPublisher,
            IPublisher<DrawInfoMessage> drawCardInfoPublisher,
            ISubscriber<DrawCardSelectMessage> drawCardSelectSubscriber,
            ISubscriber<DrawCardsMessage> drawCardsSubscriber,
            IPublisher<DrawResultMessage> drawResultPublisher,
            IObjectResolver resolver, Dictionary<string, GameObject> cardDict, 
            ISubscriber<TurnStartMessage> turnStartSubscriber,
            ISubscriber<DrawResultMessage> drawResultSubscriber,
            IPublisher<DrawCardSelectMessage> drawCardSelectPublisher)
        {
            this.game = SelectedGame;
            this.connectionManager = connectionManager;

            this.tableStateSubscriber = tableStateSubscriber;
            this.gameRoomInfoPublisher = gameRoomInfoPublisher;
            this.turnStartPublisher = turnStartPublisher;
            this.roundCompletePublisher = roundCompletePublisher;

            this.turnActionSubscriber = turnActionSubscriber;

            this.winnerPublisher = winnerPublisher;
            this.drawCardInfoPublisher = drawCardInfoPublisher;
            this.drawCardSelectSubscriber = drawCardSelectSubscriber;
            this.drawCardsSubscriber = drawCardsSubscriber;
            this.drawResultPublisher = drawResultPublisher;

            this.resolver = resolver;
            this.cardDict = cardDict;
            this.turnStartSubscriber = turnStartSubscriber;
            drawResultDisposable = drawResultSubscriber.Subscribe(DrawResult);
            this.drawCardSelectPublisher = drawCardSelectPublisher;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            dealerIdx.OnValueChanged += DealerIdxOnValueChanged;
            smallBlindIdx.OnValueChanged += SmallBlindIdxOnValueChanged;
            bigBlindIdx.OnValueChanged += BigBlindIdxOnValueChanged;
            currentPlayerIdx.OnValueChanged += CurrentPlayerIdxOnValueChanged;
            lastMaxBet.OnValueChanged += LastMaxBetOnValueChanged;
            pot.OnValueChanged += PotOnValueChanged;
            sidePot.OnValueChanged += SidePotOnValueChanged;
            round.OnValueChanged += RoundOnValueChanged;
            lastBetting.OnValueChanged += LastBettingOnValueChanged;

            var bagBuilder = DisposableBag.CreateBuilder();

            tableStateSubscriber.Subscribe(ProcessTableStateMessage).AddTo(bagBuilder);
            turnActionSubscriber.Subscribe(TurnAction).AddTo(bagBuilder);
            drawCardSelectSubscriber.Subscribe(DrawCardSelect).AddTo(bagBuilder);
            drawCardsSubscriber.Subscribe(DrawCards).AddTo(bagBuilder);
            turnStartSubscriber.Subscribe(TurnStart).AddTo(bagBuilder);

            subscription = bagBuilder.Build();
                        
            if (IsServer) InitGame();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            turnStartDisposable?.Dispose();
            drawResultDisposable?.Dispose();

            subscription?.Dispose();
        }

        private void LastBettingOnValueChanged(Betting previousValue, Betting newValue)
        {
            Debug.Log($"LastBettingOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
        }

        private void RoundOnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"RoundOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
            SetCurrentRound();
            UpdateGameInfo();
        }

        private void SidePotOnValueChanged(ulong previousValue, ulong newValue)
        {
            Debug.Log($"SidePotOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
            UpdateGameInfo();
        }

        private void PotOnValueChanged(ulong previousValue, ulong newValue)
        {
            Debug.Log($"PotOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
            UpdateGameInfo();
        }

        private void LastMaxBetOnValueChanged(ulong previousValue, ulong newValue)
        {
            Debug.Log($"LastMaxBetOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
            UpdateGameInfo();
        }

        private void CurrentPlayerIdxOnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"CurrentPlayerIdxOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
        }

        private void BigBlindIdxOnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"BigBlindIdxOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
        }

        private void SmallBlindIdxOnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"SmallBlindIdxOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
        }

        private void DealerIdxOnValueChanged(int previousValue, int newValue)
        {
            Debug.Log($"DealerIdxOnValueChanged - previousValue:{previousValue}, newValue:{newValue}");
        }

        void UpdateGameInfo()
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var player = ClientPlayerManager.GetClientPlayer(clientId);
            if (player == null) return;
            var nickName = player.ServerPlayer.NickName.Value;
            if (currentRound == null) return;
            var roundName = string.IsNullOrEmpty(currentRound.RoundName) ? $"Round {round}" : currentRound.RoundName;
            var message = new GameRoomInfoMessage(clientId, nickName, roundName, lastBetting.Value, MinRaise, pot.Value, lastMaxBet.Value);
            gameRoomInfoPublisher.Publish(message);
        }

        /// <summary>
        /// Deck 정보에 맞춰 카드들을 생성한다.
        /// </summary>
        public void InitGame()
        {
            Debug.Log("InitGame");
            round.Value = 1;

            cards.Clear();

            var suits = game.Deck.SuitList;

            for (int i = 0; i < suits.Count; i++)
            {
                var suit = suits[i];
                for (int multiply = 0; multiply < suit.multiply; multiply++)
                {
                    for (int value = suit.minValue; value <= suit.maxValue; value++)
                    {
                        if (value == 1 && suit.maxValue >= 14) continue;

                        cards.Add(new Card(suit.SuitType, (Rank)value, false));
                    }
                }
            }

            for (int wild = 0; wild < game.Deck.WildCardCount; wild++)
            {
                cards.Add(new Card(Suit.Spades, Rank.None, false, true));
            }

            ResetTableRpc();
        }

        void SetLastMaxBet(ulong bet)
        {
            if (lastMaxBet.Value < bet)
            {
                lastMaxBet.Value = bet;
            }
        }

        /// <summary>
        /// 게임 테이블 정보는 서버에서만 관리하므로 서버에서 초기화 한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void ResetTableRpc()
        {
            dealerIdx.Value = -1; // 첫 플레이 시작은 딜러가 없는 상태

            if (communityCard == null) communityCard = new List<Card>();
            else communityCard.Clear();

            lastMaxBet.Value = 0;
            pot.Value = 0;
            sidePot.Value = 0;
            round.Value = 1;
            lastBetting.Value = Betting.Fold;

            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            // 라운드 정리.
            var playables = serverPlayers.FindAll(p => p.State.Value.IsPlayable());
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                player.ResetRound();
                player.ChangeState(PlayerState.Waiting);
                ClerTableRpc(player.OwnerClientId);
            }

            Shuffle(cards, 100);
            MakeDeck();
            ApplyResetResultRpc();
        }

        /// <summary>
        /// 현제 라운드 정보를 지정한다.
        /// </summary>
        void SetCurrentRound()
        {
            if (game.Rule.Rounds.Count > round.Value - 1)
                currentRound = new PlayRound(game.Rule.Rounds[round.Value - 1]);
            else
                currentRound = null;
        }

        /// <summary>
        /// 카드를 뽑아 사용할 덱을 만든다.
        /// </summary>
        void MakeDeck()
        {
            deck.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                deck.Enqueue(cards[i]);
            }
        }

        /// <summary>
        /// 테이블 초기화 한 정보를 각 클라이언트에게도 저장한다.
        /// </summary>
        [Rpc(SendTo.ClientsAndHost)]
        public void ApplyResetResultRpc()
        {
            // 이번 라운드 정보를 지정한다.
            SetCurrentRound();
        }

        /// <summary>
        /// 이번 라운드를 진행한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void PlayRoundRpc()
        {
            // 모든 라운드가 끝났다.
            if (currentRound == null)
            {
                GameResolveRpc();
                return;
            }

            switch (currentRound.RoundState)
            {
                case RoundState.Deal:
                    {
                        SetRoleRpc();                
                        DealCardRpc();// 카드를 나눠준다.
                    }
                    break;
                case RoundState.Blind:
                    {
                        RunBlindRpc();
                    }
                    break;
                case RoundState.Bet:
                    {
                        RunBettingRpc();
                    }
                    break;
                case RoundState.Complete:
                    {
                        var serverPlayers = ServerPlayerManager.GetServerPlayers();
                        // 라운드 정리.
                        var playables = serverPlayers.FindAll(p => p.State.Value.IsPlayable());
                        for (int i = 0; i < playables.Count; i++)
                        {
                            var player = playables[i];
                            player.ResetRound();
                            player.ChangeState(PlayerState.Waiting);
                        }
                        lastBetting.Value = Betting.Fold;
                        lastMaxBet.Value = 0;
                        round.Value++;
                        SetCurrentRound();

                        ApplyRoundCompleteRpc();
                        if (IsServer) PlayRoundRpc();
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 딜러를 정한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        void SetRoleRpc()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            ServerPlayer next = null;
            if (dealerIdx.Value == -1) // 첫 시작
            {
                dealerIdx.Value = UnityEngine.Random.Range(0, serverPlayers.Count);
            }
            else
            {
                next = GetNextDealPlayer(dealerIdx.Value);
                dealerIdx.Value = serverPlayers.IndexOf(next);
            }

            if (serverPlayers.FindAll(p => p.State.Value.IsPlayable()).Count == 2)
            {
                smallBlindIdx.Value = dealerIdx.Value;
            }
            else
            {
                next = GetNextDealPlayer(dealerIdx.Value);
                smallBlindIdx.Value = serverPlayers.IndexOf(next);
            }
            next = GetNextDealPlayer(smallBlindIdx.Value);
            bigBlindIdx.Value = serverPlayers.IndexOf(next);
            if (currentRound.Blind > 0)
            {
                next = GetNextDealPlayer(bigBlindIdx.Value);
                currentPlayerIdx.Value = serverPlayers.IndexOf(next);
            }
            else
            {
                currentPlayerIdx.Value = smallBlindIdx.Value;
            }
        }

        /// <summary>
        /// 카드를 나누어 준다.
        /// </summary>
        [Rpc(SendTo.Server)]
        void DealCardRpc()
        {
            if (currentRound.BurnCardCount > 0)
            {
                for (int i = 0; i < currentRound.BurnCardCount; i++)
                {
                    // Card Burn
                    deck.Dequeue();
                }
            }

            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            var dealCards = currentRound.DealCards;
            for (int i = 0; i < dealCards.Count; i++)
            {
                var dealCard = dealCards[i];
                if (dealCard.DealTarget == DealTarget.Table)
                {
                    dealBuffer.Clear();
                    if (deck.Count > 0)
                    {
                        var card = deck.Dequeue();
                        if (dealCard.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                            card.Flip();
                        else if (dealCard.DealFace == DealFace.FaceDown && card.IsFaceUp)
                            card.Flip();

                        dealBuffer.Add(card);
                    }
                    else
                    {
                        BreakGame();
                        return;
                    }
                    communityCard.AddRange(dealBuffer);
                    string serialized = JsonUtility.ToJson(new SerializationWrapper<Card>(dealBuffer));
                    DealCardRpc(serialized);
                }
                else
                {
                    var playables = serverPlayers.FindAll(p => p.State.Value.IsPlayable());
                    int playerCount = playables.Count;
                    int dealIdx = dealerIdx.Value;
                    for (int j = 0; j < playerCount; j++)
                    {
                        var player = GetNextDealPlayer(dealIdx);
                        dealBuffer.Clear();
                        if (deck.Count > 0)
                        {
                            var card = deck.Dequeue();
                            if (dealCard.DealFace == DealFace.FaceUp && card.IsFaceUp == false)
                                card.Flip();
                            else if (dealCard.DealFace == DealFace.FaceDown && card.IsFaceUp)
                                card.Flip();

                            player.ReceiveCard(card);
                            dealBuffer.Add(card);
                        }
                        else
                        {
                            // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                            BreakGame();
                            return;
                        }
                        string serialized = JsonUtility.ToJson(new SerializationWrapper<Card>(dealBuffer));
                        DealCardRpc(serialized, player.OwnerClientId);
                        // 다음 플레이어로 변경
                        player = GetNextDealPlayer(dealIdx);
                        dealIdx = serverPlayers.IndexOf(player);
                    }
                }
            }
            // 카드를 모두 나누어 주었으므로 플레이어들 상태를 변경해 준다.
            for (int i = 0; i < serverPlayers.Count; i++)
            {
                var player = serverPlayers[i];
                player.ChangeState(PlayerState.Playing);
            }
            //ApplyDealCardRpc();
            currentRound.NextState();

            if (IsServer) PlayRoundRpc();
        }

        [Rpc(SendTo.Everyone)]
        void ApplyDealCardRpc()
        {
            currentRound.NextState();
        }

        /// <summary>
        /// blind를 처리한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        void RunBlindRpc()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            ulong blind = currentRound.Blind;
            ulong samllBlind = (blind / 2) + (blind % 2);
            if (blind > 0)
            {
                var sb = serverPlayers[smallBlindIdx.Value];
                pot.Value += samllBlind;
                sb.ApplyBet(samllBlind);
                sb.ChangeState(PlayerState.Playing);

                var bb = serverPlayers[bigBlindIdx.Value];
                pot.Value += blind;
                bb.ApplyBet(blind);
                bb.ChangeState(PlayerState.Playing);

                SetLastMaxBet(blind);
                lastBetting.Value = Betting.Raise;
                //ApplyGameRoomInfoRpc(sb.OwnerClientId, sb.NickName, currentRound.RoundName, MinRaise, sb.Chips, sb.Bet);
                //ApplyGameRoomInfoRpc(bb.OwnerClientId, bb.NickName, currentRound.RoundName, MinRaise, bb.Chips, bb.Bet);
            }

            ulong ante = currentRound.Ante;
            if (ante > 0)
            {
                var playables = serverPlayers.FindAll(p => p.Chips.Value > ante);
                for (int i = 0; i < playables.Count; i++)
                {
                    var player = playables[i];
                    pot.Value += ante;
                    player.ApplyBet(ante);
                    player.ChangeState(PlayerState.Checked);
                    SetLastMaxBet(ante);
                    //ApplyGameRoomInfoRpc(player.OwnerClientId, player.NickName, currentRound.RoundName, MinRaise, player.Chips, player.Bet);
                }
            }

            currentRound.NextState();
            if (IsServer) PlayRoundRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        void ApplyGameRoomInfoRpc(ulong clientId, string nickName, string roundName, ulong minRaise, ulong myChips, ulong myBet, bool isMyTurn = false)
        {
            if (NetworkManager.Singleton.LocalClientId == clientId && IsClient)
            {
                Debug.Log($"WaitGameRoomInfoRpc - clientId:{clientId}, nickName:{nickName}, roundName:{roundName}, lastBetting:{lastBetting.Value}, minRaise:{minRaise}, pot:{pot.Value}, lastMaxBet:{lastMaxBet.Value}, myChips:{myChips}, myBet:{myBet}");
                //var message = new GameRoomInfoMessage(clientId, nickName, roundName, lastBetting.Value, minRaise, pot.Value, lastMaxBet.Value, myChips, myBet, isMyTurn);
                //gameRoomInfoPublisher.Publish(message);
            }
        }

        /// <summary>
        /// 베팅을 시작한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        void RunBettingRpc()
        {
            if (currentRound == null) return;

            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            if (serverPlayers.Count(p => p.State.Value.IsPlayable()) <= 1)
            {
                var winners = serverPlayers.FindAll(p => p.State.Value.IsPlayable()).ConvertAll(p => p.OwnerClientId);
                var serialized = JsonUtility.ToJson(new SerializationWrapper<ulong>(winners));
                Debug.Log($"RunBettingRpc");
                if (IsServer) ResolveWinnerRpc(serialized);
                return;
            }

            if (serverPlayers.Count(p => !p.State.Value.HasActed()) == 0)
            {
                currentRound.NextState();
                if (IsServer) PlayRoundRpc();
                return;
            }

            var player = serverPlayers[currentPlayerIdx.Value];
            string roundName = string.IsNullOrEmpty(currentRound.RoundName) ? $"Round {round.Value}" : currentRound.RoundName;
            TurnStartRpc(player.OwnerClientId, MinRaise, roundName, pot.Value);
        }

        /// <summary>
        /// 클라이언트 들에게 턴 시작 정보를 알려준다.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="minRaise"></param>
        /// <param name="roundName"></param>
        /// <param name="pot"></param>
        [Rpc(SendTo.ClientsAndHost)]
        void TurnStartRpc(ulong clientId, ulong minRaise, string roundName, ulong pot)
        {
            turnStartPublisher.Publish(new TurnStartMessage(minRaise, roundName, pot, lastMaxBet.Value, lastBetting.Value, clientId));
        }

        /// <summary>
        /// 승자 처리
        /// </summary>
        /// <param name="winners"></param>
        [Rpc(SendTo.Server)]
        void ResolveWinnerRpc(string serializedWinners)
        {
            var winnersWrapper = JsonUtility.FromJson<SerializationWrapper<ulong>>(serializedWinners);
            var winners = winnersWrapper.ToList();
#if UNITY_EDITOR
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ResolveWinner");
            for (int i = 0; i < winners.Count; i++)
            {
                var player = ServerPlayerManager.GetServerPlayer(winners[i]);
                var hands = player.AllCards;
                var handRanking = HandRankingManager.GetHandRankingType(hands);
                sb.AppendLine($"Player:{player.OwnerClientId}, {handRanking.ToString()}");
                sb.AppendLine("--------------------------------------------------------------------------------");
            }
            Debug.Log(sb.ToString());
#endif
            ulong split = pot.Value / (ulong)winners.Count;
            ulong remainder = pot.Value % (ulong)winners.Count;
            Dictionary<ulong, ulong> winnerChips = new Dictionary<ulong, ulong>();
            for (int i = 0; i < winners.Count; i++)
            {
                var player = ServerPlayerManager.GetServerPlayer(winners[i]);
                ulong winChips = 0;
                winChips += split;
                if (i < (int)remainder) winChips++; // 잔여칩 할당
                winnerChips.Add(player.OwnerClientId, winChips);
                player.ApplyWinChips(winChips);
                ApplyWinnerRpc(player.OwnerClientId, winChips);
            }
            var players = ServerPlayerManager.GetServerPlayers();
        }

        /// <summary>
        /// 모든 클라이언트에게 승자 정보를 알려준다.
        /// </summary>
        /// <param name="clinetId"></param>
        /// <param name="winChips"></param>
        [Rpc(SendTo.ClientsAndHost)]
        void ApplyWinnerRpc(ulong clinetId, ulong winChips)
        {
            var clientPlayer = ClientPlayerManager.GetClientPlayer(clinetId);
            if (NetworkManager.Singleton.LocalClientId == clinetId)
            {
                winnerPublisher.Publish(new WinnerMessage(clinetId, winChips));
            }
        }

        /// <summary>
        /// 라운드 종료를 처리한다.
        /// </summary>
        [Rpc(SendTo.ClientsAndHost)]
        void ApplyRoundCompleteRpc()
        {
            roundCompletePublisher.Publish(new RoundCompleteMessage());
            SetCurrentRound();
        }

        /// <summary>
        /// 여러명이 남은 상태에서 게임이 종료되었을 때 승자를 결정한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        void GameResolveRpc()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            var playables = serverPlayers.FindAll(p => p.State.Value.IsPlayable());

            Dictionary<ServerPlayer, HandRanking> topPlayers = new Dictionary<ServerPlayer, HandRanking>();
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                var hands = player.AllCards;

                var HandRanking = HandRankingManager.GetHandRankingType(hands);

                if (topPlayers.Count == 0)
                {
                    topPlayers.Add(player, HandRanking);
                }
                else
                {
                    var topHand = topPlayers.Values.FirstOrDefault();
                    int cmp = topHand.CompareHandRanking(HandRanking);
                    if (cmp == 0)
                    {
                        topPlayers.Add(player, HandRanking);
                    }
                    else if (cmp == 1)
                    {
                        topPlayers.Clear();
                        topPlayers.Add(player, HandRanking);
                    }
                }
            }
            var winners = topPlayers.Keys.ToList().ConvertAll(p => p.OwnerClientId);
            var serialized = JsonUtility.ToJson(new SerializationWrapper<ulong>(winners));
            Debug.Log("GameResolveRpc");
            if (IsServer) ResolveWinnerRpc(serialized);
        }

        /// <summary>
        /// 다음 베팅 플레이어를 구한다.
        /// </summary>
        /// <param name="turn">현재 플레이어 idx</param>
        /// <returns></returns>
        ServerPlayer GetNextBettingPlayer(int curIdx)
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            if (serverPlayers.FindAll(p => !p.State.Value.HasActed() && p.State.Value.IsPlayable()).Count == 0)
                return null;
            int idx = (curIdx + 1) % serverPlayers.Count;
            var player = serverPlayers[idx];
            if (player.State.Value.HasActed())
                player = GetNextBettingPlayer(idx);
            return player;
        }

        /// <summary>
        /// 다음 베팅 플레이어를 구한다.
        /// </summary>
        /// <param name="turn">현재 플레이어 idx</param>
        /// <returns></returns>
        ServerPlayer GetNextDealPlayer(int curIdx)
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            if (serverPlayers.FindAll(p => p.State.Value.IsPlayable()).Count == 1)
                return null;
            int idx = (curIdx + 1) % serverPlayers.Count;
            var player = serverPlayers[idx];
            if (!player.State.Value.IsPlayable())
                player = GetNextDealPlayer(idx);
            return player;
        }

        public void Shuffle(List<Card> list, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                int idx = list.Count;
                while (idx > 1)
                {
                    idx--;
                    int changeIdx = random.Next(idx + 1);
                    var temp = list[changeIdx];
                    list[changeIdx] = list[idx];
                    list[idx] = temp;
                }
            }
        }

        /// <summary>
        /// 게임을 중간에 중지한다.
        /// </summary>
        private void BreakGame()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            var playables = serverPlayers.FindAll(p => p.Chips.Value > 0);
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                player.BreakGame();
            }

            ResetTableRpc();
            if (IsServer) PlayRoundRpc();
        }

        /// <summary>
        /// 게임 종료를 서버에 요청한다.
        /// </summary>
        [Rpc(SendTo.Server)]
        private void EndGameRpc()
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            var playables = serverPlayers.FindAll(p => p.Chips.Value > 0);
            if (playables.Count <= 1)
            {
                Debug.Log("Win");
                //최종결과 보여주기
                return;
            }
            // 플레이어들 상태 정리
            for (int i = 0; i < playables.Count; i++)
            {
                var player = playables[i];
                player.ResetGame();
                ClerTableRpc(player.OwnerClientId);
            }

            ResetTableRpc();
            if (IsServer) PlayRoundRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        void ExitGameRpc()
        {
            // 아직 호스트를 넘겨주는 기능이 없어 모두 나가는 것으로 구현.
            game = null;
            connectionManager.RequestShutdown();
            SceneLoaderWarpper.Instance.LoadScene(DefineScene.MAIN_MENU, useNetworkSceneManager: true);
        }

        /// <summary>
        /// 커뮤니티 카드 정보를 각 클라이언트에서 생성하도록 한다. (NetworkBehaviour 가 아님)
        /// </summary>
        /// <param name="serialiezed"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void DealCardRpc(string serialiezed)
        {
            var wrapper = JsonUtility.FromJson<SerializationWrapper<Card>>(serialiezed);
            List<Card> cards = wrapper.ToList();

            for (int i = 0; i < cards.Count; i++)
            {
                var card = CreateCard(GetCardName(cards[i]));
                card.transform.SetParent(trComunityPoint, false);
                card.transform.localScale = Vector3.one;
            }
            SetCardPosition(trComunityPoint);
        }

        /// <summary>
        /// 각 플레이어에게 나눠준 정보를 각 클라이언트에서 생성하도록 한다. (NetworkBehaviour 가 아님)
        /// </summary>
        /// <param name="serialiezed"></param>
        /// <param name="clientId"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void DealCardRpc(string serialiezed, ulong clientId)
        {
            var wrapper = JsonUtility.FromJson<SerializationWrapper<Card>>(serialiezed);
            List<Card> cards = wrapper.ToList();

            var clientPlayer = ClientPlayerManager.GetClientPlayer(clientId);
            for (int i = 0; i < cards.Count; i++)
            {
                var card = CreateCard(GetCardName(cards[i]));
                clientPlayer.AddCard(card, cards[i].IsFaceUp);
            }
            clientPlayer.SetCardPosition();
        }

        /// <summary>
        /// turn action 을 서버에 요청한다.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="betting"></param>
        /// <param name="bet"></param>
        [Rpc(SendTo.Server)]
        private void TurnActionRpc(ulong clientId, Betting betting, ulong bet)
        {
            Debug.Log($"TurnActionRpc(clientId:{clientId}, betting:{betting}, bet:{bet})");
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            var player = ServerPlayerManager.GetServerPlayer(clientId);

            if (bet > 0)
            {
                if (sidePot.Value > 0)
                {
                    sidePot.Value += bet;
                }
                else
                {
                    pot.Value += bet;
                    if (bet > lastMaxBet.Value)
                        lastMaxBet.Value = bet;
                }
                player.ApplyBet(bet);
            }

            switch (betting)
            {
                case Betting.Fold:
                    {
                        player.ChangeState(PlayerState.Folded);
                    }
                    break;
                case Betting.Check:
                    {
                        if (lastBetting.Value < Betting.Check)
                        {
                            lastBetting.Value = Betting.Check;
                        }
                        player.ChangeState(PlayerState.Checked);
                    }
                    break;
                case Betting.Bet:
                    {
                        if (lastBetting.Value < Betting.Bet)
                        {
                            lastBetting.Value = Betting.Bet;
                        }
                        var checkPlayers = serverPlayers.FindAll(p => p.State.Value.HasActed() && p.State.Value < PlayerState.Betted);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].ChangeState(PlayerState.Playing);
                        }
                        player.ChangeState(PlayerState.Betted);
                    }
                    break;
                case Betting.Call:
                    {
                        if (lastBetting.Value < Betting.Call)
                        {
                            lastBetting.Value = Betting.Call;
                        }
                        var checkPlayers = serverPlayers.FindAll(p => p.State.Value.HasActed() && p.State.Value < PlayerState.Called);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].ChangeState(PlayerState.Playing);
                        }
                        player.ChangeState(PlayerState.Called);
                    }
                    break;
                case Betting.Raise:
                    {
                        if (lastBetting.Value < Betting.Raise)
                        {
                            lastBetting.Value = Betting.Raise;
                        }
                        var checkPlayers = serverPlayers.FindAll(p => p.State.Value.HasActed() && p.State.Value < PlayerState.Raised);
                        for (int i = 0; i < checkPlayers.Count; i++)
                        {
                            checkPlayers[i].ChangeState(PlayerState.Playing);
                        }
                        player.ChangeState(PlayerState.Raised);
                    }
                    break;
                case Betting.AllIn:
                    {
                        player.ChangeState(PlayerState.AllIn);
                    }
                    break;
                default:
                    break;
            }
            var next = GetNextBettingPlayer(currentPlayerIdx.Value);
            if (next != null)
                currentPlayerIdx.Value = serverPlayers.IndexOf(next);
            Debug.Log($"IsServer:{IsServer}");
            if (IsServer) RunBettingRpc();
        }

        /// <summary>
        /// 테이블을 정리한다.
        /// </summary>
        /// <param name="clientId"></param>
        [Rpc(SendTo.ClientsAndHost)]
        private void ClerTableRpc(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                for (int i = 0; i < trComunityPoint.childCount; i++)
                {
                    var child = trComunityPoint.GetChild(i);
                    Destroy(child.gameObject);
                }

                var clientPlayers = ClientPlayerManager.GetClientPlayers();
                for (int i = 0; i < clientPlayers.Count; i++)
                {
                    var player = clientPlayers[i];
                    player.ClearHands();
                }
            }
        }

        /// <summary>
        /// 카드 Suit 와 Rank 로 카드 Prefab 이름을 가져온다.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public string GetCardName(Card card)
        {
            if (card.IsWild)
            {
                return "Deck03_Joker";
            }
            else
            {
                string suitName = null;
                switch (card.Suit)
                {
                    case Suit.Spades:
                        suitName = "Spade";
                        break;
                    case Suit.Hearts:
                        suitName = "Heart";
                        break;
                    case Suit.Diamonds:
                        suitName = "Diamond";
                        break;
                    case Suit.Clubs:
                        suitName = "Club";
                        break;
                }
                string rankName = null;
                if (card.Rank < Rank.Jack)
                {
                    rankName = ((int)card.Rank).ToString();
                }
                else
                {
                    switch (card.Rank)
                    {
                        case Rank.Jack:
                            rankName = "J";
                            break;
                        case Rank.Queen:
                            rankName = "Q";
                            break;
                        case Rank.King:
                            rankName = "K";
                            break;
                        case Rank.Ace:
                            rankName = "A";
                            break;
                        default:
                            break;
                    }
                }
                return $"Deck03_{suitName}_{rankName}";
            }
        }

        /// <summary>
        /// prefab 이름으로 카드 생성
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GameObject CreateCard(string prefabName)
        {
            if (cardDict.TryGetValue(prefabName, out var prefab))
            {
                var obj = resolver.Instantiate(prefab);
                return obj;
            }
            else
                throw new Exception($"{prefabName} prefab not found!");
        }

        /// <summary>
        /// 카드들 위치 지정한다.
        /// </summary>
        /// <param name="container"></param>
        public static void SetCardPosition(Transform container)
        {
            int cardCount = container.childCount;
            float startX = 0;
            if (cardCount % 2 == 0)
                startX = (cardCount / 2) * -CardSpace + (0.5f * CardSpace);
            else
                startX = (cardCount / 2) * -CardSpace;

            for (int i = 0; i < cardCount; i++)
            {
                var card = container.GetChild(i);
                card.localPosition = new Vector3(startX + i * CardSpace, 0, 0);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int layerMask = 1 << LayerMask.NameToLayer("Card");
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, layerMask))
            {
                var objCard = hit.collider.GetComponent<ObjectCard>();
                if (objCard != null)
                {
                    if (curPlayr.ClientPlayer.HasHands(objCard))
                    {
                        drawCardSelectPublisher.Publish(new DrawCardSelectMessage(curPlayr, objCard));
                    }
                }
            }
        }

        private void ProcessTableStateMessage(TableStateMessage message)
        {
            Debug.Log($"ProcessTableStateMessage {message}");
            switch (message.Type)
            {
                case TableStateType.Start:
                    if (IsServer) PlayRoundRpc();
                    break;
                case TableStateType.End:
                    EndGameRpc();
                    break;
                case TableStateType.Exit:
                    ExitGameRpc();
                    break;
                default:
                    break;
            }

        }

        void DrawCardSelect(DrawCardSelectMessage message)
        {
            if (currentRound.DrawCardCount > 0)
            {
                //var player = players.Find(p => p.NickName == message.objectPlayer.);
                //if (player != null && !player.IsDraw)
                //{
                //    player.SelectDrawCard(message.objectCard, currentRound.DrawCardCount);
                //    drawCardInfoPublisher.Publish(new DrawInfoMessage(player.DrawsCount));
                //}
            }
        }

        void DrawCards(DrawCardsMessage message)
        {
            var player = ServerPlayerManager.GetServerPlayer(message.ClientId);
            List<Card> cards = new List<Card>();
            if (player.DrawsCount > deck.Count)
            {
                // 더이상 나눠줄 카드가 없으므로 게임을 끝낸다.
                BreakGame();
                return;
            }

            for (int i = 0; i < player.DrawsCount; i++)
            {
                cards.Add(deck.Dequeue());
            }

            player.DrawCards(cards);

            drawResultPublisher.Publish(new DrawResultMessage(player.OwnerClientId, cards));
        }

        /// <summary>
        /// 플레이어가 Turn 에 취한 행동을 처리한다.
        /// </summary>
        /// <param name="message"></param>
        void TurnAction(TurnActionMessage message)
        {
            TurnActionRpc(message.ClientId, message.betting, message.bet);
        }

        private void DrawResult(DrawResultMessage message)
        {
            var cards = message.cards;
            var player = ServerPlayerManager.GetServerPlayer(message.ClientId);
            TaskDrawResult(cards, player);
        }

        private async void TaskDrawResult(List<Card> cards, ServerPlayer serverPlayer)
        {
            await serverPlayer.ClientPlayer.RemoveDrawAsync();

            for (int i = 0; i < cards.Count; i++)
            {
                var card = CreateCard(GetCardName(cards[i]));
                serverPlayer.ClientPlayer.AddCard(card, cards[i].IsFaceUp);
            }
            serverPlayer.ClientPlayer.SetCardPosition();
        }

        private void TurnStart(TurnStartMessage message)
        {
            var serverPlayers = ServerPlayerManager.GetServerPlayers();
            curPlayr = serverPlayers.Find(op => op.OwnerClientId == message.clientId);
        }
    }
}
