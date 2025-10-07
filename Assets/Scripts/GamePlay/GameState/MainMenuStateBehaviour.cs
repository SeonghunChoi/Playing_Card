using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;
using PlayingCard.GamePlay.UI;
using PlayingCard.Utilities;
using PlayingCard.Utilities.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.GameState
{
    internal class MainMenuStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.MainMenu;

        [SerializeField]
        MainMenuScroller scroller;
        [SerializeField]
        Button buttonExit;
        [SerializeField]
        Button buttonStart;

        [Inject]
        GameManager gameManager;
        [Inject]
        PlayTable table;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

        }

        protected override void Start()
        {
            base.Start();

            buttonExit.AddOnClickEvent(GameQuit);
            buttonStart.AddOnClickEvent(GameStart);

            gameManager.onGameChanged += GameChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            gameManager.onGameChanged -= GameChanged;
        }

        void GameChanged(Game newGame)
        {
            bool isEmpty = newGame == null;
            buttonStart.interactable = !isEmpty;
        }

        void GameStart()
        {
            if (gameManager.Game != null)
            {
                table.SetGame(gameManager.Game);
                SceneLoaderWarpper.Instance.LoadScene("GameRoom");
            }
        }

        void GameQuit()
        {
            gameManager.QuitGame();
        }
    }
}
