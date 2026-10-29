using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    public List<CardData> allCardData;
    private List<CardData> drawDeck = new List<CardData>();
    // Track cards that were played/discarded (removed from deck permanently)
    private Dictionary<CardData.Suit, int> playedDiscardedSuitCounts = new Dictionary<CardData.Suit, int>();
    public GameObject cardPrefab;
    public Transform handSpawnPoint;
    private HandManager handManager;
    private UIManager uiManager;
    private ICardLayoutCalculator layoutCalculator;

    public int RemainingCardCount
    {
        get { return drawDeck != null ? drawDeck.Count : 0; }
    }

    public int TotalCardCount
    {
        get { return allCardData != null ? allCardData.Count : 0; }
    }

    void Awake()
    {
        handManager = FindFirstObjectByType<HandManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        layoutCalculator = new CardLayoutCalculator();
    }

    void Start()
    {
        if (allCardData == null || allCardData.Count == 0)
        {
            return;
        }

        // The initial hand is drawn by HandManager.StartLevel when the run starts.
        InitializeDeck();
        ShuffleDrawDeck();
    }

    public void InitializeDeck()
    {
        drawDeck.Clear();
        // Reset played/discarded suit counts when reinitializing deck
        playedDiscardedSuitCounts = new Dictionary<CardData.Suit, int>
        {
            { CardData.Suit.Glitch, 0 },
            { CardData.Suit.Core, 0 },
            { CardData.Suit.Neural, 0 },
            { CardData.Suit.Protocol, 0 }
        };
        
        if (allCardData != null)
        {
            foreach (CardData cardData in allCardData)
            {
                if (cardData != null && !cardData.IsJoker)
                {
                    drawDeck.Add(cardData);
                }
            }
        }
        
        NotifyDeckCountChanged();
    }

    public void ShuffleDrawDeck()
    {
        if (drawDeck == null || drawDeck.Count == 0)
        {
            return;
        }
        
        System.Random rng = new System.Random();
        drawDeck = drawDeck.OrderBy(a => rng.Next()).ToList();
    }

    public void DrawInitialHand(int count)
    {
        Quaternion baseRotation = cardPrefab.transform.rotation;
        List<CardObject> drawnCards = new List<CardObject>();
        
        for (int i = 0; i < count; i++)
        {
            CardLayoutInfo layoutInfo = layoutCalculator.CalculateCardLayout(i, count, handSpawnPoint.position, baseRotation);
            CardLayoutAngles angles = ((CardLayoutCalculator)layoutCalculator).CalculateCardAngles(i, count);
            
            CardObject card = DrawCard(layoutInfo.position, angles.rotationAngle, angles.rollAngle, layoutInfo.sortingOrder);
            if (card != null)
            {
                drawnCards.Add(card);
            }
        }
        
        for (int i = 0; i < drawnCards.Count; i++)
        {
            if (drawnCards[i] != null && drawnCards[i].transform != null)
            {
                Transform cardTransform = drawnCards[i].transform;
                if (cardTransform.parent != null)
                {
                    cardTransform.SetSiblingIndex(i);
                }
            }
        }
    }

    public CardObject DrawCard(Vector3 spawnPosition, float rotationAngle = 0f, float rollAngle = 0f, int sortingOrder = 0)
    {
        Quaternion baseRotation = cardPrefab.transform.rotation;
        Quaternion fanRotation = Quaternion.Euler(0, rotationAngle, 0);
        Quaternion rollRotation = Quaternion.Euler(0, 0, rollAngle);
        Quaternion spawnRotation = baseRotation * fanRotation * rollRotation;

        return DrawCardInternal(spawnPosition, spawnRotation, sortingOrder, updateOriginalPosition: true);
    }

    public CardObject DrawCardAtPosition(Vector3 spawnPosition, Quaternion spawnRotation, int sortingOrder)
    {
        return DrawCardInternal(spawnPosition, spawnRotation, sortingOrder, updateOriginalPosition: true);
    }

    private CardObject DrawCardInternal(Vector3 spawnPosition, Quaternion spawnRotation, int sortingOrder, bool updateOriginalPosition)
    {
        if (drawDeck == null || drawDeck.Count == 0)
        {
            return null;
        }

        CardData drawnCardData = drawDeck[0];
        drawDeck.RemoveAt(0);
        NotifyDeckCountChanged();

        GameObject cardGO = Instantiate(cardPrefab, spawnPosition, spawnRotation);
        cardGO.name = $"{drawnCardData.cardRank}_of_{drawnCardData.cardSuit}";

        CardObject cardObject = cardGO.GetComponent<CardObject>();

        if (cardObject != null)
        {
            cardObject.Initialize(drawnCardData, handManager);
            SetCardSortingOrder(cardObject, sortingOrder);

            if (updateOriginalPosition)
            {
                cardObject.UpdateOriginalPosition(spawnPosition);
            }

            handManager.AddCardToHand(cardObject);
            
            if (handManager != null && handManager.currentHand != null)
            {
                int handIndex = handManager.currentHand.IndexOf(cardObject);
                if (handIndex >= 0 && cardObject.transform != null)
                {
                    Transform cardTransform = cardObject.transform;
                    if (cardTransform.parent != null)
                    {
                        cardTransform.SetSiblingIndex(handIndex);
                    }
                }
            }
            
            return cardObject;
        }

        return null;
    }

    private void SetCardSortingOrder(CardObject cardObject, int sortingOrder)
    {
        if (cardObject == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = cardObject.SpriteRenderer;
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void EnsureDeckHasCards()
    {
        if (drawDeck.Count > 0)
            return;

        NotifyStatus("Deste bitti, yeniden karistirildi.");

        if (allCardData == null || allCardData.Count == 0)
        {
            return;
        }

        InitializeDeck();
        ShuffleDrawDeck();
    }

    private void NotifyStatus(string message)
    {
        if (uiManager != null)
        {
            uiManager.SetStatus(message);
        }
    }

    private void NotifyDeckCountChanged()
    {
        if (uiManager != null)
        {
            uiManager.UpdateDeckCount(RemainingCardCount, TotalCardCount);
        }
    }

    public void RearrangeHand()
    {
        if (handManager == null || handManager.currentHand == null || handManager.currentHand.Count == 0)
        {
            return;
        }

        List<CardObject> cards = handManager.currentHand;
        int count = cards.Count;

        if (count == 0)
        {
            return;
        }

        Quaternion baseRotation = cardPrefab.transform.rotation;

        for (int i = 0; i < count; i++)
        {
            CardObject card = cards[i];
            if (card == null) continue;

            CardLayoutInfo layoutInfo = layoutCalculator.CalculateCardLayout(i, count, handSpawnPoint.position, baseRotation);

            card.transform.rotation = layoutInfo.rotation;

            SpriteRenderer spriteRenderer = card.SpriteRenderer;
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = layoutInfo.sortingOrder;
            }

            card.UpdateOriginalPosition(layoutInfo.position);
        }
    }

    public Dictionary<CardData.Suit, int> GetSuitCounts()
    {
        Dictionary<CardData.Suit, int> suitCounts = new Dictionary<CardData.Suit, int>
        {
            { CardData.Suit.Glitch, 0 },
            { CardData.Suit.Core, 0 },
            { CardData.Suit.Neural, 0 },
            { CardData.Suit.Protocol, 0 }
        };

        // Count all cards in allCardData (excluding jokers)
        if (allCardData != null)
        {
            foreach (CardData cardData in allCardData)
            {
                if (cardData == null || cardData.IsJoker)
                {
                    continue;
                }

                CardData.Suit suit = cardData.cardSuit;
                if (suitCounts.ContainsKey(suit))
                {
                    suitCounts[suit]++;
                }
            }
        }

        // Subtract cards that are currently in hand
        if (handManager != null && handManager.currentHand != null)
        {
            foreach (CardObject cardObject in handManager.currentHand)
            {
                if (cardObject == null || cardObject.cardInfo == null || cardObject.cardInfo.IsJoker)
                {
                    continue;
                }

                CardData.Suit suit = cardObject.cardInfo.cardSuit;
                if (suitCounts.ContainsKey(suit) && suitCounts[suit] > 0)
                {
                    suitCounts[suit]--;
                }
            }
        }

        // Subtract cards that were played/discarded
        foreach (var kvp in playedDiscardedSuitCounts)
        {
            if (suitCounts.ContainsKey(kvp.Key) && suitCounts[kvp.Key] >= kvp.Value)
            {
                suitCounts[kvp.Key] -= kvp.Value;
            }
        }
        return suitCounts;
    }

    // Method to register played/discarded cards
    public void RegisterPlayedDiscardedCards(List<CardObject> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            return;
        }

        foreach (CardObject card in cards)
        {
            if (card == null || card.cardInfo == null || card.cardInfo.IsJoker)
            {
                continue;
            }

            CardData.Suit suit = card.cardInfo.cardSuit;
            
            if (!playedDiscardedSuitCounts.ContainsKey(suit))
            {
                playedDiscardedSuitCounts[suit] = 0;
            }
            playedDiscardedSuitCounts[suit]++;
        }
    }

}
