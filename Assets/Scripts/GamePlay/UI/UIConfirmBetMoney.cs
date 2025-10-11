using Cysharp.Threading.Tasks;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.Utilities.UI;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayingCard.GamePlay.UI
{
    public class UIConfirmBetMoney : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup panel;
        [SerializeField]
        TextMeshProUGUI textTitle;
        [SerializeField]
        TextMeshProUGUI textMoney;
        [SerializeField]
        Slider slider;
        [SerializeField]
        Button buttonLeft;
        [SerializeField]
        Button buttonRight;
        [SerializeField]
        Button buttonYes;
        [SerializeField]
        Button buttonNo;

        TaskCompletionSource<ulong> tcs;

        ulong minValue;
        ulong maxValue;
        ulong bet;

        private void Start()
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            buttonLeft.AddOnClickEvent(OnClickLeft);
            buttonRight.AddOnClickEvent(OnClickRight);
            buttonYes.AddOnClickEvent(OnClickYes);
            buttonNo.AddOnClickEvent(OnClickNo);
        }

        void Show()
        {
            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }

        void Hide()
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        public async UniTask<ulong> GetBetMoney(Player player, Betting betting, ulong lastMaxBet, ulong minRaise)
        {
            textTitle.text = betting.ToString().ToUpper();

            ulong callAmount = lastMaxBet - player.Bet;

            minValue = callAmount;
            maxValue = player.Money;
            bet = minValue;

            SetTextMoney(minValue, maxValue);
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = bet;

            Show();
            tcs = new TaskCompletionSource<ulong>();
            ulong value = await tcs.Task;
            Hide();
            return value;
        }

        private void SetTextMoney(ulong minValue, ulong maxValue)
        {
            textMoney.text = $"<color=yellow>{minValue.ToString("N0")}</color>/<color=white>{maxValue.ToString("N0")}</color>";
        }

        private void OnSliderValueChanged(float value)
        {
            bet = (ulong)value;
            if (bet < minValue)
                bet = minValue;
            else if (bet > maxValue)
                bet = maxValue;
            SetTextMoney(bet, maxValue);
        }

        private void OnClickLeft()
        {
            if (bet == ulong.MinValue) return;
            bet--;
            if (bet < minValue)
                bet = minValue;
            SetTextMoney(bet, maxValue);
            slider.value = bet;
        }

        private void OnClickRight()
        {
            if (bet == ulong.MaxValue) return;
            bet++;
            if (bet > maxValue)
                bet = maxValue;
            SetTextMoney(bet, maxValue);
            slider.value = bet;
        }

        private void OnClickYes()
        {
            tcs.TrySetResult(bet);
        }

        private void OnClickNo()
        {
            tcs.TrySetCanceled();
            Hide();
        }
    }
}
