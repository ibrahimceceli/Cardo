using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public List<CardObject> currentHand = new List<CardObject>();
    public int totalChips = 0;
    public float totalMultiplier = 1f;
    private int totalScore = 0;
    private const int MAX_DISCARDS = 4;
    private const int BASE_HAND_SIZE = 8;
    private const int MAX_JOKERS = 5;
    private const float LEVEL_END_DELAY = 1f;
    // Index 0 is level 1. The number of entries defines the number of levels.
    private static readonly int[] LEVEL_SCORE_THRESHOLDS = { 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100 };
    private static readonly int[] LEVEL_MAX_HANDS = { 5, 5, 4, 4, 3, 3, 3, 2, 2, 2 };
    private int remainingDiscards = MAX_DISCARDS;
    private int currentLevel = 1;
    private int remainingHands = LEVEL_MAX_HANDS[0];
    private bool isLevelComplete = false;
    private bool isGameOver = false;
    private List<CardData> selectedJokers = new List<CardData>();

    public DeckManager deckManager;
    private UIManager uiManager;

    private PokerHandEvaluator handEvaluator;
    private HandScoringService scoringService;
    private Camera mainCamera;
    private PointerEventData uiPointerEventData;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    private struct HandScore
    {
        public PokerHandType handType;
        public int chips;
        public float multiplier;
        public int total;
    }

    void Awake()
    {
        if (deckManager == null)
        {
            deckManager = FindFirstObjectByType<DeckManager>();
        }

        uiManager = FindFirstObjectByType<UIManager>();

        handEvaluator = new PokerHandEvaluator();
        scoringService = new HandScoringService();

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isLevelComplete || isGameOver)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            HandleCardClick();
        }
    }

    // Only UI graphics count here; the camera's PhysicsRaycaster also reports card colliders.
    private bool IsPointerOverUI()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        if (uiPointerEventData == null)
        {
            uiPointerEventData = new PointerEventData(eventSystem);
        }

        uiPointerEventData.position = Input.mousePosition;
        uiRaycastResults.Clear();
        eventSystem.RaycastAll(uiPointerEventData, uiRaycastResults);

        foreach (RaycastResult result in uiRaycastResults)
        {
            if (result.module is GraphicRaycaster)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleCardClick()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }
        
        if (currentHand == null || currentHand.Count == 0)
        {
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] allHits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        System.Collections.Generic.List<System.Tuple<CardObject, RaycastHit>> hitCardsWithInfo = 
            new System.Collections.Generic.List<System.Tuple<CardObject, RaycastHit>>();
        
        foreach (RaycastHit hit in allHits)
        {
            CardObject card = hit.collider.GetComponent<CardObject>();
            if (card == null || !currentHand.Contains(card))
            {
                continue;
            }
            
            hitCardsWithInfo.Add(new System.Tuple<CardObject, RaycastHit>(card, hit));
        }
        
        if (hitCardsWithInfo.Count == 0)
        {
            return;
        }
        
        CardObject topmostCard = GetTopmostCard(hitCardsWithInfo);
        
        if (topmostCard != null)
        {
            topmostCard.DetectClick();
        }
    }
    
    private CardObject GetTopmostCard(System.Collections.Generic.List<System.Tuple<CardObject, RaycastHit>> hitCardsWithInfo)
    {
        if (hitCardsWithInfo == null || hitCardsWithInfo.Count == 0)
        {
            return null;
        }
        
        if (hitCardsWithInfo.Count == 1)
        {
            return hitCardsWithInfo[0].Item1;
        }
        
        int highestSortingOrder = int.MinValue;
        CardObject topmostCard = null;
        System.Collections.Generic.List<CardObject> cardsWithHighestOrder = 
            new System.Collections.Generic.List<CardObject>();
        
        foreach (var hitInfo in hitCardsWithInfo)
        {
            CardObject card = hitInfo.Item1;
            
            if (card == null || card.SpriteRenderer == null)
            {
                continue;
            }
            
            int sortingOrder = card.SpriteRenderer.sortingOrder;
            
            if (sortingOrder > highestSortingOrder)
            {
                highestSortingOrder = sortingOrder;
                cardsWithHighestOrder.Clear();
                cardsWithHighestOrder.Add(card);
                topmostCard = card;
            }
            else if (sortingOrder == highestSortingOrder)
            {
                cardsWithHighestOrder.Add(card);
            }
        }
        
        if (cardsWithHighestOrder.Count == 1)
        {
            return topmostCard;
        }
        
        hitCardsWithInfo.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));
        
        foreach (var hitInfo in hitCardsWithInfo)
        {
            CardObject card = hitInfo.Item1;
            
            if (cardsWithHighestOrder.Contains(card))
            {
                return card;
            }
        }
        
        return topmostCard;
    }


    public void AddCardToHand(CardObject newCard)
    {
        currentHand.Add(newCard);
    }

    public void UpdateSelectedHandType()
    {
        if (uiManager == null)
        {
            return;
        }

        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0 || selectedCards.Count > 5)
        {
            uiManager.HideHandType();
            uiManager.UpdateScore(0, 0);
            return;
        }

        HandScore score = CalculateHandScore(selectedCards);

        if (score.handType != PokerHandType.None)
        {
            uiManager.ShowHandType(score.handType);
            uiManager.UpdateScore(score.chips, score.multiplier);
        }
        else
        {
            uiManager.HideHandType();
            uiManager.UpdateScore(0, 0);
        }
    }

    // Single source of truth for scoring, shared by the selection preview and PlayHand.
    private HandScore CalculateHandScore(List<CardObject> selectedCards)
    {
        HandScore score = new HandScore();
        score.handType = handEvaluator.DetermineHand(selectedCards);

        List<CardObject> scoringCards = GetCardsThatFormHand(score.handType, selectedCards);

        int cardChipTotal = 0;
        foreach (CardObject card in scoringCards)
        {
            if (card != null && card.cardInfo != null)
            {
                cardChipTotal += card.cardInfo.NumericValue;
            }
        }

        score.chips = scoringService.GetBaseScore(score.handType)
            + cardChipTotal
            + GetTotalJokerBonusScore()
            + GetSuitBasedJokerBonusScore(scoringCards);

        score.multiplier = scoringService.GetMultiplier(score.handType)
            + GetTotalJokerMultiplierBonus()
            + GetHandTypeBasedJokerMultiplierBonus(score.handType);

        score.total = Mathf.RoundToInt(score.chips * score.multiplier);
        return score;
    }

    private List<CardObject> GetSelectedCards()
    {
        return currentHand.Where(c => c.isSelected).ToList();
    }

    private List<CardObject> GetCardsThatFormHand(PokerHandType handType, List<CardObject> selectedCards)
    {
        if (selectedCards == null || selectedCards.Count == 0)
        {
            return new List<CardObject>();
        }

        switch (handType)
        {
            case PokerHandType.HighCard:
                return GetHighestCard(selectedCards);
            
            case PokerHandType.Pair:
                return GetPairCards(selectedCards);
            
            case PokerHandType.TwoPair:
                return GetTwoPairCards(selectedCards);
            
            case PokerHandType.ThreeOfAKind:
                return GetThreeOfAKindCards(selectedCards);
            
            case PokerHandType.FourOfAKind:
                return GetFourOfAKindCards(selectedCards);
            
            case PokerHandType.Straight:
            case PokerHandType.Flush:
            case PokerHandType.FullHouse:
            case PokerHandType.StraightFlush:
            case PokerHandType.RoyalFlush:
            case PokerHandType.FiveOfAKind:
            case PokerHandType.FlushHouse:
            case PokerHandType.FlushFive:
                return selectedCards;
            
            case PokerHandType.None:
            default:
                return new List<CardObject>();
        }
    }

    private List<CardObject> GetHighestCard(List<CardObject> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            return new List<CardObject>();
        }

        CardObject highestCard = cards[0];
        foreach (CardObject card in cards)
        {
            if (card != null && card.cardInfo != null && 
                highestCard != null && highestCard.cardInfo != null)
            {
                if (card.cardInfo.NumericValue > highestCard.cardInfo.NumericValue)
                {
                    highestCard = card;
                }
            }
        }

        return new List<CardObject> { highestCard };
    }

    private List<CardObject> GetPairCards(List<CardObject> cards)
    {
        if (cards == null || cards.Count < 2)
        {
            return new List<CardObject>();
        }

        Dictionary<int, List<CardObject>> rankGroups = new Dictionary<int, List<CardObject>>();
        
        foreach (CardObject card in cards)
        {
            if (card == null || card.cardInfo == null)
            {
                continue;
            }

            int rank = card.cardInfo.NumericValue;
            if (!rankGroups.ContainsKey(rank))
            {
                rankGroups[rank] = new List<CardObject>();
            }
            rankGroups[rank].Add(card);
        }

        foreach (var group in rankGroups.Values)
        {
            if (group.Count >= 2)
            {
                return group;
            }
        }

        return new List<CardObject>();
    }

    private List<CardObject> GetTwoPairCards(List<CardObject> cards)
    {
        if (cards == null || cards.Count < 4)
        {
            return new List<CardObject>();
        }

        Dictionary<int, List<CardObject>> rankGroups = new Dictionary<int, List<CardObject>>();
        
        foreach (CardObject card in cards)
        {
            if (card == null || card.cardInfo == null)
            {
                continue;
            }

            int rank = card.cardInfo.NumericValue;
            if (!rankGroups.ContainsKey(rank))
            {
                rankGroups[rank] = new List<CardObject>();
            }
            rankGroups[rank].Add(card);
        }

        List<CardObject> result = new List<CardObject>();
        int pairCount = 0;

        var sortedGroups = rankGroups.Values.Where(g => g != null && g.Count > 0 && g[0] != null && g[0].cardInfo != null)
            .OrderByDescending(g => g[0].cardInfo.NumericValue);

        foreach (var group in sortedGroups)
        {
            if (group.Count >= 2 && pairCount < 2)
            {
                result.AddRange(group);
                pairCount++;
            }
        }

        return result;
    }

    private List<CardObject> GetThreeOfAKindCards(List<CardObject> cards)
    {
        if (cards == null || cards.Count < 3)
        {
            return new List<CardObject>();
        }

        Dictionary<int, List<CardObject>> rankGroups = new Dictionary<int, List<CardObject>>();
        
        foreach (CardObject card in cards)
        {
            if (card == null || card.cardInfo == null)
            {
                continue;
            }

            int rank = card.cardInfo.NumericValue;
            if (!rankGroups.ContainsKey(rank))
            {
                rankGroups[rank] = new List<CardObject>();
            }
            rankGroups[rank].Add(card);
        }

        foreach (var group in rankGroups.Values)
        {
            if (group.Count >= 3)
            {
                return group;
            }
        }

        return new List<CardObject>();
    }

    private List<CardObject> GetFourOfAKindCards(List<CardObject> cards)
    {
        if (cards == null || cards.Count < 4)
        {
            return new List<CardObject>();
        }

        Dictionary<int, List<CardObject>> rankGroups = new Dictionary<int, List<CardObject>>();
        
        foreach (CardObject card in cards)
        {
            if (card == null || card.cardInfo == null)
            {
                continue;
            }

            int rank = card.cardInfo.NumericValue;
            if (!rankGroups.ContainsKey(rank))
            {
                rankGroups[rank] = new List<CardObject>();
            }
            rankGroups[rank].Add(card);
        }

        foreach (var group in rankGroups.Values)
        {
            if (group.Count >= 4)
            {
                return group;
            }
        }

        return new List<CardObject>();
    }

    public bool CanSelectCard(CardObject card)
    {
        if (card != null && card.isSelected)
        {
            return true;
        }

        int selectedCount = GetSelectedCards().Count;
        return selectedCount < 5;
    }

    private void RemoveCardsFromHand(List<CardObject> cards)
    {
        foreach (CardObject card in cards)
        {
            currentHand.Remove(card);
            Destroy(card.gameObject);
        }
    }

    // Draws as many replacements as the deck allows, so a short deck no longer means zero cards.
    private void DrawReplacementCards(CardTransformData transformData)
    {
        if (deckManager == null)
        {
            return;
        }

        for (int i = 0; i < transformData.positions.Count; i++)
        {
            CardObject newCard = deckManager.DrawCardAtPosition(transformData.positions[i], transformData.rotations[i], transformData.sortingOrders[i]);
            if (newCard == null)
            {
                break;
            }
        }
    }

    public bool PlayHand()
    {
        if (isLevelComplete || isGameOver || remainingHands <= 0)
        {
            return false;
        }

        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0 || selectedCards.Count > 5)
        {
            UpdateDiscardUI();
            return false;
        }

        HandScore score = CalculateHandScore(selectedCards);

        totalScore += score.total;
        totalChips += score.total;
        totalMultiplier = score.multiplier;

        remainingHands--;
        UpdateHandCountUI();

        if (uiManager != null)
        {
            uiManager.UpdateScore(totalScore);
        }

        CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);

        // Register played cards in DeckManager BEFORE destroying them
        if (deckManager != null)
        {
            deckManager.RegisterPlayedDiscardedCards(selectedCards);
        }

        RemoveCardsFromHand(selectedCards);

        if (totalScore >= GetScoreThresholdForLevel(currentLevel))
        {
            CompleteLevel();
        }
        else if (remainingHands <= 0)
        {
            EndRun(won: false);
        }
        else
        {
            DrawReplacementCards(transformData);
        }

        // Keep the played hand's breakdown on screen until the player selects new cards.
        if (uiManager != null)
        {
            uiManager.ShowHandType(score.handType);
            uiManager.UpdateScore(score.chips, score.multiplier);
            uiManager.UpdateDeckInfo();
        }

        return true;
    }

    public void DiscardSelectedAndDraw()
    {
        if (isLevelComplete || isGameOver)
        {
            return;
        }

        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0 || selectedCards.Count > 5 || remainingDiscards <= 0)
        {
            UpdateDiscardUI();
            return;
        }

        remainingDiscards--;

        CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);

        // Register discarded cards in DeckManager
        if (deckManager != null)
        {
            deckManager.RegisterPlayedDiscardedCards(selectedCards);
        }

        RemoveCardsFromHand(selectedCards);
        DrawReplacementCards(transformData);

        UpdateSelectedHandType();
        UpdateDiscardUI();

        if (uiManager != null)
        {
            uiManager.UpdateDeckInfo();
        }
    }

    private void CompleteLevel()
    {
        isLevelComplete = true;

        if (currentLevel >= MaxLevel)
        {
            EndRun(won: true);
            return;
        }

        StartCoroutine(ShowJokerSelectionDelayed());
    }

    private void EndRun(bool won)
    {
        isGameOver = true;
        StartCoroutine(ShowGameOverDelayed(won));
    }

    private System.Collections.IEnumerator ShowGameOverDelayed(bool won)
    {
        // Cards are world objects drawn in front of the canvas, so they must be hidden for the overlay.
        HideHandCards();
        yield return new WaitForSeconds(LEVEL_END_DELAY);

        if (uiManager != null)
        {
            uiManager.ShowGameOver(won, totalScore, currentLevel);
        }
    }

    public int GetRemainingDiscards()
    {
        return remainingDiscards;
    }

    public int MaxDiscards
    {
        get { return MAX_DISCARDS; }
    }

    public int MaxJokers
    {
        get { return MAX_JOKERS; }
    }

    public int MaxLevel
    {
        get { return LEVEL_SCORE_THRESHOLDS.Length; }
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
    }

    public int TotalScore
    {
        get { return totalScore; }
    }

    public int GetCurrentScoreThreshold()
    {
        return GetScoreThresholdForLevel(currentLevel);
    }

    public void AddJoker(CardData joker)
    {
        if (joker != null && joker.IsJoker)
        {
            if (selectedJokers == null)
            {
                selectedJokers = new List<CardData>();
            }
            
            if (selectedJokers.Count < MAX_JOKERS)
            {
                selectedJokers.Add(joker);
            }
        }
    }

    public bool CanAddJoker()
    {
        return selectedJokers == null || selectedJokers.Count < MAX_JOKERS;
    }

    public int GetJokerCount()
    {
        return selectedJokers != null ? selectedJokers.Count : 0;
    }

    public List<CardData> GetSelectedJokers()
    {
        return selectedJokers != null ? new List<CardData>(selectedJokers) : new List<CardData>();
    }

    public bool RemoveJoker(CardData jokerToRemove)
    {
        if (selectedJokers == null || jokerToRemove == null)
        {
            return false;
        }

        return selectedJokers.Remove(jokerToRemove);
    }

    public bool ReplaceJoker(CardData oldJoker, CardData newJoker)
    {
        if (selectedJokers == null || oldJoker == null || newJoker == null || !newJoker.IsJoker)
        {
            return false;
        }

        int index = selectedJokers.IndexOf(oldJoker);
        if (index >= 0)
        {
            selectedJokers[index] = newJoker;
            return true;
        }

        return false;
    }

    public int GetTotalHandSizeBonus()
    {
        if (selectedJokers == null || selectedJokers.Count == 0)
        {
            return 0;
        }

        int totalBonus = 0;
        foreach (CardData joker in selectedJokers)
        {
            if (joker != null && joker.IsJoker)
            {
                totalBonus += joker.JokerHandSizeBonus;
            }
        }

        return totalBonus;
    }

    private int GetTotalJokerBonusScore()
    {
        if (selectedJokers == null || selectedJokers.Count == 0)
        {
            return 0;
        }

        int totalBonus = 0;
        foreach (CardData joker in selectedJokers)
        {
            if (joker != null && joker.IsJoker)
            {
                totalBonus += joker.JokerBonusScore;
            }
        }

        return totalBonus;
    }

    private int GetSuitBasedJokerBonusScore(List<CardObject> selectedCards)
    {
        if (selectedJokers == null || selectedJokers.Count == 0 || selectedCards == null)
        {
            return 0;
        }

        int totalBonus = 0;

        foreach (CardData joker in selectedJokers)
        {
            if (joker == null || !joker.IsJoker)
            {
                continue;
            }

            string description = joker.JokerDescription.ToLower();
            string name = joker.name.ToLower();
            CardData.Suit targetSuit = CardData.Suit.Glitch;
            bool isSuitBasedJoker = false;

            if (name.Contains("bit") || description.Contains("bit-miner") || description.Contains("clubs") || description.Contains("glitch"))
            {
                targetSuit = CardData.Suit.Glitch;
                isSuitBasedJoker = true;
            }
            else if (name.Contains("core") || description.Contains("core-extractor") || description.Contains("diamonds"))
            {
                targetSuit = CardData.Suit.Core;
                isSuitBasedJoker = true;
            }
            else if (name.Contains("logic") || description.Contains("logic-harvester") || description.Contains("hearts") || description.Contains("neural"))
            {
                targetSuit = CardData.Suit.Neural;
                isSuitBasedJoker = true;
            }
            else if (name.Contains("void") || description.Contains("void-siphon") || description.Contains("spades") || description.Contains("protocol"))
            {
                targetSuit = CardData.Suit.Protocol;
                isSuitBasedJoker = true;
            }

            if (isSuitBasedJoker)
            {
                int suitCardCount = 0;
                foreach (CardObject card in selectedCards)
                {
                    if (card != null && card.cardInfo != null && card.cardInfo.cardSuit == targetSuit)
                    {
                        suitCardCount++;
                    }
                }

                totalBonus += suitCardCount * 20;
            }
        }

        return totalBonus;
    }

    private float GetTotalJokerMultiplierBonus()
    {
        if (selectedJokers == null || selectedJokers.Count == 0)
        {
            return 0f;
        }

        float totalBonus = 0f;
        foreach (CardData joker in selectedJokers)
        {
            if (joker != null && joker.IsJoker)
            {
                totalBonus += (joker.JokerMultiplier - 1);
            }
        }

        return totalBonus;
    }

    private float GetHandTypeBasedJokerMultiplierBonus(PokerHandType handType)
    {
        if (selectedJokers == null || selectedJokers.Count == 0)
        {
            return 0f;
        }

        float totalBonus = 0f;

        foreach (CardData joker in selectedJokers)
        {
            if (joker == null || !joker.IsJoker)
            {
                continue;
            }

            string description = joker.JokerDescription.ToLower();
            string name = joker.name.ToLower();
            float multiplierBonus = 0f;
            bool isHandTypeBasedJoker = false;

            if ((name.Contains("twin") || description.Contains("twin-thread") || (description.Contains("pair") && description.Contains("0.1"))) && handType == PokerHandType.Pair)
            {
                multiplierBonus = 0.1f;
                isHandTypeBasedJoker = true;
            }
            else if ((name.Contains("dual") || description.Contains("dual-layer") || (description.Contains("two pair") && description.Contains("0.2"))) && handType == PokerHandType.TwoPair)
            {
                multiplierBonus = 0.2f;
                isHandTypeBasedJoker = true;
            }
            else if ((name.Contains("subnet") || description.Contains("subnet overflow") || (description.Contains("flush") && description.Contains("0.4"))) && handType == PokerHandType.Flush)
            {
                multiplierBonus = 0.4f;
                isHandTypeBasedJoker = true;
            }
            else if ((name.Contains("mainframe") || description.Contains("mainframe override") || (description.Contains("full house") && description.Contains("0.6"))) && handType == PokerHandType.FullHouse)
            {
                multiplierBonus = 0.6f;
                isHandTypeBasedJoker = true;
            }

            if (isHandTypeBasedJoker)
            {
                totalBonus += multiplierBonus;
            }
        }

        return totalBonus;
    }

    private void HideAllCards()
    {
        if (currentHand != null)
        {
            foreach (CardObject card in currentHand)
            {
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }

            currentHand.Clear();
        }

        CardObject[] allCardsInScene = FindObjectsByType<CardObject>(FindObjectsSortMode.None);
        if (allCardsInScene != null)
        {
            foreach (CardObject card in allCardsInScene)
            {
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }
        }
    }

    private int GetMaxHandsForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, LEVEL_MAX_HANDS.Length - 1);
        return LEVEL_MAX_HANDS[index];
    }

    private int GetScoreThresholdForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, LEVEL_SCORE_THRESHOLDS.Length - 1);
        return LEVEL_SCORE_THRESHOLDS[index];
    }

    // Starts a fresh run from level 1 with no jokers. Used by the start screen and the restart button.
    public void StartNewRun()
    {
        selectedJokers.Clear();
        StartLevel(1);

        if (uiManager != null)
        {
            uiManager.UpdateJokerCount(GetJokerCount());
        }
    }

    public void StartLevel(int level)
    {
        StopAllCoroutines();

        currentLevel = level;
        totalScore = 0;
        totalChips = 0;
        totalMultiplier = 1f;
        remainingDiscards = MAX_DISCARDS;
        remainingHands = GetMaxHandsForLevel(level);
        isLevelComplete = false;
        isGameOver = false;

        HideAllCards();
        ClearCurrentHand();

        if (deckManager != null)
        {
            deckManager.InitializeDeck();
            deckManager.ShuffleDrawDeck();
            deckManager.DrawInitialHand(BASE_HAND_SIZE + GetTotalHandSizeBonus());
        }

        if (uiManager != null)
        {
            uiManager.HideGameOver();
            uiManager.UpdateDiscardStatus(remainingDiscards);
            if (deckManager != null)
            {
                uiManager.UpdateDeckCount(deckManager.RemainingCardCount, deckManager.TotalCardCount);
            }
            uiManager.HideHandType();
            uiManager.UpdateScore(0, 0);
            uiManager.UpdateScore(0);
            uiManager.UpdateHandCount(remainingHands, GetMaxHandsForLevel(level));
        }
    }

    public int GetRemainingHands()
    {
        return remainingHands;
    }

    public int GetMaxHands()
    {
        return GetMaxHandsForLevel(currentLevel);
    }

    private void ClearCurrentHand()
    {
        if (currentHand == null)
        {
            return;
        }

        foreach (CardObject card in currentHand)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        currentHand.Clear();
    }

    
    private void UpdateDiscardUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateDiscardStatus(remainingDiscards);
        }
    }

    private void UpdateHandCountUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateHandCount(remainingHands, GetMaxHandsForLevel(currentLevel));
        }
    }

    private System.Collections.IEnumerator ShowJokerSelectionDelayed()
    {
        HideHandCards();
        yield return new WaitForSeconds(LEVEL_END_DELAY);
        ShowJokerSelection();
    }

    private void ShowJokerSelection()
    {
        HideHandCards();
        
        if (uiManager != null)
        {
            uiManager.ShowJokerSelection();
        }
    }

    public void HideHandCards()
    {
        if (currentHand != null)
        {
            foreach (CardObject card in currentHand)
            {
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(false);
                }
            }
        }
        
        CardObject[] allCardsInScene = FindObjectsByType<CardObject>(FindObjectsSortMode.None);
        if (allCardsInScene != null)
        {
            foreach (CardObject card in allCardsInScene)
            {
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowHandCards()
    {
        if (currentHand != null)
        {
            foreach (CardObject card in currentHand)
            {
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(true);
                }
            }
        }
    }
}
