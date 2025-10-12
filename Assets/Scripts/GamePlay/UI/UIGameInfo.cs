using MessagePipe;
using PlayingCard.GamePlay.Message;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.UI
{
    public class UIGameInfo : MonoBehaviour
    {
        [SerializeField]
        Text textGameName;

        [SerializeField]
        Text textGameDesc;

        private IDisposable selectGameDisposalbe;

        [Inject]
        public void Set(ISubscriber<SelectGameMessage> selectGameSubscriber)
        {
            selectGameDisposalbe = selectGameSubscriber.Subscribe(SelectGame);
        }

        private void SelectGame(SelectGameMessage message)
        {
            var game = message.game;
            textGameName.text = game.GameName;
            textGameDesc.text = game.GameDescription;
        }

        private void OnDestroy()
        {
            selectGameDisposalbe?.Dispose();
        }
    }
}
