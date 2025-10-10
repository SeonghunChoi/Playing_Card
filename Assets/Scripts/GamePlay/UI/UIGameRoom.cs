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

        private IDisposable turnStartDisposable;

        IPlayTable table;

        Player player;

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
            IPlayTable table, 
            ISubscriber<TrunStartMessage> turnStartSubscriber)
        {
            this.table = table;
            turnStartDisposable = turnStartSubscriber.Subscribe(StartTurn);

            Init(table);
        }

        private void StartTurn(TrunStartMessage message)
        {
            player = message.player;

            SetPlayerInfo(player);
            SetActionButtons();
        }

        private void Init(IPlayTable table)
        {
            textPot.text = table.Pot.ToString("N0");
            textRound.text = $"Round: {table.Round}";
        }

        private void SetPlayerInfo(Player player)
        {
            textPlayerId.text = $"Player_{player.Id}";
            textMoney.text = player.Money.ToString("N0");
            textBet.text = player.Bet.ToString("N0");
        }

        private void SetActionButtons()
        {
            ulong callAmount = table.MaxBet - player.Bet;
            ulong remainMoney = player.Money - player.Bet;

            buttonFold.gameObject.SetActive(player.State.IsBetable());
            buttonCheck.gameObject.SetActive(player.State.IsBetable() && callAmount == 0);
            buttonBet.gameObject.SetActive(player.State.IsBetable() && callAmount == 0 && table.MaxBet == 0);
            buttonCall.gameObject.SetActive(player.State.IsBetable() && callAmount > 0 && remainMoney >= callAmount);
            buttonRaise.gameObject.SetActive(player.State.IsBetable() && callAmount > 0 && remainMoney > callAmount + table.MinRiase);
            buttonAllIn.gameObject.SetActive(player.State.IsBetable() && callAmount > 0 && remainMoney > 0);
        }

        private void OnClickExit()
        {
            table.ExitGame();
        }

        private void OnClickFold()
        {
            player.SetState(PlayerState.Folded);
        }

        private void OnClickCheck()
        {

        }

        private void OnClickBet()
        {

        }

        private void OnClickCall()
        {

        }

        private void OnClickRaise()
        {

        }

        private void OnClickAllIn()
        {

        }

        private void OnDestroy()
        {
            turnStartDisposable?.Dispose();
        }
    }
}
