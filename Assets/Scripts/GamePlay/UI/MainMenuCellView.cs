using EnhancedUI.EnhancedScroller;
using PlayingCard.GamePlay.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PlayingCard.GamePlay.UI
{
    /// <summary>
    /// Main Menu 메뉴 리스트 셀 UI object
    /// </summary>
    public class MainMenuCellView : EnhancedScrollerCellView
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI textName;

        Game game;
        UnityAction<Game> onClickCell;

        private void Awake()
        {
            button.onClick.AddListener(OnClickButton);
        }
        public void Set(Game game, UnityAction<Game> onClickCell)
        {
            this.game = game;
            this.onClickCell = onClickCell;
            textName.text = game.GameName;
        }

        void OnClickButton()
        {
            onClickCell?.Invoke(game);
        }
    }
}
