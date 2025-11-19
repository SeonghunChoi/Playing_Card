using PlayingCard.GamePlay.Presenter.Gamestate;
using PlayingCard.GamePlay.View.UI;
using PlayingCard.LobbyManagement.NetModels;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.Model.PlayModels
{
    public class ServerPlayer : NetworkBehaviour
    {
        [SerializeField]
        ClientPlayer clientPlayer;

        UIGameRoom uiGameRoom;

        public ClientPlayer ClientPlayer
        {
            get { return clientPlayer; }
        }

        public NetworkVariable<NetworkString> NickName = new NetworkVariable<NetworkString>();
        public NetworkVariable<ulong> Chips = new NetworkVariable<ulong>();
        /// <summary>
        /// 게임 동안 Betting 한 칩 개수
        /// </summary>
        public NetworkVariable<ulong> Bet = new NetworkVariable<ulong>();
        public NetworkVariable<ulong> RoundBet = new NetworkVariable<ulong>();
        public NetworkVariable<PlayerState> State = new NetworkVariable<PlayerState>();


        /// <summary>
        /// 플레이어가 가진 모든 카드
        /// </summary>
        public List<Card> AllCards
        {
            get
            {
                List<Card> cards = new List<Card>();
                cards.AddRange(Hands);
                cards.AddRange(Board);

                return cards;
            }
        }

        /// <summary>
        /// player가 가지고 있는 카드 중 비공개 카드
        /// </summary>
        public List<Card> Hands = new List<Card>();
        /// <summary>
        /// player가 가지고 있는 카드 중 공개한 카드
        /// </summary>
        public List<Card> Board = new List<Card>();

        public int DrawsCount { get { return draws.Count; } }

        List<Card> draws = new List<Card>();

        public bool IsDraw { get { return isDraw; } }
        bool isDraw;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                clientPlayer = GetComponentInChildren<ClientPlayer>();
            }

            Chips.OnValueChanged += OnChipsChanged;
            Bet.OnValueChanged += OnBetChanged;
            RoundBet.OnValueChanged += OnRoundBetChanged;
            State.OnValueChanged += OnPlayerStateChanged;

            var roots = gameObject.scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var clientGameRoom = root.GetComponent<ClientGameRoomStateBehaviour>();
                if (clientGameRoom != null)
                {
                    if (clientGameRoom.Container.TryResolve<UIGameRoom>(out uiGameRoom))
                    {
                        break;
                    }
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            Chips.OnValueChanged -= OnChipsChanged;
            Bet.OnValueChanged -= OnBetChanged;
            RoundBet.OnValueChanged -= OnRoundBetChanged;
            State.OnValueChanged -= OnPlayerStateChanged;
        }

        public void ApplyBet(ulong bet)
        {
            Chips.Value -= bet;
            Bet.Value += bet;
            RoundBet.Value += bet;
        }

        /// <summary>
        /// 승리로 획득한 Chip 개수를 적용한다.
        /// </summary>
        /// <param name="chips"></param>
        public void ApplyWinChips(ulong chips)
        {
            Chips.Value += chips;
        }

        public void ChangeState(PlayerState state)
        {
            State.Value = state;
        }

        public void BreakGame()
        {
            Chips.Value += Bet.Value;
            ResetGame();
        }

        public void ResetRound()
        {
            RoundBet.Value = 0;
        }

        public void ResetGame()
        {
            Bet.Value = 0;
            RoundBet.Value = 0;
            isDraw = false;
            State.Value = PlayerState.Waiting;

            Hands.Clear();
            Board.Clear();
            draws.Clear();
        }

        public void DrawCards(List<Card> cards)
        {
            for (int i = 0; i < draws.Count; i++)
            {
                var draw = draws[i];
                if (Hands.Contains(draw))
                {
                    Hands.Remove(draw);
                }
            }
            draws.Clear();
            Hands.AddRange(cards);
            isDraw = true;
        }

        /// <summary>
        /// 카드를 받는다.
        /// </summary>
        /// <param name="card"></param>
        public void ReceiveCard(Card card)
        {
            if (card.IsFaceUp) Board.Add(card);
            else Hands.Add(card);
        }

        private void OnRoundBetChanged(ulong previousValue, ulong newValue)
        {
            uiGameRoom?.UpdateActionButtons();
        }

        private void OnBetChanged(ulong previousValue, ulong newValue)
        {
            uiGameRoom?.SetPlayerInfoRpc(OwnerClientId, NickName.Value.ToString(), Chips.Value, newValue);
        }

        private void OnChipsChanged(ulong previousValue, ulong newValue)
        {
            uiGameRoom?.SetPlayerInfoRpc(OwnerClientId, NickName.Value.ToString(), newValue, Bet.Value);
        }

        private void OnPlayerStateChanged(PlayerState previousValue, PlayerState newValue)
        {
            Debug.Log($"ClientId:{OwnerClientId}, PlayerState:[{previousValue}]->[{newValue}]");
            uiGameRoom?.UpdateActionButtons();
        }
    }
}
