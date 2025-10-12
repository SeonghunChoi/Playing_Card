using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.GameState
{
    /// <summary>
    /// GameRoom Scene에서 한정적으로 사용할 Container 설정
    /// </summary>
    public class GameRoomStateBehaviour : GameStateBehaviour
    {
        public override GameState ActiveState => GameState.GameRoom;

        [SerializeField]
        List<GameObject> cardPrefabs;

        private IPublisher<StartGameMessage> startGamePublisher;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponentInHierarchy<UIConfirmBetMoney>();
            builder.RegisterComponentInHierarchy<UIWinner>();
            var deckDict = new Dictionary<string, GameObject>();
            foreach (var cardPrefab in cardPrefabs)
                deckDict.Add(cardPrefab.name, cardPrefab);

            builder.RegisterInstance(deckDict);
        }

        /// <summary>
        /// GameRoom Scene 시작 시 처리할 설정
        /// </summary>
        protected override void Start()
        {
            base.Start();

            startGamePublisher = Container.Resolve<IPublisher<StartGameMessage>>();
            startGamePublisher.Publish(new StartGameMessage());
        }
    }
}
