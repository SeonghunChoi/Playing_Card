using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.Model.Message;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.GamePlay.View.PlayObject;
using PlayingCard.GamePlay.View.UI;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter.GameState
{
    /// <summary>
    /// GameRoom Scene에서 한정적으로 사용할 Container 설정
    /// </summary>
    public class GameRoomStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.GameRoom;

        [SerializeField]
        List<GameObject> cardPrefabs;

        private IPublisher<TableStateMessage> startGamePublisher;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            MessagePipeOptions options = Parent.Container.Resolve<MessagePipeOptions>();
            builder.RegisterMessageBroker<TableStateMessage>(options);

            builder.RegisterMessageBroker<TurnStartMessage>(options);
            builder.RegisterMessageBroker<TurnActionMessage>(options);
            builder.RegisterMessageBroker<DealCardMessage>(options);
            builder.RegisterMessageBroker<WinnerMessage>(options);
            builder.RegisterMessageBroker<DrawInfoMessage>(options);
            builder.RegisterMessageBroker<DrawCardSelectMessage>(options);
            builder.RegisterMessageBroker<DrawCardsMessage>(options);
            builder.RegisterMessageBroker<DrawResultMessage>(options);

            builder.RegisterComponentInHierarchy<UIConfirmBetMoney>();
            builder.RegisterComponentInHierarchy<UIWinner>();
            var deckDict = new Dictionary<string, GameObject>();
            foreach (var cardPrefab in cardPrefabs)
                deckDict.Add(cardPrefab.name, cardPrefab);

            builder.RegisterInstance(deckDict);

            var gameManager = Parent.Container.Resolve<IGameManager>() as GameManager;
            builder.RegisterInstance(gameManager.SelectedGame).WithParameter("SelectedGame");
            builder.RegisterInstance(new HandRankingManager()).AsImplementedInterfaces();

            // Manager 등록
            builder.Register<PlayTable>(Lifetime.Scoped).AsImplementedInterfaces();
        }

        /// <summary>
        /// GameRoom Scene 시작 시 처리할 설정
        /// </summary>
        protected override void Start()
        {
            base.Start();

            startGamePublisher = Container.Resolve<IPublisher<TableStateMessage>>();
            startGamePublisher.Publish(new TableStateMessage(TableStateType.Start));
        }
    }
}
