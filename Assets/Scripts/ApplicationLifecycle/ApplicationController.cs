using MessagePipe;
using PlayingCard.GamePlay;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Configuration.Define;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.PlayModels;
using System.Collections.Generic;
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
        List<Game> GameList;

        public string startSceneName = DefineScene.MAIN_MENU;

        private ISubscriber<QuitGameMessage> quitGameSubscriber;

        private IGameManager gameManager;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // MessageBroker 등록
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            builder.RegisterMessageBroker<QuitGameMessage>(options);
            builder.RegisterMessageBroker<SelectGameMessage>(options);
            builder.RegisterMessageBroker<StartGameMessage>(options);
            builder.RegisterMessageBroker<EndGameMessage>(options);
            builder.RegisterMessageBroker<ExitGameMessage>(options);
            builder.RegisterMessageBroker<TurnStartMessage>(options);
            builder.RegisterMessageBroker<TurnActionMessage>(options);

            // Manager 등록
            builder.RegisterInstance(GameList);
            builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<HandRankingManager>(Lifetime.Singleton);
            builder.Register<PlayTable>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;

            // 게임 종료 Message 구독
            var disposableBag = DisposableBag.CreateBuilder();
            quitGameSubscriber = Container.Resolve<ISubscriber<QuitGameMessage>>();
            quitGameSubscriber.Subscribe(x => QuitGame(x)).AddTo(disposableBag);

            gameManager = Container.Resolve<IGameManager>();

            SceneManager.LoadScene(startSceneName);
        }

        private void QuitGame(QuitGameMessage message)
        {
            gameManager.Dispose();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    } 
}
