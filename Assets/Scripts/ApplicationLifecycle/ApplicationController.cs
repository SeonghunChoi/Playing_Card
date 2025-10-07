using PlayingCard.GamePlay;
using PlayingCard.GamePlay.PlayModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.ApplicationLifecycle
{
    /// <summary>
    /// 게임의 시작과 종료를 관리한다.
    /// DI 초기화도 여기서 처리한다.
    /// </summary>
    public class ApplicationController : LifetimeScope
    {
        [SerializeField]
        GameManager GameManager;

        public string startSceneName = "MainMenu";
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(GameManager);
            builder.Register<PlayTable>(Lifetime.Singleton);
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(GameManager.gameObject);
            Application.targetFrameRate = 120;
            GameManager.onGameQuit += QuitGame;

            SceneManager.LoadScene(startSceneName);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameManager.onGameQuit -= QuitGame;
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    } 
}
