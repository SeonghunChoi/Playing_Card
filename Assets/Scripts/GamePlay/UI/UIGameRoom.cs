using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.Utilities.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.UI
{
    public class UIGameRoom : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI textPlayerId;
        [SerializeField]
        TextMeshProUGUI textMoney;
        [SerializeField]
        TextMeshProUGUI textBet;
        [SerializeField]
        TextMeshProUGUI textPot;
        [SerializeField]
        TextMeshProUGUI textRound;
        [SerializeField]
        Button buttonExit;

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

        [SerializeField]
        UIConfirmBetMoney uiConfirmBetMoney;

        private IDisposable turnStartDisposable;
        private IPublisher<TurnActionMessage> turnActionPublisher;
        private IPublisher<ExitGameMessage> exitGamePublisher;

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

            buttonFold.AddOnClickEvent(OnClickFold);
            buttonCheck.AddOnClickEvent(OnClickCheck);
            buttonBet.AddOnClickEvent(OnClickBet);
            buttonCall.AddOnClickEvent(OnClickCall);
            buttonRaise.AddOnClickEvent(OnClickRaise);
            buttonAllIn.AddOnClickEvent(OnClickAllIn);
        }

        [Inject]
        public void Set(
            ISubscriber<TurnStartMessage> turnStartSubscriber,
            IPublisher<TurnActionMessage> turnActionPublisher,
            IPublisher<ExitGameMessage> exitGamePublisher)
        {
            turnStartDisposable = turnStartSubscriber.Subscribe(StartTurn);
            this.turnActionPublisher = turnActionPublisher;
            this.exitGamePublisher = exitGamePublisher;
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

            SetRoundInfo(message.Pot, message.Round);
            SetPlayerInfo(message.player);
            ShowActionButtons(message.player, message.LastMaxBet, message.MinRaise, message.LastBetting);
        }

        /// <summary>
        /// 라운드 정보 처리
        /// </summary>
        /// <param name="pot"></param>
        /// <param name="round"></param>
        private void SetRoundInfo(ulong pot, int round)
        {
            textPot.text = pot.ToString("N0");
            textRound.text = $"Round: {round}";
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

            buttonFold.gameObject.SetActive(player.State.IsBetable(lastBetting));
            buttonCheck.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount == 0);
            buttonBet.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount == 0 && lastMaxBet == 0);
            buttonCall.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Chips >= callAmount);
            buttonRaise.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Chips > callAmount + minRaise);
            buttonAllIn.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Chips > 0);
        }

        private void OnClickExit()
        {
            exitGamePublisher.Publish(new ExitGameMessage());
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
                ulong bet = await uiConfirmBetMoney.GetBetChips(player, Betting.Bet, lastMaxBet, minRaise);
                turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Bet, bet));
            }
            catch (OperationCanceledException)
            {
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
                ulong callAmount = lastMaxBet - player.Bet;
                ulong bet = await uiConfirmBetMoney.GetBetChips(player, Betting.Raise, callAmount + minRaise, minRaise);
                turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Raise, bet));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Cancel");
            }
        }

        private void OnClickAllIn()
        {
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.AllIn, player.Chips));
        }

        private void OnDestroy()
        {
            turnStartDisposable?.Dispose();
        }
    }
}
