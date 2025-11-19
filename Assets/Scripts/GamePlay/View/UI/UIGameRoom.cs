using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.Utilities.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    /// <summary>
    /// Game Room 용 UI
    /// </summary>
    public class UIGameRoom : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup canvas;

        [SerializeField]
        TextMeshProUGUI textPlayerId;
        [SerializeField]
        TextMeshProUGUI textMoney;
        [SerializeField]
        TextMeshProUGUI textBet;
        [SerializeField]
        TextMeshProUGUI textPot;
        [SerializeField]
        TextMeshProUGUI textMaxBet;
        [SerializeField]
        TextMeshProUGUI textRound;
        [SerializeField]
        Button buttonExit;

        [SerializeField]
        Button buttonDraw;

        [SerializeField]
        Button buttonFold;
        [SerializeField]
        Button buttonCheck;
        [SerializeField]
        Button buttonBet;
        [SerializeField]
        Button buttonCall;
        [SerializeField]
        Button buttonRaise;
        [SerializeField]
        Button buttonAllIn;

        List<Button> actionButtons;

        UIConfirmBetMoney uiConfirmBetMoney;
        UIWinner uiWinner;
        UIMyTurn uiMyTurn;

        private IPublisher<TableStateMessage> tableStatePublisher;
        private ISubscriber<GameRoomInfoMessage> gameRoomInfoSubscriber;
        private ISubscriber<TurnStartMessage> turnStartSubscriber;
        private ISubscriber<RoundCompleteMessage> roundCompleteSubscriber;

        private IPublisher<TurnActionMessage> turnActionPublisher;

        private IPublisher<DrawCardsMessage> drawCardsPublisher;
        private ISubscriber<DrawInfoMessage> drawInfoSubscriber;
        private ISubscriber<WinnerMessage> winnerSubscriber;

        private IDisposable subscription;

        /// <summary>
        /// 이번 라운드 최대 Betting Chip 개수
        /// </summary>
        ulong lastMaxBet;
        /// <summary>
        /// 최소 Raise Chip 개수
        /// </summary>
        ulong minRaise;

        private void Awake()
        {
            actionButtons = new List<Button>() { buttonFold, buttonCheck,  buttonBet, buttonCall, buttonRaise, buttonAllIn };
        }

        private void Start()
        {
            buttonExit.AddOnClickEvent(OnClickExit);

            buttonDraw.AddOnClickEvent(OnClickDraw);

            buttonFold.AddOnClickEvent(OnClickFold);
            buttonCheck.AddOnClickEvent(OnClickCheck);
            buttonBet.AddOnClickEvent(OnClickBet);
            buttonCall.AddOnClickEvent(OnClickCall);
            buttonRaise.AddOnClickEvent(OnClickRaise);
            buttonAllIn.AddOnClickEvent(OnClickAllIn);

            var disposableBag = DisposableBag.CreateBuilder();

            gameRoomInfoSubscriber.Subscribe(ProcessGameRoomInfo).AddTo(disposableBag);
            turnStartSubscriber.Subscribe(StartTurn).AddTo(disposableBag);
            roundCompleteSubscriber.Subscribe(ProcessRoundComplete).AddTo(disposableBag);
            drawInfoSubscriber.Subscribe(DrawInfo).AddTo(disposableBag);
            winnerSubscriber.Subscribe(ShowWinner).AddTo(disposableBag);

            subscription = disposableBag.Build();
        }

        [Inject]
        public void Set(
            UIConfirmBetMoney uiConfirmBetMoney,
            UIWinner uiWinner,
            UIMyTurn uiMyTurn,
            IPublisher<TableStateMessage> tableStatePublisher,
            ISubscriber<GameRoomInfoMessage> gameRoomInfoSubscriber,
            ISubscriber<TurnStartMessage> turnStartSubscriber,
            ISubscriber<RoundCompleteMessage> roundCompleteSubscriber,

            IPublisher<TurnActionMessage> turnActionPublisher,            

            IPublisher<DrawCardsMessage> drawCardsPublisher,
            ISubscriber<DrawInfoMessage> drawInfoSubscriber,
            ISubscriber<WinnerMessage> winnerSubscriber)
        {
            this.uiConfirmBetMoney = uiConfirmBetMoney;
            this.uiWinner = uiWinner;
            this.uiMyTurn = uiMyTurn;

            this.tableStatePublisher = tableStatePublisher;
            this.gameRoomInfoSubscriber = gameRoomInfoSubscriber;
            this.turnStartSubscriber = turnStartSubscriber;
            this.roundCompleteSubscriber = roundCompleteSubscriber;

            this.turnActionPublisher = turnActionPublisher;

            this.drawCardsPublisher = drawCardsPublisher;
            this.drawInfoSubscriber = drawInfoSubscriber;
            this.winnerSubscriber = winnerSubscriber;
        }

        /// <summary>
        /// 라운드 정보 처리
        /// </summary>
        /// <param name="pot"></param>
        /// <param name="round"></param>
        private void SetRoundInfo(ulong pot, string roundName)
        {
            textPot.text = pot.ToString("N0");
            textRound.text = roundName;
        }

        /// <summary>
        /// 플레이어 정보 처리
        /// </summary>
        /// <param name="player"></param>
        [Rpc(SendTo.ClientsAndHost)]
        public void SetPlayerInfoRpc(ulong clientId, string nickName, ulong chips, ulong bet)
        {
            Debug.Log($"SetPlayerInfoRpc - clientId:{clientId}, nickName:{nickName}, chips:{chips}, bet:{bet}");
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                textPlayerId.text = nickName;
                textMoney.text = chips.ToString("N0");
                textBet.text = bet.ToString("N0");
            }
        }

        public void UpdateActionButtons()
        {
            ShowActionButtons(minRaise);
        }
        private void ShowActionButtons(ulong minRaise)
        {
            var clientId = NetworkManager.Singleton.LocalClientId;
            var clientPlayer = ClientPlayerManager.GetClientPlayer(clientId);
            ulong callAmount = lastMaxBet - clientPlayer.ServerPlayer.RoundBet.Value;
            if (lastMaxBet < clientPlayer.ServerPlayer.RoundBet.Value)
                callAmount = 0;
            Debug.Log($"[ShowActionButtons] clientId:{clientId}, State:{clientPlayer.ServerPlayer.State.Value}, Chips:{clientPlayer.ServerPlayer.Chips.Value}, callAmount:{callAmount} = lastMaxBet:{lastMaxBet} - bet:{clientPlayer.ServerPlayer.RoundBet.Value};\n");

            textMaxBet.text = lastMaxBet.ToString("N0");

            buttonFold.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed());
            buttonCheck.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed() && callAmount == 0);
            buttonBet.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed() && callAmount == 0 && lastMaxBet == 0);
            buttonCall.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed() && callAmount > 0 && clientPlayer.ServerPlayer.Chips.Value >= callAmount);
            buttonRaise.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed() && callAmount > 0 && clientPlayer.ServerPlayer.Chips.Value > callAmount + minRaise);
            buttonAllIn.gameObject.SetActive(!clientPlayer.ServerPlayer.State.Value.HasActed() && callAmount > 0 && clientPlayer.ServerPlayer.Chips.Value > 0);

            buttonDraw.gameObject.SetActive(false);
        }

        private void SetButtonsInteraction(bool isMyturn)
        {
            buttonDraw.interactable = isMyturn;
            buttonFold.interactable = isMyturn;
            buttonCheck.interactable = isMyturn;
            buttonBet.interactable = isMyturn;
            buttonCall.interactable = isMyturn;
            buttonRaise.interactable = isMyturn;
            buttonAllIn.interactable = isMyturn;

            var buttonColors = buttonFold.colors;
            foreach (var actionButton in actionButtons)
            {
                if (actionButton.interactable)
                    actionButton.targetGraphic.color = buttonColors.normalColor;
                else
                    actionButton.targetGraphic.color = buttonColors.disabledColor;
            }

            if (isMyturn) ShowMyTurn();
        }

        private async void ShowMyTurn()
        {
            try
            {
                await uiMyTurn.ShowTurn();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnClickExit()
        {
            tableStatePublisher.Publish(new TableStateMessage(TableStateType.Exit));
        }

        private void OnClickDraw()
        {
            drawCardsPublisher.Publish(new DrawCardsMessage(NetworkManager.Singleton.LocalClientId));
            buttonDraw.gameObject.SetActive(false);
        }

        private void OnClickFold()
        {
            turnActionPublisher.Publish(new TurnActionMessage(NetworkManager.Singleton.LocalClientId, Betting.Fold, 0));
        }

        private void OnClickCheck()
        {
            turnActionPublisher.Publish(new TurnActionMessage(NetworkManager.Singleton.LocalClientId, Betting.Check, 0));
        }

        private void OnClickBet()
        {
            TaskBet();
        }

        /// <summary>
        /// Betting Chip 개수 처리
        /// </summary>
        private async void TaskBet()
        {
            try
            {
                canvas.interactable = false;
                ulong bet = await uiConfirmBetMoney.GetBetChips(NetworkManager.Singleton.LocalClientId, Betting.Bet, lastMaxBet, minRaise);
                canvas.interactable = true;
                turnActionPublisher.Publish(new TurnActionMessage(NetworkManager.Singleton.LocalClientId, Betting.Bet, bet));
            }
            catch (OperationCanceledException)
            {
                canvas.interactable = true;
                Debug.Log("Cancel");
            }
        }

        private void OnClickCall()
        {
            var clientPlayer = ClientPlayerManager.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
            ulong callAmount = lastMaxBet - clientPlayer.ServerPlayer.RoundBet.Value;
            if (lastMaxBet < clientPlayer.ServerPlayer.RoundBet.Value)
                callAmount = 0;
            Debug.Log($"OnClickCall - callAmount:{callAmount} = lastMaxBet:{lastMaxBet} - MyBet:{clientPlayer.ServerPlayer.RoundBet.Value}");
            turnActionPublisher.Publish(new TurnActionMessage(clientPlayer.ServerPlayer.OwnerClientId, Betting.Call, callAmount));
        }

        private void OnClickRaise()
        {
            TaskRaise();
        }

        /// <summary>
        /// Raise Chip 개수 처리
        /// </summary>
        private async void TaskRaise()
        {
            try
            {
                var clientPlayer = ClientPlayerManager.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
                canvas.interactable = false;
                ulong callAmount = lastMaxBet - clientPlayer.ServerPlayer.RoundBet.Value;
                if (lastMaxBet < clientPlayer.ServerPlayer.RoundBet.Value)
                    callAmount = 0;
                ulong bet = await uiConfirmBetMoney.GetBetChips(clientPlayer.ServerPlayer.OwnerClientId, Betting.Raise, callAmount + minRaise, minRaise);
                canvas.interactable = true;
                turnActionPublisher.Publish(new TurnActionMessage(clientPlayer.ServerPlayer.OwnerClientId, Betting.Raise, bet));
            }
            catch (OperationCanceledException)
            {
                canvas.interactable = true;
                Debug.Log("Cancel");
            }
        }

        private void OnClickAllIn()
        {
            var clientPlayer = ClientPlayerManager.GetClientPlayer(NetworkManager.Singleton.LocalClientId);
            turnActionPublisher.Publish(new TurnActionMessage(clientPlayer.ServerPlayer.OwnerClientId, Betting.AllIn, clientPlayer.ServerPlayer.Chips.Value));
        }

        /// <summary>
        /// Turn 시작 메시지 처리
        /// </summary>
        /// <param name="message"></param>
        private void StartTurn(TurnStartMessage message)
        {
            this.lastMaxBet = message.LastMaxBet;
            this.minRaise = message.MinRaise;

            SetRoundInfo(message.Pot, message.RoundName);
            ShowActionButtons(message.MinRaise/*, message.LastBetting*/);
            SetButtonsInteraction(message.clientId == NetworkManager.Singleton.LocalClientId);

        }

        private void ProcessGameRoomInfo(GameRoomInfoMessage message)
        {
            if (NetworkManager.Singleton.LocalClientId == message.ClientId)
            {
                this.lastMaxBet = message.LastMaxBet;
                minRaise = message.MinRaise;
                SetRoundInfo(message.Pot, message.RoundName);
                ShowActionButtons(message.MinRaise/*, message.LastBetting*/);
                //SetButtonsInteraction(message.isMyTurn);
            }
        }

        private void ProcessRoundComplete(RoundCompleteMessage message)
        {
            this.lastMaxBet = 0;
        }

        /// <summary>
        /// 승리자 확인 Message 처리
        /// </summary>
        /// <param name="message"></param>
        private void ShowWinner(WinnerMessage message)
        {
            TaskShowWinner(message.clientId, message.winChips);
        }

        /// <summary>
        /// 승리자 확인 UI 표시
        /// </summary>
        /// <param name="winners"></param>
        private async void TaskShowWinner(ulong clientId, ulong winChips)
        {
            canvas.interactable = false;
            await uiWinner.ShowWinner(clientId, winChips);
            canvas.interactable = true;
            tableStatePublisher.Publish(new TableStateMessage(TableStateType.End));
        }

        /// <summary>
        /// 승리자 확인 UI 표시
        /// </summary>
        /// <param name="winners"></param>
        private async void TaskShowWinner(Dictionary<ulong, ulong> winners)
        {
            canvas.interactable = false;
            foreach (var clientId in winners.Keys)
            {
                ulong chips = winners[clientId];
                await uiWinner.ShowWinner(clientId, chips);
            }
            canvas.interactable = true;
            tableStatePublisher.Publish(new TableStateMessage(TableStateType.End));
        }

        private void DrawInfo(DrawInfoMessage message)
        {
            buttonDraw.gameObject.SetActive(message.DrawCardCount > 0);
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
        }
    }
}
