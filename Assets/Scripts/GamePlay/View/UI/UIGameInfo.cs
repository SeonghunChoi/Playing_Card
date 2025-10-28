using MessagePipe;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Message;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    public class UIGameInfo : MonoBehaviour
    {
        [SerializeField]
        Text textGameName;

        [SerializeField]
        Text textGameDesc;

        private List<Game> gameList;
        private IDisposable messageDisposable;

        [Inject]
        public void Set(
            List<Game> gameList,
            ISubscriber<MainMenuMessage> messageSubscriber)
        {
            this.gameList = gameList;
            messageDisposable = messageSubscriber.Subscribe(SelectGame);
        }

        private void SelectGame(MainMenuMessage message)
        {
            if (message.messageType != MainMenuMessageType.Menu) return;

            var game = gameList[message.value];
            textGameName.text = game.GameName;
            textGameDesc.text = game.GameDescription;
        }

        private void OnDestroy()
        {
            messageDisposable?.Dispose();
        }
    }
}
