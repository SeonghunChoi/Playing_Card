using MessagePipe;
using PlayingCard.ApplicationLifecycle.Message;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.GamePlay.Model.PlayModels;
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

        private ISubscriber<QuitApplicationMessage> quitGameSubscriber;

        //private IGameManager gameManager;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // MessageBroker 등록
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            builder.RegisterMessageBroker<QuitApplicationMessage>(options);

            //builder.RegisterMessageBroker<StartGameMessage>(options);
            //builder.RegisterMessageBroker<ExitGameMessage>(options);
            //builder.RegisterMessageBroker<EndGameMessage>(options);
            //builder.RegisterMessageBroker<TurnStartMessage>(options);
            //builder.RegisterMessageBroker<TurnActionMessage>(options);
            //builder.RegisterMessageBroker<DealCardMessage>(options);
            //builder.RegisterMessageBroker<WinnerMessage>(options);
            //builder.RegisterMessageBroker<DrawInfoMessage>(options);
            //builder.RegisterMessageBroker<DrawCardSelectMessage>(options);
            //builder.RegisterMessageBroker<DrawCardsMessage>(options);
            //builder.RegisterMessageBroker<DrawResultMessage>(options);

            // Manager 등록
            builder.RegisterInstance(GameList);
            builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();
            //builder.Register<HandRankingManager>(Lifetime.Singleton);
            //builder.Register<PlayTable>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 120;

            // 게임 종료 Message 구독
            var disposableBag = DisposableBag.CreateBuilder();
            quitGameSubscriber = Container.Resolve<ISubscriber<QuitApplicationMessage>>();
            quitGameSubscriber.Subscribe(x => QuitGame(x)).AddTo(disposableBag);

            SceneManager.LoadScene(startSceneName);
        }

        private void QuitGame(QuitApplicationMessage message)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    } 
}
