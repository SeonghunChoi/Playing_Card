using PlayingCard.GamePlay.Model;
using PlayingCard.GamePlay.View.PlayObject;
using PlayingCard.GamePlay.View.UI;
using PlayingCard.Utilities.Net;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.Presenter.Gamestate
{
    [RequireComponent(typeof(NetcodeHooks))]
    public class ClientGameRoomStateBehaviour : GameStateBehaviour
    {
        [SerializeField]
        NetcodeHooks netcodeHooks;

        public override GameState ActiveState => GameState.GameRoom;


        [SerializeField]
        List<GameObject> cardPrefabs;

        public bool InitialSpawnDone { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            var deckDict = new Dictionary<string, GameObject>();
            foreach (var cardPrefab in cardPrefabs)
                deckDict.Add(cardPrefab.name, cardPrefab);
            builder.RegisterInstance(deckDict);

            builder.RegisterComponentInHierarchy<UIGameRoom>();
            builder.RegisterComponentInHierarchy<UIConfirmBetMoney>();
            builder.RegisterComponentInHierarchy<UIWinner>();
            builder.RegisterComponentInHierarchy<UIMyTurn>();

            builder.RegisterComponentInHierarchy<NetworkGameTable>();

            var gameManager = Parent.Container.Resolve<IGameManager>() as GameManager;
            builder.RegisterInstance(gameManager.SelectedGame).WithParameter("SelectedGame");
        }

        protected override void Awake()
        {
            base.Awake();

            netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawnHook;
        }

        protected override void OnDestroy()
        {
            if (netcodeHooks)
            {
                netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawnHook;
            }

            base.OnDestroy();
        }


        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
            }
        }

        private void OnNetworkDespawnHook()
        {
        }
    }
}
