using MessagePipe;
using PlayingCard.ApplicationLifecycle.Message;
using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Configuration.Define;
using PlayingCard.Utilities;
using System;
using System.Collections.Generic;
using Unity.Netcode;
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
        NetworkManager networkManager;
        [SerializeField]
        ConnectionManager connectionManager;

        [SerializeField] 
        List<Game> GameList;

        public string startSceneName = DefineScene.MAIN_MENU;

        private ISubscriber<QuitApplicationMessage> quitGameSubscriber;

        private IDisposable subscription;

        List<string> Names = new List<string> { "Ace", "Jack", "Queen", "King", "Joker" };

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // MessageBroker 등록
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            builder.RegisterMessageBroker<QuitApplicationMessage>(options);

            // Manager 등록
            builder.RegisterComponent(connectionManager);
            builder.RegisterComponent(networkManager);

            builder.Register<ProfileManager>(Lifetime.Singleton);

            builder.RegisterInstance(GameList);
            builder.Register<GameManager>(Lifetime.Singleton).AsImplementedInterfaces();

            var name = $"{Names[UnityEngine.Random.Range(0, Names.Count)]}_{UnityEngine.Random.Range(0, 10)}";
            builder.RegisterInstance(name).WithParameter("nickname", name);
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(networkManager.gameObject);
            Application.targetFrameRate = 120;

            // 게임 종료 Message 구독
            var disposableBag = DisposableBag.CreateBuilder();
            quitGameSubscriber = Container.Resolve<ISubscriber<QuitApplicationMessage>>();
            quitGameSubscriber.Subscribe(x => QuitGame(x)).AddTo(disposableBag);
            subscription = disposableBag.Build();

            SceneManager.LoadScene(startSceneName);
        }

        private void QuitGame(QuitApplicationMessage message)
        {
            subscription?.Dispose();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    } 
}
