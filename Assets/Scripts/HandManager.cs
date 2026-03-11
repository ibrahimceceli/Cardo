using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public List<CardObject> currentHand = new List<CardObject>();
    public int totalChips = 0;
    public float totalMultiplier = 1f;
    private int totalScore = 0;
    private int remainingDiscards = 4;
    private const int LEVEL_1_SCORE_THRESHOLD = 200;
    private const int LEVEL_2_SCORE_THRESHOLD = 300;
    private const int LEVEL_3_SCORE_THRESHOLD = 400;
    private const int LEVEL_4_SCORE_THRESHOLD = 500;
    private const int LEVEL_5_SCORE_THRESHOLD = 600;
    private const int LEVEL_6_SCORE_THRESHOLD = 700;
    private const int LEVEL_7_SCORE_THRESHOLD = 800;
    private const int LEVEL_8_SCORE_THRESHOLD = 900;
    private const int LEVEL_9_SCORE_THRESHOLD = 1000;
    private const int LEVEL_10_SCORE_THRESHOLD = 1100;
    private const int LEVEL_1_MAX_HANDS = 5;
    private const int LEVEL_2_MAX_HANDS = 5;
    private const int LEVEL_3_MAX_HANDS = 4;
    private const int LEVEL_4_MAX_HANDS = 4;
    private const int LEVEL_5_MAX_HANDS = 3;
    private const int LEVEL_6_MAX_HANDS = 3;
    private const int LEVEL_7_MAX_HANDS = 3;
    private const int LEVEL_8_MAX_HANDS = 2;
    private const int LEVEL_9_MAX_HANDS = 2;
    private const int LEVEL_10_MAX_HANDS = 2;
    private const int MAX_JOKERS = 5;
    private int currentLevel = 1;
    private int remainingHands = LEVEL_1_MAX_HANDS;
    private bool hasCompletedLevel1 = false;
    private bool hasCompletedLevel2 = false;
    private bool hasCompletedLevel3 = false;
    private bool hasCompletedLevel4 = false;
    private bool hasCompletedLevel5 = false;
    private bool hasCompletedLevel6 = false;
    private bool hasCompletedLevel7 = false;
    private bool hasCompletedLevel8 = false;
    private bool hasCompletedLevel9 = false;
    private bool hasCompletedLevel10 = false;
    private List<CardData> selectedJokers = new List<CardData>();

    public DeckManager deckManager;
    private UIManager uiManager;
    
    private PokerHandEvaluator handEvaluator;
    private HandScoringService scoringService;
    private Camera mainCamera;

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

    void Start()
    {
        if (uiManager != null && uiManager.IsStartScreenActive())
        {
            return;
        }
        
        if (uiManager != null)
        {
            uiManager.UpdateScore(0, 0);
            uiManager.UpdateHandCount(remainingHands, GetMaxHandsForLevel(currentLevel));
        }
        UpdateDiscardUI();
    }
    
    void Update()
    {
        if (hasCompletedLevel1 && currentLevel == 1)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleCardClick();
        }
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
        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0 || selectedCards.Count > 5)
        {
            if (uiManager != null)
            {
                uiManager.HideHandType();
                uiManager.UpdateScore(0, 0);
            }
            return;
        }

        PokerHandType handType = handEvaluator.DetermineHand(selectedCards);

        if (handType != PokerHandType.None)
        {
            if (uiManager != null)
            {
                uiManager.ShowHandType(handType);
                
                int baseChipScore = scoringService.GetBaseScore(handType);
                float multiplier = scoringService.GetMultiplier(handType);
                
                int jokerBonusScore = GetTotalJokerBonusScore();
                float jokerMultiplierBonus = GetTotalJokerMultiplierBonus();
                float handTypeBasedMultiplierBonus = GetHandTypeBasedJokerMultiplierBonus(handType);
                
                List<CardObject> scoringCards = GetCardsThatFormHand(handType, selectedCards);
                int suitBasedBonusScore = GetSuitBasedJokerBonusScore(scoringCards);
                
                baseChipScore += jokerBonusScore + suitBasedBonusScore;
                multiplier += jokerMultiplierBonus + handTypeBasedMultiplierBonus;
                
                uiManager.UpdateScore(baseChipScore, multiplier);
            }
        }
        else
        {
            if (uiManager != null)
            {
                uiManager.HideHandType();
                uiManager.UpdateScore(0, 0);
            }
        }
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

    private void ResetSelection(List<CardObject> playedCards)
    {
        foreach (CardObject card in playedCards)
        {
            if (card.isSelected)
            {
                card.DetectClick(playSound: false);
            }

            currentHand.Remove(card);
            Destroy(card.gameObject);
        }
    }


    public bool PlayHand()
    {
        if (remainingHands <= 0)
        {
            int scoreThreshold = GetScoreThresholdForLevel(currentLevel);
            bool hasCompletedLevel = false;
            
            if (currentLevel == 1 && !hasCompletedLevel1)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 2 && !hasCompletedLevel2)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 3 && !hasCompletedLevel3)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 4 && !hasCompletedLevel4)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 5 && !hasCompletedLevel5)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 6 && !hasCompletedLevel6)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 7 && !hasCompletedLevel7)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 8 && !hasCompletedLevel8)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 9 && !hasCompletedLevel9)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            else if (currentLevel == 10 && !hasCompletedLevel10)
            {
                hasCompletedLevel = totalScore >= scoreThreshold;
            }
            
            if (!hasCompletedLevel && uiManager != null)
            {
                uiManager.ShowLoseMessage();
            }
            return false;
        }

        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0 || selectedCards.Count > 5)
        {
            if (uiManager != null)
            {
                uiManager.UpdateDiscardStatus(remainingDiscards);
            }
            return false;
        }

        PokerHandType handType = handEvaluator.DetermineHand(selectedCards);

        int baseChipScore = scoringService.GetBaseScore(handType);
        float multiplier = scoringService.GetMultiplier(handType);
        
        int jokerBonusScore = GetTotalJokerBonusScore();
        float jokerMultiplierBonus = GetTotalJokerMultiplierBonus();
        float handTypeBasedMultiplierBonus = GetHandTypeBasedJokerMultiplierBonus(handType);
        
        multiplier += jokerMultiplierBonus;
        multiplier += handTypeBasedMultiplierBonus;
        
        List<CardObject> scoringCards = GetCardsThatFormHand(handType, selectedCards);
        int suitBasedBonusScore = GetSuitBasedJokerBonusScore(scoringCards);
        
        int cardChipTotal = 0;
        foreach (var card in scoringCards)
        {
            if (card != null && card.cardInfo != null)
            {
                cardChipTotal += card.cardInfo.NumericValue;
            }
        }
        
        int totalChipsForHand = baseChipScore + cardChipTotal + jokerBonusScore + suitBasedBonusScore;
        int finalScore = Mathf.RoundToInt(totalChipsForHand * multiplier);

        totalScore += finalScore;
        totalChips += finalScore;
        totalMultiplier = multiplier;

        bool wasLastHand = (remainingHands == 1);
        remainingHands--;
        UpdateHandCountUI();

        UpdateScoreUI(totalChipsForHand, multiplier, totalScore);
        
        bool levelCompleted = false;
        if (wasLastHand)
        {
            int scoreThreshold = GetScoreThresholdForLevel(currentLevel);
            if (currentLevel == 1 && !hasCompletedLevel1 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel1 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 2 && !hasCompletedLevel2 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel2 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 3 && !hasCompletedLevel3 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel3 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 4 && !hasCompletedLevel4 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel4 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 5 && !hasCompletedLevel5 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel5 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 6 && !hasCompletedLevel6 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel6 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 7 && !hasCompletedLevel7 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel7 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 8 && !hasCompletedLevel8 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel8 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 9 && !hasCompletedLevel9 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel9 = true;
                levelCompleted = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
            else if (currentLevel == 10 && !hasCompletedLevel10 && totalScore >= scoreThreshold)
            {
                hasCompletedLevel10 = true;
                levelCompleted = true;
                if (uiManager != null)
                {
                    uiManager.ShowWinMessage();
                }
            }
            
            if (!levelCompleted)
            {
                CheckLevelCompletionAfterLastHand();
            }
        }

        CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);
        
        // Register played cards in DeckManager BEFORE destroying them
        if (deckManager != null)
        {
            deckManager.RegisterPlayedDiscardedCards(selectedCards);
        }
        
        ResetSelection(selectedCards);
        
        if (levelCompleted)
        {
            HideHandCards();
        }
        else
        {
            bool shouldDrawCards = true;
            if (currentLevel == 1 && hasCompletedLevel1)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 2 && hasCompletedLevel2)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 3 && hasCompletedLevel3)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 4 && hasCompletedLevel4)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 5 && hasCompletedLevel5)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 6 && hasCompletedLevel6)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 7 && hasCompletedLevel7)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 8 && hasCompletedLevel8)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 9 && hasCompletedLevel9)
            {
                shouldDrawCards = false;
            }
            else if (currentLevel == 10 && hasCompletedLevel10)
            {
                shouldDrawCards = false;
            }
            
            if (shouldDrawCards)
            {
                int cardsToDraw = transformData.positions.Count;
                
                if (deckManager != null && deckManager.RemainingCardCount >= cardsToDraw)
                {
                    for (int i = 0; i < cardsToDraw; i++)
                    {
                        CardObject newCard = deckManager.DrawCardAtPosition(transformData.positions[i], transformData.rotations[i], transformData.sortingOrders[i]);
                        if (newCard == null)
                        {
                            break;
                        }
                    }
                }
            }
        }
        
        UpdateSelectedHandType();
        
        if (uiManager != null)
        {
            uiManager.UpdateDeckInfo();
        }
        
        return true;
    }

    public void DiscardSelectedAndDraw()
    {
        List<CardObject> selectedCards = GetSelectedCards();

        if (selectedCards.Count == 0)
        {
            if (uiManager != null)
            {
                uiManager.UpdateDiscardStatus(remainingDiscards);
            }
            return;
        }

        if (selectedCards.Count > 5)
        {
            if (uiManager != null)
            {
                uiManager.UpdateDiscardStatus(remainingDiscards);
            }
            return;
        }

        if (remainingDiscards <= 0)
        {
            if (uiManager != null)
            {
                uiManager.UpdateDiscardStatus(remainingDiscards);
            }
            return;
        }

        remainingDiscards--;

        CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);
        
        // Register discarded cards in DeckManager
        if (deckManager != null)
        {
            deckManager.RegisterPlayedDiscardedCards(selectedCards);
        }
        
        foreach (CardObject card in selectedCards)
        {
            currentHand.Remove(card);
            Destroy(card.gameObject);
        }

        if (!hasCompletedLevel1 || currentLevel != 1)
        {
            int cardsToDraw = transformData.positions.Count;
            
            if (deckManager != null && deckManager.RemainingCardCount >= cardsToDraw)
            {
                for (int i = 0; i < cardsToDraw; i++)
                {
                    CardObject newCard = deckManager.DrawCardAtPosition(transformData.positions[i], transformData.rotations[i], transformData.sortingOrders[i]);
                    if (newCard == null)
                    {
                        break;
                    }
                }
            }
        }
        
        UpdateDiscardUI();
        
        if (uiManager != null)
        {
            uiManager.UpdateDeckInfo();
        }
    }
    
    public int GetRemainingDiscards()
    {
        return remainingDiscards;
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
    }

    public bool HasCompletedLevel1
    {
        get { return hasCompletedLevel1; }
    }

    public bool HasCompletedLevel2
    {
        get { return hasCompletedLevel2; }
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
        switch (level)
        {
            case 1: return LEVEL_1_MAX_HANDS;
            case 2: return LEVEL_2_MAX_HANDS;
            case 3: return LEVEL_3_MAX_HANDS;
            case 4: return LEVEL_4_MAX_HANDS;
            case 5: return LEVEL_5_MAX_HANDS;
            case 6: return LEVEL_6_MAX_HANDS;
            case 7: return LEVEL_7_MAX_HANDS;
            case 8: return LEVEL_8_MAX_HANDS;
            case 9: return LEVEL_9_MAX_HANDS;
            case 10: return LEVEL_10_MAX_HANDS;
            default: return LEVEL_1_MAX_HANDS;
        }
    }

    private int GetScoreThresholdForLevel(int level)
    {
        switch (level)
        {
            case 1: return LEVEL_1_SCORE_THRESHOLD;
            case 2: return LEVEL_2_SCORE_THRESHOLD;
            case 3: return LEVEL_3_SCORE_THRESHOLD;
            case 4: return LEVEL_4_SCORE_THRESHOLD;
            case 5: return LEVEL_5_SCORE_THRESHOLD;
            case 6: return LEVEL_6_SCORE_THRESHOLD;
            case 7: return LEVEL_7_SCORE_THRESHOLD;
            case 8: return LEVEL_8_SCORE_THRESHOLD;
            case 9: return LEVEL_9_SCORE_THRESHOLD;
            case 10: return LEVEL_10_SCORE_THRESHOLD;
            default: return LEVEL_1_SCORE_THRESHOLD;
        }
    }

    public void StartLevel2()
    {
        StartLevel(2);
    }

    public void StartLevel(int level)
    {
        currentLevel = level;
        totalScore = 0;
        totalChips = 0;
        totalMultiplier = 1f;
        remainingDiscards = 4;
        remainingHands = GetMaxHandsForLevel(level);

        HideAllCards();
        ClearCurrentHand();

        if (deckManager != null)
        {
            deckManager.InitializeDeck();
            deckManager.ShuffleDrawDeck();
            
            int baseHandSize = 8;
            int handSizeBonus = GetTotalHandSizeBonus();
            deckManager.DrawInitialHand(baseHandSize + handSizeBonus);
        }

        if (uiManager != null)
        {
            uiManager.UpdateDiscardStatus(remainingDiscards);
            if (deckManager != null)
            {
                uiManager.UpdateDeckCount(deckManager.RemainingCardCount, deckManager.TotalCardCount);
            }
            uiManager.UpdateScore(0, 0);
            uiManager.UpdateScore(0);
            uiManager.UpdateHandCount(remainingHands, GetMaxHandsForLevel(level));
            uiManager.HideLoseMessage();
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

    
    private void UpdateScoreUI(int baseChipScore, float multiplier, int totalScore)
    {
        if (uiManager != null)
        {
            uiManager.UpdateScore(baseChipScore, multiplier);
            uiManager.UpdateScore(totalScore);
        }
        
        CheckLevelCompletion();
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
    
    private void CheckLevelCompletion()
    {
        if (remainingHands <= 0)
        {
            return;
        }
        
        int scoreThreshold = GetScoreThresholdForLevel(currentLevel);
        
        if (currentLevel == 1 && !hasCompletedLevel1)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel1 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 2 && !hasCompletedLevel2)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel2 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 3 && !hasCompletedLevel3)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel3 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 4 && !hasCompletedLevel4)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel4 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 5 && !hasCompletedLevel5)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel5 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 6 && !hasCompletedLevel6)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel6 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 7 && !hasCompletedLevel7)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel7 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 8 && !hasCompletedLevel8)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel8 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 9 && !hasCompletedLevel9)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel9 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 10 && !hasCompletedLevel10)
        {
            if (totalScore >= scoreThreshold)
            {
                hasCompletedLevel10 = true;
                
                if (uiManager != null)
                {
                    uiManager.ShowWinMessage();
                }
            }
        }
    }
    
    private void CheckLevelCompletionAfterLastHand()
    {
        int scoreThreshold = GetScoreThresholdForLevel(currentLevel);
        bool hasCompletedLevel = false;
        
        if (currentLevel == 1 && !hasCompletedLevel1)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel1 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 2 && !hasCompletedLevel2)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel2 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 3 && !hasCompletedLevel3)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel3 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 4 && !hasCompletedLevel4)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel4 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 5 && !hasCompletedLevel5)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel5 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 6 && !hasCompletedLevel6)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel6 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 7 && !hasCompletedLevel7)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel7 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 8 && !hasCompletedLevel8)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel8 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 9 && !hasCompletedLevel9)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel9 = true;
                StartCoroutine(ShowJokerSelectionDelayed());
            }
        }
        else if (currentLevel == 10 && !hasCompletedLevel10)
        {
            hasCompletedLevel = totalScore >= scoreThreshold;
            if (hasCompletedLevel)
            {
                hasCompletedLevel10 = true;
                
                if (uiManager != null)
                {
                    uiManager.ShowWinMessage();
                }
            }
        }
        
        if (!hasCompletedLevel && uiManager != null)
        {
            uiManager.ShowLoseMessage();
        }
    }

    private System.Collections.IEnumerator ShowJokerSelectionDelayed()
    {
        HideHandCards();
        yield return new WaitForSeconds(1f);
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
