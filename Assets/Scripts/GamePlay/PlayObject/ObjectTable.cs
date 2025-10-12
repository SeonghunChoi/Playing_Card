using MessagePipe;
using PlayingCard.GamePlay.Message;
using PlayingCard.GamePlay.PlayModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace PlayingCard.GamePlay.PlayObject
{
    /// <summary>
    /// Game Room 의 GameObject 들을 관리
    /// </summary>
    public class ObjectTable : MonoBehaviour
    {
        const float CardSpace = 1.8f;

        [SerializeField]
        Transform trComunityPoint;

        [SerializeField]
        List<ObjectPlayer> objectPlayers;

        private IObjectResolver resolver;
        private Dictionary<string, GameObject> cardDict;
        private IDisposable dealCardDisposable;

        [Inject]
        public void Set(IObjectResolver resolver, Dictionary<string, GameObject> cardDict, ISubscriber<DealCardMessage> dealCardSubscriber)
        {
            this.resolver = resolver;
            this.cardDict = cardDict;
            dealCardDisposable = dealCardSubscriber.Subscribe(DealCard);
        }

        private void DealCard(DealCardMessage message)
        {
            var cards = message.dealCards;
            // 카드 받을 플레이어가 없다면, 테이블에 놓는다.
            if (message.player == null)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = CreateCard(GetCardName(cards[i]));
                    card.transform.SetParent(trComunityPoint, false);
                    card.transform.localScale = Vector3.one;
                }
                SetCardPosition(trComunityPoint);
            }
            else
            {
                // 플레이어 아이디로 찾는다.
                var objectPlayer = objectPlayers.Find(op => op.Id == message.player.Id);
                for (int i = 0; i < cards.Count; i++)
                {
                    var card = CreateCard(GetCardName(cards[i]));
                    objectPlayer.AddCard(card, cards[i].IsFaceUp);
                }
                objectPlayer.SetCardPosition();
            }
        }

        /// <summary>
        /// 카드 Suit 와 Rank 로 카드 Prefab 이름을 가져온다.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public string GetCardName(Card card)
        {
            if (card.IsWild)
            {
                return "Deck03_Joker";
            }
            else
            {
                string suitName = null;
                switch (card.Suit)
                {
                    case Suit.Spades:
                        suitName = "Spade";
                        break;
                    case Suit.Hearts:
                        suitName = "Heart";
                        break;
                    case Suit.Diamonds:
                        suitName = "Diamond";
                        break;
                    case Suit.Clubs:
                        suitName = "Club";
                        break;
                }
                string rankName = null;
                if (card.Rank < Rank.Jack)
                {
                    rankName = ((int)card.Rank).ToString();
                }
                else
                {
                    switch (card.Rank)
                    {
                        case Rank.Jack:
                            rankName = "J";
                            break;
                        case Rank.Queen:
                            rankName = "Q";
                            break;
                        case Rank.King:
                            rankName = "K";
                            break;
                        case Rank.Ace:
                            rankName = "A";
                            break;
                        default:
                            break;
                    }
                }
                return $"Deck03_{suitName}_{rankName}";
            }
        }

        /// <summary>
        /// prefab 이름으로 카드 생성
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GameObject CreateCard(string prefabName)
        {
            if (cardDict.TryGetValue(prefabName, out var prefab))
            {
                var obj = resolver.Instantiate(prefab);
                return obj;
            }
            else
                throw new Exception($"{prefabName} prefab not found!");
        }

        /// <summary>
        /// 카드들 위치 지정한다.
        /// </summary>
        /// <param name="container"></param>
        public static void SetCardPosition(Transform container)
        {
            int cardCount = container.childCount;
            float startX = 0;
            if (cardCount % 2 == 0)
                startX = (cardCount / 2) * -CardSpace + (0.5f * CardSpace);
            else
                startX = (cardCount / 2) * -CardSpace;

            for (int i = 0; i < cardCount; i++)
            {
                var card = container.GetChild(i);
                card.localPosition = new Vector3(startX + i * CardSpace, 0, 0);
            }
        }

        private void OnDestroy()
        {
            dealCardDisposable?.Dispose();
        }
    }
}
