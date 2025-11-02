using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.Utilities.UI;
using System;
using System.Collections.Generic;
using TMPro;
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

        UIConfirmBetMoney uiConfirmBetMoney;
        UIWinner uiWinner;

        private IPublisher<TableStateMessage> tableStatePublisher;
        private ISubscriber<TurnStartMessage> turnStartSubscriber;
        private IPublisher<TurnActionMessage> turnActionPublisher;
        private IPublisher<DrawCardsMessage> drawCardsPublisher;
        private ISubscriber<DrawInfoMessage> drawInfoSubscriber;
        private IPublisher<SetPlayerCameraMessage> setPlayerCameraPublisher;
        private ISubscriber<WinnerMessage> winnerSubscriber;

        private IDisposable subscription;

        /// <summary>
        /// 현재 TurnAction 플레이어
        /// </summary>
        Player player;
        /// <summary>
        /// 이번 라운드 최대 Betting Chip 개수
        /// </summary>
        ulong lastMaxBet;
        /// <summary>
        /// 최소 Raise Chip 개수
        /// </summary>
        ulong minRaise;

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

            turnStartSubscriber.Subscribe(StartTurn).AddTo(disposableBag);
            drawInfoSubscriber.Subscribe(DrawInfo).AddTo(disposableBag);
            winnerSubscriber.Subscribe(ShowWinner).AddTo(disposableBag);

            subscription = disposableBag.Build();
        }

        [Inject]
        public void Set(
            UIConfirmBetMoney uiConfirmBetMoney,
            UIWinner uiWinner,
            IPublisher<TableStateMessage> tableStatePublisher,
            ISubscriber<TurnStartMessage> turnStartSubscriber,
            IPublisher<TurnActionMessage> turnActionPublisher,            
            IPublisher<DrawCardsMessage> drawCardsPublisher,
            ISubscriber<DrawInfoMessage> drawInfoSubscriber,
            IPublisher<SetPlayerCameraMessage> setPlayerCameraPublisher,
            ISubscriber<WinnerMessage> winnerSubscriber)
        {
            this.uiConfirmBetMoney = uiConfirmBetMoney;
            this.uiWinner = uiWinner;

            this.tableStatePublisher = tableStatePublisher;
            this.turnStartSubscriber = turnStartSubscriber;
            this.turnActionPublisher = turnActionPublisher;
            this.drawCardsPublisher = drawCardsPublisher;
            this.drawInfoSubscriber = drawInfoSubscriber;
            this.setPlayerCameraPublisher = setPlayerCameraPublisher;
            this.winnerSubscriber = winnerSubscriber;
        }

        /// <summary>
        /// Turn 시작 메시지 처리
        /// </summary>
        /// <param name="message"></param>
        private void StartTurn(TurnStartMessage message)
        {
            this.player = message.player;
            this.lastMaxBet = message.LastMaxBet;
            this.minRaise = message.MinRaise;

            SetRoundInfo(message.Pot, message.RoundName);
            SetPlayerInfo(message.player);
            ShowActionButtons(message.player, message.LastMaxBet, message.MinRaise, message.LastBetting);
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
        private void SetPlayerInfo(Player player)
        {
            textPlayerId.text = $"Player_{player.Id}";
            textMoney.text = player.Chips.ToString("N0");
            textBet.text = player.Bet.ToString("N0");
        }

        private void ShowActionButtons(Player player, ulong lastMaxBet, ulong minRaise, Betting lastBetting)
        {
            ulong callAmount = lastMaxBet - player.Bet;

            textMaxBet.text = lastMaxBet.ToString("N0");

            buttonFold.gameObject.SetActive(!player.State.HasActed());
            buttonCheck.gameObject.SetActive(!player.State.HasActed() && callAmount == 0);
            buttonBet.gameObject.SetActive(!player.State.HasActed() && callAmount == 0 && lastMaxBet == 0);
            buttonCall.gameObject.SetActive(!player.State.HasActed() && callAmount > 0 && player.Chips >= callAmount);
            buttonRaise.gameObject.SetActive(!player.State.HasActed() && callAmount > 0 && player.Chips > callAmount + minRaise);
            buttonAllIn.gameObject.SetActive(!player.State.HasActed() && callAmount > 0 && player.Chips > 0);

            buttonDraw.gameObject.SetActive(false);
        }

        private void OnClickExit()
        {
            tableStatePublisher.Publish(new TableStateMessage(TableStateType.Exit));
        }

        private void OnClickDraw()
        {
            drawCardsPublisher.Publish(new DrawCardsMessage(player));
            buttonDraw.gameObject.SetActive(false);
        }

        private void OnClickFold()
        {
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Fold, 0));
        }

        private void OnClickCheck()
        {
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Check, 0));
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
                ulong bet = await uiConfirmBetMoney.GetBetChips(player, Betting.Bet, lastMaxBet, minRaise);
                canvas.interactable = true;
                turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Bet, bet));
            }
            catch (OperationCanceledException)
            {
                canvas.interactable = true;
                Debug.Log("Cancel");
            }
        }

        private void OnClickCall()
        {
            ulong callAmount = lastMaxBet - player.Bet;
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Call, callAmount));
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
                canvas.interactable = false;
                ulong callAmount = lastMaxBet - player.Bet;
                ulong bet = await uiConfirmBetMoney.GetBetChips(player, Betting.Raise, callAmount + minRaise, minRaise);
                canvas.interactable = true;
                turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Raise, bet));
            }
            catch (OperationCanceledException)
            {
                canvas.interactable = true;
                Debug.Log("Cancel");
            }
        }

        private void OnClickAllIn()
        {
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.AllIn, player.Chips));
        }

        /// <summary>
        /// 승리자 확인 Message 처리
        /// </summary>
        /// <param name="message"></param>
        private void ShowWinner(WinnerMessage message)
        {
            var winners = message.winners;
            TaskShowWinner(winners);
        }

        /// <summary>
        /// 승리자 확인 UI 표시
        /// </summary>
        /// <param name="winners"></param>
        private async void TaskShowWinner(Dictionary<Player, ulong> winners)
        {
            canvas.interactable = false;
            foreach (var player in winners.Keys)
            {
                setPlayerCameraPublisher.Publish(new SetPlayerCameraMessage(player));
                ulong chips = winners[player];
                await uiWinner.ShowWinner(player, chips);
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
