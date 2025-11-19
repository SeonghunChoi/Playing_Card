using Cysharp.Threading.Tasks;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.Utilities.UI;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayingCard.GamePlay.View.UI
{
    public class UIWinner : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup panel;

        [SerializeField]
        TextMeshProUGUI textWinner;

        [SerializeField]
        TextMeshProUGUI textChips;

        [SerializeField]
        Button buttonYes;

        TaskCompletionSource<bool> tcs;

        private void Start()
        {
            buttonYes.AddOnClickEvent(OnClickYes);
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

        public async UniTask<bool> ShowWinner(ulong clientId, ulong chips)
        {
            textWinner.text = $"Player {clientId}";
            textChips.text = chips.ToString("N0");

            Show();
            tcs = new TaskCompletionSource<bool>();
            var value = await tcs.Task;
            return value;
        }

        void OnClickYes()
        {
            tcs.TrySetResult(true);
            Hide();
        }
    }
}
