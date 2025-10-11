using EnhancedUI.EnhancedScroller;
using MessagePipe;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.Message;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.UI
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

        private IPublisher<SelectGameMessage> selectGamePublisher;

        private void Start()
        {
            scroller.Delegate = this;
        }

        [Inject]
        public void Set(
            List<Game> games, 
            IPublisher<SelectGameMessage> selectGamePublisher)
        {
            dataList = games;
            this.selectGamePublisher = selectGamePublisher;

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
            selectGamePublisher.Publish(new SelectGameMessage(game));
        }
    }
}
