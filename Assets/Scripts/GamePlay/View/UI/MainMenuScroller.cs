using EnhancedUI.EnhancedScroller;
using MessagePipe;
using PlayingCard.GamePlay.Model.Configuration;
using PlayingCard.GamePlay.Model.Message;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.View.UI
{
    /// <summary>
    /// Main Menu 용 Scrollview
    /// </summary>
    public class MainMenuScroller : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] 
        EnhancedScroller scroller;
        [SerializeField]
        float cellHeight = 120f;

        [SerializeField] 
        MainMenuCellView cellViewPrefab;

        List<Game> dataList;

        private IPublisher<MainMenuMessage> mainMenuMessagePublisher;

        private void Start()
        {
            scroller.Delegate = this;
        }

        [Inject]
        public void Set(
            List<Game> games, 
            IPublisher<MainMenuMessage> mainMenuMessagePublisher)
        {
            dataList = games;
            this.mainMenuMessagePublisher = mainMenuMessagePublisher;

            scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return dataList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return cellHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            MainMenuCellView cellView = scroller.GetCellView(cellViewPrefab) as MainMenuCellView;

            cellView.Set(dataList[dataIndex], OnClickCell);

            return cellView;
        }

        void OnClickCell(Game game)
        {
            int idx = dataList.IndexOf(game);
            MainMenuMessage message = new MainMenuMessage(MainMenuMessageType.Menu, idx);
            mainMenuMessagePublisher.Publish(message);
        }
    }
}
