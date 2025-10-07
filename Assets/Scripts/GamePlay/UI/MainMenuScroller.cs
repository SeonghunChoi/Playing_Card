using EnhancedUI.EnhancedScroller;
using PlayingCard.GamePlay.Configuration;
using PlayingCard.GamePlay.PlayModels;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace PlayingCard.GamePlay.UI
{
    public class MainMenuScroller : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] 
        EnhancedScroller scroller;
        [SerializeField]
        float cellHeight = 120f;

        [SerializeField] 
        MainMenuCellView cellViewPrefab;

        GameManager gameManager;

        List<Game> dataList;

        private void Start()
        {
            scroller.Delegate = this;
        }

        [Inject]
        public void Set(GameManager gameManager)
        {
            this.gameManager = gameManager;

            dataList = gameManager.GameList;

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
            gameManager.SetGame(game);
        }
    }
}
