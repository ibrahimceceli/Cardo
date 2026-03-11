using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class JokerSelectionManager : MonoBehaviour
{
    [Header("References")]
    public Transform[] jokerSpawnPoints = new Transform[3];
    [Header("Current Joker Spawn Points (For Replacement - Only needed if max jokers reached)")]
    public Transform[] currentJokerSpawnPoints = new Transform[5];
    [Header("Joker Cards (Optional - Auto-filled from DeckManager if empty)")]
    public CardData[] allJokerCards;
    [Header("Tooltip")]
    public JokerTooltip jokerTooltip;
    [Header("Swap Buttons")]
    public Button swapButton;
    public Button continueButton;
    
    private HandManager handManager;
    private UIManager uiManager;
    private DeckManager deckManager;
    
    private List<GameObject> jokerUIElements = new List<GameObject>();
    private List<GameObject> currentJokerUIElements = new List<GameObject>();
    private CardData pendingNewJoker = null;
    private CardData pendingOldJoker = null;
    private bool isReplacementMode = false;
    private bool hasSwapped = false;
    private Vector2[] originalSpawnPointPositions = new Vector2[3];
    private Dictionary<CardData, GameObject> newJokerCardMap = new Dictionary<CardData, GameObject>();
    private Dictionary<CardData, GameObject> oldJokerCardMap = new Dictionary<CardData, GameObject>();
    private Dictionary<CardData, bool> newJokerSelectedMap = new Dictionary<CardData, bool>();
    private Dictionary<CardData, bool> oldJokerSelectedMap = new Dictionary<CardData, bool>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private const float selectionScale = 1.1f;

    void Awake()
    {
        handManager = FindFirstObjectByType<HandManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        deckManager = FindFirstObjectByType<DeckManager>();
        
        if (jokerTooltip == null)
        {
            jokerTooltip = FindFirstObjectByType<JokerTooltip>();
        }
    }

    void OnEnable()
    {
        if (swapButton != null)
        {
            swapButton.onClick.AddListener(HandleSwapButtonClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HandleContinueButtonClicked);
        }
    }

    void OnDisable()
    {
        if (swapButton != null)
        {
            swapButton.onClick.RemoveListener(HandleSwapButtonClicked);
        }
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueButtonClicked);
        }
    }

    public void ShowJokers()
    {
        if (jokerSpawnPoints == null || jokerSpawnPoints.Length != 3)
        {
            return;
        }

        if (handManager != null)
        {
            handManager.HideHandCards();
        }

        ClearJokerCards();
        ClearCurrentJokerCards();

        isReplacementMode = false;
        pendingNewJoker = null;
        pendingOldJoker = null;
        hasSwapped = false;
        newJokerCardMap.Clear();
        oldJokerCardMap.Clear();
        newJokerSelectedMap.Clear();
        oldJokerSelectedMap.Clear();
        originalScales.Clear();

        bool isFull = handManager != null && handManager.GetJokerCount() >= 5;

        for (int i = 0; i < jokerSpawnPoints.Length && i < originalSpawnPointPositions.Length; i++)
        {
            if (jokerSpawnPoints[i] != null)
            {
                RectTransform spawnRect = jokerSpawnPoints[i] as RectTransform;
                if (spawnRect == null)
                {
                    spawnRect = jokerSpawnPoints[i].GetComponent<RectTransform>();
                }
                if (spawnRect != null)
                {
                    if (originalSpawnPointPositions[i] == Vector2.zero)
                    {
                        originalSpawnPointPositions[i] = spawnRect.anchoredPosition;
                    }
                    
                    if (isFull)
                    {
                        spawnRect.anchoredPosition = new Vector2(originalSpawnPointPositions[i].x, 125f);
                    }
                    else
                    {
                        spawnRect.anchoredPosition = originalSpawnPointPositions[i];
                    }
                }
            }
        }

        if (isFull)
        {
            isReplacementMode = true;
            ShowCurrentJokersForReplacement();
            
            if (swapButton != null)
            {
                swapButton.gameObject.SetActive(true);
                swapButton.interactable = false;
            }
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.interactable = true;
            }
            
            UpdateButtonStates();
        }
        else
        {
            if (swapButton != null)
            {
                swapButton.gameObject.SetActive(false);
            }
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }
        }

        List<CardData> allJokers = GetAllJokerCards();
        
        if (allJokers.Count == 0)
        {
            return;
        }

        List<CardData> selectedJokers = SelectRandomJokers(allJokers, 3);

        for (int i = 0; i < selectedJokers.Count && i < jokerSpawnPoints.Length; i++)
        {
            if (selectedJokers[i] == null || !selectedJokers[i].IsJoker)
            {
                continue;
            }

            if (jokerSpawnPoints[i] == null)
            {
                continue;
            }

            CreateJokerUICard(selectedJokers[i], i, isFull);
        }
    }

    private List<CardData> GetAllJokerCards()
    {
        List<CardData> jokers = new List<CardData>();

        if (allJokerCards != null && allJokerCards.Length > 0)
        {
            foreach (CardData cardData in allJokerCards)
            {
                if (cardData != null && cardData.IsJoker && !jokers.Contains(cardData))
                {
                    jokers.Add(cardData);
                }
            }
        }

        if (deckManager != null && deckManager.allCardData != null)
        {
            foreach (CardData cardData in deckManager.allCardData)
            {
                if (cardData != null && cardData.IsJoker && !jokers.Contains(cardData))
                {
                    jokers.Add(cardData);
                }
            }
        }

        CardData[] allJokersFromResources = Resources.LoadAll<CardData>("");
        if (allJokersFromResources != null)
        {
            foreach (CardData cardData in allJokersFromResources)
            {
                if (cardData != null && cardData.IsJoker && !jokers.Contains(cardData))
                {
                    jokers.Add(cardData);
                }
            }
        }

        return jokers;
    }

    private List<CardData> SelectRandomJokers(List<CardData> allJokers, int count)
    {
        List<CardData> selected = new List<CardData>();

        if (allJokers == null || allJokers.Count == 0 || count <= 0)
        {
            return selected;
        }

        List<CardData> availableJokers = new List<CardData>(allJokers);
        
        if (handManager != null)
        {
            List<CardData> alreadySelectedJokers = handManager.GetSelectedJokers();
            if (alreadySelectedJokers != null && alreadySelectedJokers.Count > 0)
            {
                foreach (CardData selectedJoker in alreadySelectedJokers)
                {
                    if (selectedJoker != null)
                    {
                        availableJokers.Remove(selectedJoker);
                    }
                }
            }
        }
        
        if (availableJokers.Count == 0)
        {
            return selected;
        }
        
        System.Random rng = new System.Random();
        int selectCount = System.Math.Min(count, availableJokers.Count);
        
        for (int i = 0; i < selectCount; i++)
        {
            int randomIndex = rng.Next(0, availableJokers.Count);
            selected.Add(availableJokers[randomIndex]);
            availableJokers.RemoveAt(randomIndex);
        }

        return selected;
    }

    private void CreateJokerUICard(CardData jokerData, int index, bool isForReplacement)
    {
        if (jokerData == null)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        if (jokerSpawnPoints[index] == null)
        {
            return;
        }

        GameObject jokerPanel = new GameObject($"JokerCard_{index}");
        jokerPanel.transform.SetParent(jokerSpawnPoints[index], false);
        
        RectTransform rectTransform = jokerPanel.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        
        float cardWidth = isForReplacement ? 150f : 200f;
        float cardHeight = isForReplacement ? 225f : 300f;
        rectTransform.sizeDelta = new Vector2(cardWidth, cardHeight);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image panelImage = jokerPanel.AddComponent<Image>();
        if (jokerData.cardFrontGraphic != null)
        {
            panelImage.sprite = jokerData.cardFrontGraphic;
            panelImage.color = Color.white;
        }
        else
        {
            panelImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        }

        Button button = jokerPanel.AddComponent<Button>();
        if (isForReplacement)
        {
            button.onClick.AddListener(() => ToggleNewJokerSelection(jokerData));
            newJokerCardMap[jokerData] = jokerPanel;
            newJokerSelectedMap[jokerData] = false;
        }
        else
        {
            button.onClick.AddListener(() => SelectJoker(jokerData));
        }

        originalScales[jokerPanel] = rectTransform.localScale;
        AddTooltipToJokerCard(jokerPanel, jokerData);

        jokerUIElements.Add(jokerPanel);
    }

    private void SelectJoker(CardData selectedJokerData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (selectedJokerData == null || !selectedJokerData.IsJoker)
        {
            return;
        }

        if (handManager != null)
        {
            if (handManager.GetJokerCount() >= 5)
            {
                return;
            }
            
            handManager.AddJoker(selectedJokerData);
            
            int nextLevel = handManager.CurrentLevel + 1;
            if (nextLevel <= 10)
            {
                handManager.StartLevel(nextLevel);
            }
            
            if (uiManager != null)
            {
                uiManager.UpdateJokerCount(handManager.GetJokerCount());
            }
        }

        ClearJokerCards();
        ClearCurrentJokerCards();

        if (jokerTooltip != null)
        {
            jokerTooltip.HideTooltip();
        }

        if (uiManager != null)
        {
            uiManager.HideJokerSelection();
        }

        isReplacementMode = false;
        pendingNewJoker = null;
    }

    private void ToggleNewJokerSelection(CardData newJokerData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (newJokerData == null || !newJokerData.IsJoker || hasSwapped)
        {
            return;
        }

        // Toggle selection
        bool isSelected = newJokerSelectedMap.ContainsKey(newJokerData) ? newJokerSelectedMap[newJokerData] : false;
        isSelected = !isSelected;
        newJokerSelectedMap[newJokerData] = isSelected;

        if (isSelected)
        {
            pendingNewJoker = newJokerData;
            // Deselect other new jokers
            foreach (var kvp in newJokerSelectedMap.ToList())
            {
                if (kvp.Key != newJokerData && kvp.Value)
                {
                    newJokerSelectedMap[kvp.Key] = false;
                }
            }
        }
        else
        {
            if (pendingNewJoker == newJokerData)
            {
                pendingNewJoker = null;
            }
        }

        isReplacementMode = true;

        if (jokerTooltip != null)
        {
            jokerTooltip.HideTooltip();
        }

        UpdateJokerCardHighlights();
        UpdateButtonStates();
    }

    private void ToggleOldJokerSelection(CardData oldJokerData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (oldJokerData == null || !oldJokerData.IsJoker || hasSwapped)
        {
            return;
        }

        // Toggle selection
        bool isSelected = oldJokerSelectedMap.ContainsKey(oldJokerData) ? oldJokerSelectedMap[oldJokerData] : false;
        isSelected = !isSelected;
        oldJokerSelectedMap[oldJokerData] = isSelected;

        if (isSelected)
        {
            pendingOldJoker = oldJokerData;
            // Deselect other old jokers
            foreach (var kvp in oldJokerSelectedMap.ToList())
            {
                if (kvp.Key != oldJokerData && kvp.Value)
                {
                    oldJokerSelectedMap[kvp.Key] = false;
                }
            }
        }
        else
        {
            if (pendingOldJoker == oldJokerData)
            {
                pendingOldJoker = null;
            }
        }

        UpdateJokerCardHighlights();
        UpdateButtonStates();
    }

    private void ShowCurrentJokersForReplacement()
    {
        if (handManager == null)
        {
            return;
        }

        List<CardData> currentJokers = handManager.GetSelectedJokers();
        
        if (currentJokers == null || currentJokers.Count == 0)
        {
            return;
        }

        if (currentJokerSpawnPoints == null || currentJokerSpawnPoints.Length == 0)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        int cardCount = currentJokers.Count;

        for (int i = 0; i < cardCount && i < currentJokerSpawnPoints.Length; i++)
        {
            if (currentJokers[i] == null || !currentJokers[i].IsJoker)
            {
                continue;
            }

            if (currentJokerSpawnPoints[i] == null)
            {
                continue;
            }

            RectTransform spawnRect = currentJokerSpawnPoints[i] as RectTransform;
            if (spawnRect == null)
            {
                spawnRect = currentJokerSpawnPoints[i].GetComponent<RectTransform>();
                if (spawnRect == null)
                {
                    continue;
                }
            }

            if (isReplacementMode)
            {
                // Limit dolduğunda özel pozisyonlar
                float[] xPositions = new float[] { -400f, -200f, 0f, 200f, 400f };
                float xPos = i < xPositions.Length ? xPositions[i] : 0f;
                spawnRect.anchoredPosition = new Vector2(xPos, -120f);
            }
            else
            {
                // Limit dolmadığında orijinal pozisyon (ekranın ortası)
                float cardWidth = 150f;
                float cardSpacing = 60f;
                float totalWidth = (cardCount - 1) * cardSpacing + cardWidth;
                float startX = -totalWidth / 2f + cardWidth / 2f;
                spawnRect.anchoredPosition = new Vector2(startX + i * cardSpacing, -120f);
            }
            spawnRect.anchorMin = new Vector2(0.5f, 0.5f);
            spawnRect.anchorMax = new Vector2(0.5f, 0.5f);
            spawnRect.pivot = new Vector2(0.5f, 0.5f);

            CreateCurrentJokerUICard(currentJokers[i], i);
        }
    }

    private void CreateCurrentJokerUICard(CardData jokerData, int index)
    {
        if (jokerData == null || currentJokerSpawnPoints == null || index >= currentJokerSpawnPoints.Length || currentJokerSpawnPoints[index] == null)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        GameObject jokerPanel = new GameObject($"CurrentJokerCard_{index}");
        jokerPanel.transform.SetParent(currentJokerSpawnPoints[index], false);
        
        RectTransform rectTransform = jokerPanel.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(150f, 225f);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Image panelImage = jokerPanel.AddComponent<Image>();
        if (jokerData.cardFrontGraphic != null)
        {
            panelImage.sprite = jokerData.cardFrontGraphic;
            panelImage.color = Color.white;
        }
        else
        {
            if (isReplacementMode)
            {
                panelImage.color = new Color(0.3f, 0.2f, 0.2f, 0.9f);
            }
            else
            {
                panelImage.color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
            }
        }

        Button button = jokerPanel.AddComponent<Button>();
        if (isReplacementMode)
        {
            button.onClick.AddListener(() => ToggleOldJokerSelection(jokerData));
            oldJokerCardMap[jokerData] = jokerPanel;
            oldJokerSelectedMap[jokerData] = false;
        }

        originalScales[jokerPanel] = rectTransform.localScale;

        AddTooltipToJokerCard(jokerPanel, jokerData);

        currentJokerUIElements.Add(jokerPanel);
    }

    private void HandleSwapButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (pendingNewJoker == null || pendingOldJoker == null || handManager == null || hasSwapped)
        {
            return;
        }

        if (handManager.ReplaceJoker(pendingOldJoker, pendingNewJoker))
        {
            hasSwapped = true;
            
            if (uiManager != null)
            {
                uiManager.UpdateJokerCount(handManager.GetJokerCount());
            }

            int nextLevel = handManager.CurrentLevel + 1;
            if (nextLevel <= 10)
            {
                handManager.StartLevel(nextLevel);
            }

            ClearJokerCards();
            ClearCurrentJokerCards();

            if (jokerTooltip != null)
            {
                jokerTooltip.HideTooltip();
            }

            if (uiManager != null)
            {
                uiManager.HideJokerSelection();
            }

            isReplacementMode = false;
            pendingNewJoker = null;
            pendingOldJoker = null;
            hasSwapped = false;
            newJokerCardMap.Clear();
            oldJokerCardMap.Clear();
        }
    }

    private void HandleContinueButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (handManager == null)
        {
            return;
        }

        int nextLevel = handManager.CurrentLevel + 1;
        if (nextLevel <= 10)
        {
            handManager.StartLevel(nextLevel);
        }

        ClearJokerCards();
        ClearCurrentJokerCards();

        if (jokerTooltip != null)
        {
            jokerTooltip.HideTooltip();
        }

        if (uiManager != null)
        {
            uiManager.HideJokerSelection();
        }

        isReplacementMode = false;
        pendingNewJoker = null;
        pendingOldJoker = null;
        hasSwapped = false;
        newJokerCardMap.Clear();
        oldJokerCardMap.Clear();
        newJokerSelectedMap.Clear();
        oldJokerSelectedMap.Clear();
        originalScales.Clear();
    }

    private void UpdateButtonStates()
    {
        bool canSwap = pendingNewJoker != null && pendingOldJoker != null && !hasSwapped;
        
        if (swapButton != null)
        {
            swapButton.interactable = canSwap;
        }
        
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
    }

    private void UpdateJokerCardHighlights()
    {
        // Update new joker cards (top row)
        foreach (var kvp in newJokerCardMap)
        {
            if (kvp.Value != null)
            {
                bool isSelected = newJokerSelectedMap.ContainsKey(kvp.Key) && newJokerSelectedMap[kvp.Key];
                Image img = kvp.Value.GetComponent<Image>();
                RectTransform rectTransform = kvp.Value.GetComponent<RectTransform>();
                
                if (img != null)
                {
                    img.color = isSelected ? new Color(1f, 1f, 0.5f, 1f) : Color.white;
                }
                
                if (rectTransform != null && originalScales.ContainsKey(kvp.Value))
                {
                    rectTransform.localScale = isSelected ? originalScales[kvp.Value] * selectionScale : originalScales[kvp.Value];
                }
            }
        }

        // Update old joker cards (bottom row)
        foreach (var kvp in oldJokerCardMap)
        {
            if (kvp.Value != null)
            {
                bool isSelected = oldJokerSelectedMap.ContainsKey(kvp.Key) && oldJokerSelectedMap[kvp.Key];
                Image img = kvp.Value.GetComponent<Image>();
                RectTransform rectTransform = kvp.Value.GetComponent<RectTransform>();
                
                if (img != null)
                {
                    img.color = isSelected ? new Color(1f, 0.5f, 0.5f, 1f) : Color.white;
                }
                
                if (rectTransform != null && originalScales.ContainsKey(kvp.Value))
                {
                    rectTransform.localScale = isSelected ? originalScales[kvp.Value] * selectionScale : originalScales[kvp.Value];
                }
            }
        }
    }

    private void ClearJokerCards()
    {
        foreach (GameObject jokerUI in jokerUIElements)
        {
            if (jokerUI != null)
            {
                Destroy(jokerUI);
            }
        }

        jokerUIElements.Clear();
    }

    private void ClearCurrentJokerCards()
    {
        foreach (GameObject jokerUI in currentJokerUIElements)
        {
            if (jokerUI != null)
            {
                Destroy(jokerUI);
            }
        }

        currentJokerUIElements.Clear();
    }

    private void AddTooltipToJokerCard(GameObject jokerCard, CardData jokerData)
    {
        if (jokerCard == null || jokerData == null || jokerTooltip == null)
        {
            return;
        }

        EventTrigger trigger = jokerCard.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = jokerCard.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { OnJokerPointerEnter(jokerData, data); });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { OnJokerPointerExit(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnJokerPointerEnter(CardData jokerData, BaseEventData eventData)
    {
        if (jokerTooltip == null || jokerData == null)
        {
            return;
        }

        PointerEventData pointerData = eventData as PointerEventData;
        if (pointerData != null)
        {
            jokerTooltip.ShowTooltip(jokerData, pointerData.position);
        }
    }

    private void OnJokerPointerExit()
    {
        if (jokerTooltip != null)
        {
            jokerTooltip.HideTooltip();
        }
    }
}