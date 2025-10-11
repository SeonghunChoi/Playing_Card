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

        Player player;
        ulong lastMaxBet;
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

        private void StartTurn(TurnStartMessage message)
        {
            this.player = message.player;
            this.lastMaxBet = message.LastMaxBet;
            this.minRaise = message.MinRaise;

            Init(message.Pot, message.Round);
            SetPlayerInfo(message.player);
            SetActionButtons(message.player, message.LastMaxBet, message.MinRaise, message.LastBetting);
        }

        private void Init(ulong pot, int round)
        {
            textPot.text = pot.ToString("N0");
            textRound.text = $"Round: {round}";
        }

        private void SetPlayerInfo(Player player)
        {
            textPlayerId.text = $"Player_{player.Id}";
            textMoney.text = player.Money.ToString("N0");
            textBet.text = player.Bet.ToString("N0");
        }

        private void SetActionButtons(Player player, ulong lastMaxBet, ulong minRaise, Betting lastBetting)
        {
            ulong callAmount = lastMaxBet - player.Bet;

            buttonFold.gameObject.SetActive(player.State.IsBetable(lastBetting));
            buttonCheck.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount == 0);
            buttonBet.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount == 0 && lastMaxBet == 0);
            buttonCall.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Money >= callAmount);
            buttonRaise.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Money > callAmount + minRaise);
            buttonAllIn.gameObject.SetActive(player.State.IsBetable(lastBetting) && callAmount > 0 && player.Money > 0);
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

        private async void TaskBet()
        {
            try
            {
                ulong bet = await uiConfirmBetMoney.GetBetMoney(player, Betting.Bet, lastMaxBet, minRaise);
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

        private async void TaskRaise()
        {
            try
            {
                ulong callAmount = lastMaxBet - player.Bet;
                ulong bet = await uiConfirmBetMoney.GetBetMoney(player, Betting.Raise, callAmount + minRaise, minRaise);
                turnActionPublisher.Publish(new TurnActionMessage(player, Betting.Raise, bet));
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Cancel");
            }
        }

        private void OnClickAllIn()
        {
            turnActionPublisher.Publish(new TurnActionMessage(player, Betting.AllIn, player.Money));
        }

        private void OnDestroy()
        {
            turnStartDisposable?.Dispose();
        }
    }
}
