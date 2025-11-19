using EnhancedUI.EnhancedScroller;
using PlayingCard.ConnectionManagement;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.PlayModels;
using PlayingCard.LobbyManagement;
using PlayingCard.Utilities.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    public enum LobbyMode
    {
        Unready,
        Ready,
        LobbyEnding,
        FatalError
    }

    public class UILobby : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField]
        Text textLobbyName;
        [SerializeField]
        Text textLobbyUserCount;
        [SerializeField]
        Text textLobbyState;

        [SerializeField]
        Button buttonReady;
        [SerializeField]
        TextMeshProUGUI textReadyButton;

        [SerializeField]
        EnhancedScroller scroller;
        [SerializeField]
        float cellHeight = 150f;

        [SerializeField]
        LobbySeatCellView cellViewPrefab;

        public int SeatsCount => dataList.Count;
        public Action onClickRaady;

        List<LobbySeat> dataList = new List<LobbySeat>();

        private Game game;
        private LobbyManager lobbyManager;
        private ConnectionManager connectionManager;

        [Inject]
        public void Construct(Game SelectedGame,
            LobbyManager lobbyManager,
            ConnectionManager connectionManager)
        {
            game = SelectedGame;
            this.lobbyManager = lobbyManager;
            this.connectionManager = connectionManager;
        }

        private void Start()
        {
            scroller.Delegate = this;
            buttonReady.AddOnClickEvent(OnClickReady);

            textLobbyName.text = $"게임 이름: {game.GameName}";
            UpdateSeatInfo();
            UpdateStateInfo();
        }

        private void OnEnable()
        {
            lobbyManager.IsLobbyClosed.OnValueChanged += OnLobbyClosedValueChanged;
        }

        private void OnDisable()
        {
            lobbyManager.IsLobbyClosed.OnValueChanged -= OnLobbyClosedValueChanged;
        }

        private void OnLobbyClosedValueChanged(bool previousValue, bool newValue)
        {
            UpdateStateInfo();
        }

        public void InitializeSeat()
        {
            dataList.Clear();
            for (int i = 0; i < connectionManager.MaxConnectedPlayers; i++)
            {
                var seat = new LobbySeat(i);
                dataList.Add(seat);
            }
        }

        void UpdateSeatInfo()
        {
            var activeCount = dataList.FindAll(data => data.SeatState != LobbyManager.SeatState.Inactive).Count;
            textLobbyUserCount.text = $"[접속자 수]/[최대 인원]: {activeCount}/{dataList.Count}";
        }

        void UpdateStateInfo()
        {
            if (lobbyManager.IsLobbyClosed.Value)
            {
                textLobbyState.text = "게임 시작";
            }
            else
            {
                textLobbyState.text = "준비 기다리는 중";
            }
        }

        private void OnClickReady()
        {
            onClickRaady?.Invoke();
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            LobbySeatCellView cellView = scroller.GetCellView(cellViewPrefab) as LobbySeatCellView;
            cellView.Set(dataList[cellIndex]);
            return cellView;
        }

        public int GetAvailableSeatIdx()
        {
            var freeSeat = dataList.Find(seat => seat.SeatState == LobbyManager.SeatState.Inactive);
            if (freeSeat != null)
                return dataList.IndexOf(freeSeat);
            else
                return -1;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellHeight;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return dataList.Count;
        }

        internal void ConfigureLobbyMode(LobbyMode mode)
        {
            switch (mode)
            {
                case LobbyMode.Unready:
                    textReadyButton.text = "READY!";
                    break;
                case LobbyMode.Ready:
                    textReadyButton.text = "UNREADY";
                    break;
                case LobbyMode.LobbyEnding:
                    break;
                case LobbyMode.FatalError:
                    break;
                default:
                    break;
            }
        }

        internal void UpdateSeats(LobbyManager.LobbyPlayerState[] curSeats)
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                var data = dataList[i];
                data.SetInfo(curSeats[i].SeatState, curSeats[i].PlayerNumber, curSeats[i].UserName);
            }
            scroller.ReloadData();

            UpdateSeatInfo();
        }
    }
}
