using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PlayingCard.GamePlay.View.UI
{
    public class UIMyTurn : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup panel;

        public void Show()
        {
            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }

        public void Hide()
        {
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        public async UniTask ShowTurn()
        {
            Show();

            await UniTask.WaitForSeconds(2f, cancellationToken: this.GetCancellationTokenOnDestroy());

            Hide();
        }
    }
}
