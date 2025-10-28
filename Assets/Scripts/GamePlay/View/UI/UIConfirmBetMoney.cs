using Cysharp.Threading.Tasks;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.Utilities.UI;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayingCard.GamePlay.View.UI
{
    /// <summary>
    /// 배팅 Chip 개수 확정 팝업 UI
    /// </summary>
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

        /// <summary>
        /// Betting Chip 최소 개수
        /// </summary>
        ulong minValue;
        /// <summary>
        /// Betting Chip 최대 개수
        /// </summary>
        ulong maxValue;
        /// <summary>
        /// Betting Chip 개수
        /// </summary>
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

        /// <summary>
        /// Chip 개수 받기
        /// </summary>
        /// <param name="player"></param>
        /// <param name="betting"></param>
        /// <param name="lastMaxBet"></param>
        /// <param name="minRaise"></param>
        /// <returns></returns>
        public async UniTask<ulong> GetBetChips(Player player, Betting betting, ulong lastMaxBet, ulong minRaise)
        {
            textTitle.text = betting.ToString().ToUpper();

            ulong callAmount = lastMaxBet - player.Bet;

            minValue = callAmount;
            maxValue = player.Chips;
            bet = minValue;

            SetTextMoney(minValue, maxValue);
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = bet;

            Show();
            tcs = new TaskCompletionSource<ulong>();
            ulong value = await tcs.Task;
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
            Hide();
        }

        private void OnClickNo()
        {
            tcs.TrySetCanceled();
            Hide();
        }
    }
}
