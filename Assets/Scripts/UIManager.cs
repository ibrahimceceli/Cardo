using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public HandManager handManager;
    public Button playHandButton;
    public Button discardButton;
    public TMP_Text statusText;
    public TMP_Text chipsText;
    public TMP_Text multiplierText;
    public TMP_Text scoreText;
    public TMP_Text handTypeText;
    public TMP_Text deckCountText;
    public TMP_Text jokerCountText;
    public TMP_Text handCountText;
    
    [Header("Deck Info")]
    public TMP_Text deckInfoText;
    
    [Header("Joker Selection UI")]
    public GameObject jokerSelectionUI;
    public GameObject normalGameUI;
    public JokerSelectionManager jokerSelectionManager;
    
    [Header("Joker View UI")]
    public GameObject jokerViewUI;
    public Transform[] jokerViewSpawnPoints;
    public Button jokerViewBackButton;
    [Header("Tooltip")]
    public JokerTooltip jokerTooltip;
    
    [Header("Start Screen UI")]
    public GameObject startScreenUI;
    public TMP_Text clickToPlayText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverTitleText;
    public TMP_Text gameOverDetailText;
    public Button restartButton;

    [Header("Background Panel Manager")]
    public BackgroundPanelManager backgroundPanelManager;
    
    [Header("Background Color")]
    [Tooltip("Tüm background panelleri için kullanılacak renk")]
    [SerializeField]
    private Color backgroundColor = new Color(0f, 0f, 0f, 0.2f);
    
    private const float BACKGROUND_ALPHA = 0.2f;
    
    [Header("Background Padding")]
    [Tooltip("Arka plan dikdörtgenlerinin text'e göre yatay padding'i")]
    public float backgroundPaddingX = 8f;
    [Tooltip("Arka plan dikdörtgenlerinin text'e göre dikey padding'i")]
    public float backgroundPaddingY = 4f;
    
    [Header("Background Position Offset")]
    [Tooltip("Arka plan dikdörtgenlerinin pozisyon offset'i (X)")]
    public Vector2 backgroundPositionOffset = Vector2.zero;
    [Tooltip("Arka plan dikdörtgenlerinin boyut çarpanı")]
    public Vector2 backgroundSizeMultiplier = Vector2.one;
    
    private Image chipsBackground;
    private Image multiplierBackground;
    private Image scoreBackground;
    private Image discardBackground;
    private Image handTypeBackground;
    private Image deckCountBackground;
    private Image jokerCountBackground;
    private Image handCountBackground;
    
    private List<GameObject> jokerViewUIElements = new List<GameObject>();
    private Canvas cachedCanvas;

    private Canvas GetCanvas()
    {
        if (cachedCanvas == null)
        {
            cachedCanvas = GetComponentInParent<Canvas>();
            if (cachedCanvas == null && chipsText != null)
            {
                cachedCanvas = chipsText.GetComponentInParent<Canvas>();
            }
        }
        return cachedCanvas;
    }
    
    private void TryFindBackgroundPanelManager()
    {
        if (backgroundPanelManager == null)
        {
            backgroundPanelManager = GetComponent<BackgroundPanelManager>();
            if (backgroundPanelManager == null)
            {
                backgroundPanelManager = FindFirstObjectByType<BackgroundPanelManager>();
            }
        }
        
        if (backgroundPanelManager != null)
        {
            backgroundPanelManager.backgroundPaddingX = backgroundPaddingX;
            backgroundPanelManager.backgroundPaddingY = backgroundPaddingY;
            backgroundPanelManager.backgroundPositionOffset = backgroundPositionOffset;
            backgroundPanelManager.backgroundSizeMultiplier = backgroundSizeMultiplier;
        }
    }

    private void EnsureBackgroundPanelManager()
    {
        if (backgroundPanelManager == null)
        {
            backgroundPanelManager = GetComponent<BackgroundPanelManager>();
            if (backgroundPanelManager == null)
            {
                backgroundPanelManager = FindFirstObjectByType<BackgroundPanelManager>();
            }
            if (backgroundPanelManager == null)
            {
                GameObject bgManagerObj = new GameObject("BackgroundPanelManager");
                backgroundPanelManager = bgManagerObj.AddComponent<BackgroundPanelManager>();
                bgManagerObj.transform.SetParent(transform, false);
            }
        }
        
        if (backgroundPanelManager != null)
        {
            backgroundPanelManager.backgroundPaddingX = backgroundPaddingX;
            backgroundPanelManager.backgroundPaddingY = backgroundPaddingY;
            backgroundPanelManager.backgroundPositionOffset = backgroundPositionOffset;
            backgroundPanelManager.backgroundSizeMultiplier = backgroundSizeMultiplier;
        }
    }
    
    private void EnsureTextActive(TMP_Text text)
    {
        if (text != null)
        {
            text.gameObject.SetActive(true);
        }
    }

    private void ConfigureText(TMP_Text text, bool setActive = true)
    {
        if (text != null)
        {
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            if (setActive)
            {
                EnsureTextActive(text);
            }
        }
    }
    
    void Awake()
    {
        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }
        
        if (jokerTooltip == null)
        {
            jokerTooltip = FindFirstObjectByType<JokerTooltip>();
        }
        
        EnsureBackgroundPanelManager();
        
        if (Application.isPlaying)
        {
            HideNormalGameUI();
        }
    }

    void Start()
    {
        ShowStartScreen();

        // Without a start screen the run begins immediately.
        if (!IsStartScreenActive())
        {
            StartCoroutine(StartGameAfterScreenHide());
        }
    }
    
    void OnValidate()
    {
        TryFindBackgroundPanelManager();
    }
    
    
    private System.Collections.IEnumerator CreateBackgroundPanelsDelayed()
    {
        yield return null;
        
        if (backgroundPanelManager == null)
        {
            EnsureBackgroundPanelManager();
        }
        
        if (Application.isPlaying && backgroundPanelManager != null)
        {
            var backgroundMap = new System.Collections.Generic.Dictionary<string, System.Action<Transform>>
            {
                { "ChipsBackground", (child) => backgroundPanelManager.SetupBackground(child, ref chipsBackground, ref chipsText, (text) => ConfigureText(text)) },
                { "MultiplierBackground", (child) => backgroundPanelManager.SetupBackground(child, ref multiplierBackground, ref multiplierText, (text) => ConfigureText(text)) },
                { "ScoreBackground", (child) => backgroundPanelManager.SetupBackground(child, ref scoreBackground, ref scoreText, (text) => ConfigureText(text)) },
                { "DiscardBackground", (child) => backgroundPanelManager.SetupBackground(child, ref discardBackground, ref statusText, (text) => ConfigureText(text)) },
                { "HandTypeBackground", (child) => backgroundPanelManager.SetupBackground(child, ref handTypeBackground, ref handTypeText, (text) => ConfigureText(text)) },
                { "DeckCountBackground", (child) => backgroundPanelManager.SetupBackground(child, ref deckCountBackground, ref deckCountText, (text) => ConfigureText(text)) },
                { "JokerCountBackground", (child) => backgroundPanelManager.SetupBackground(child, ref jokerCountBackground, ref jokerCountText, (text) => ConfigureText(text)) },
                { "HandCountBackground", (child) => backgroundPanelManager.SetupBackground(child, ref handCountBackground, ref handCountText, (text) => ConfigureText(text)) }
            };

            yield return backgroundPanelManager.CreateBackgroundPanelsDelayed(
                backgroundMap,
                ConfigureTextSettingsRuntime,
                null);
            
            yield return null;
        }
        else
        {
            CreateBackgroundPanelsIfNeeded();
        }
    }
    
    private void ConfigureTextSettingsRuntime()
    {
        ConfigureText(chipsText, setActive: false);
        ConfigureText(multiplierText, setActive: false);
        ConfigureText(scoreText, setActive: false);
        ConfigureText(statusText, setActive: false);
        ConfigureText(handTypeText, setActive: false);
        ConfigureText(deckCountText, setActive: false);
        ConfigureText(jokerCountText, setActive: false);
        ConfigureText(handCountText, setActive: false);
    }
    
    

    private System.Collections.IEnumerator InitializeScoreUIDelayed()
    {
        yield return null;
        
        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }
        
        if (handManager != null)
        {
            UpdateScore(0, 0);
            UpdateScore(handManager.TotalScore);
            UpdateDiscardStatus(handManager.GetRemainingDiscards());
            UpdateHandCount(handManager.GetRemainingHands(), handManager.GetMaxHands());
        }

        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        if (deckManager != null)
        {
            UpdateDeckCount(deckManager.RemainingCardCount, deckManager.TotalCardCount);
        }

        if (handManager != null)
        {
            UpdateJokerCount(handManager.GetJokerCount());
        }

        if (deckInfoText != null)
        {
            deckInfoText.gameObject.SetActive(false);
        }

        if (deckCountText != null)
        {
            Button deckTextButton = deckCountText.GetComponent<Button>();
            if (deckTextButton == null)
            {
                deckTextButton = deckCountText.gameObject.AddComponent<Button>();
            }
            
            deckTextButton.onClick.RemoveAllListeners();
            deckTextButton.onClick.AddListener(HandleDeckButtonClicked);
            
            ColorBlock colors = deckTextButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            deckTextButton.colors = colors;
        }

        if (jokerCountText != null)
        {
            Button jokerTextButton = jokerCountText.GetComponent<Button>();
            if (jokerTextButton == null)
            {
                jokerTextButton = jokerCountText.gameObject.AddComponent<Button>();
            }
            
            jokerTextButton.onClick.RemoveAllListeners();
            jokerTextButton.onClick.AddListener(HandleJokerCountButtonClicked);
            
            ColorBlock colors = jokerTextButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            jokerTextButton.colors = colors;
        }

        if (jokerViewBackButton != null)
        {
            jokerViewBackButton.onClick.RemoveAllListeners();
            jokerViewBackButton.onClick.AddListener(HandleJokerViewBackButtonClicked);
        }
    }
    
    private void EnsureTextsVisible()
    {
        EnsureTextActive(chipsText);
        EnsureTextActive(multiplierText);
        EnsureTextActive(scoreText);
        EnsureTextActive(statusText);
        EnsureTextActive(deckCountText);
        EnsureTextActive(jokerCountText);
        EnsureTextActive(handCountText);
    }

    // Button listeners are wired here only; the scene must not add persistent onClick calls for them.
    void OnEnable()
    {
        if (Application.isPlaying)
        {
            if (playHandButton != null)
            {
                playHandButton.onClick.AddListener(HandlePlayHandClicked);
            }
            if (discardButton != null)
            {
                discardButton.onClick.AddListener(HandleDiscardClicked);
            }
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            SetupStartScreenClick();
            
            if (handManager != null && multiplierText != null)
            {
                UpdateScore(handManager.totalChips, handManager.totalMultiplier);
                UpdateDiscardStatus(handManager.GetRemainingDiscards());
            }
        }
    }

    void OnDisable()
    {
        if (playHandButton != null)
        {
            playHandButton.onClick.RemoveListener(HandlePlayHandClicked);
        }
        if (discardButton != null)
        {
            discardButton.onClick.RemoveListener(HandleDiscardClicked);
        }
        if (jokerViewBackButton != null)
        {
            jokerViewBackButton.onClick.RemoveListener(HandleJokerViewBackButtonClicked);
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        if (startScreenUI != null)
        {
            UnityEngine.EventSystems.EventTrigger trigger = startScreenUI.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger != null)
            {
                trigger.triggers.Clear();
            }
        }
    }

    public void HandlePlayHandClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (handManager == null)
        {
            return;
        }

        handManager.PlayHand();
        UpdateDiscardStatus(handManager.GetRemainingDiscards());
    }

    public void HandleDiscardClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (handManager == null)
        {
            return;
        }

        handManager.DiscardSelectedAndDraw();
    }

    public void HandleRestartClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (handManager == null)
        {
            return;
        }

        HideGameOver();
        HideJokerSelectionUI();
        ShowNormalGameUI();

        if (deckInfoText != null)
        {
            deckInfoText.gameObject.SetActive(false);
        }

        handManager.StartNewRun();
    }

    public void HandleDeckButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (deckInfoText != null)
        {
            bool currentlyActive = deckInfoText.gameObject.activeSelf;
            deckInfoText.gameObject.SetActive(!currentlyActive);
            
            // Update deck info when opening the panel
            if (!currentlyActive)
            {
                UpdateDeckInfo();
            }
        }
    }

    public void UpdateDeckInfo()
    {
        DeckManager deckManager = FindFirstObjectByType<DeckManager>();
        if (deckManager == null)
        {
            return;
        }

        Dictionary<CardData.Suit, int> suitCounts = deckManager.GetSuitCounts();
        
        int glitchCount = suitCounts.ContainsKey(CardData.Suit.Glitch) ? suitCounts[CardData.Suit.Glitch] : 0;
        int coreCount = suitCounts.ContainsKey(CardData.Suit.Core) ? suitCounts[CardData.Suit.Core] : 0;
        int neuralCount = suitCounts.ContainsKey(CardData.Suit.Neural) ? suitCounts[CardData.Suit.Neural] : 0;
        int protocolCount = suitCounts.ContainsKey(CardData.Suit.Protocol) ? suitCounts[CardData.Suit.Protocol] : 0;

        if (deckInfoText != null)
        {
            deckInfoText.text = $"<color=#00ffff>glitch: {glitchCount}</color>\n<color=#ff00cc>core: {coreCount}</color>\n<color=#fa9c1c>neural: {neuralCount}</color>\n<color=#8a00c4>protocol: {protocolCount}</color>";
        }
    }


    public void HandleJokerCountButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }

        if (handManager != null && handManager.GetJokerCount() > 0)
        {
            ShowJokerView();
        }
    }

    public void ShowJokerView()
    {
        HideNormalGameUI();
        HideJokerSelectionUI();

        if (handManager != null)
        {
            handManager.HideHandCards();
        }

        if (jokerViewUI != null)
        {
            jokerViewUI.SetActive(true);
        }

        DisplayJokerCards();
    }

    public void HideJokerView()
    {
        HideJokerViewPanel();
        ShowNormalGameUI();

        if (handManager != null)
        {
            handManager.ShowHandCards();
        }
    }

    // Hides only the joker view itself, without bringing the game HUD back (used by the start screen).
    private void HideJokerViewPanel()
    {
        if (jokerViewUI != null)
        {
            jokerViewUI.SetActive(false);
        }

        ClearJokerViewCards();

        if (jokerTooltip != null)
        {
            jokerTooltip.HideTooltip();
        }
    }

    public void HandleJokerViewBackButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        HideJokerView();
    }

    private void DisplayJokerCards()
    {
        ClearJokerViewCards();

        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }

        if (handManager == null)
        {
            return;
        }

        List<CardData> selectedJokers = handManager.GetSelectedJokers();

        if (selectedJokers == null || selectedJokers.Count == 0)
        {
            return;
        }

        if (jokerViewSpawnPoints == null || jokerViewSpawnPoints.Length == 0)
        {
            return;
        }

        Canvas canvas = GetCanvas();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        int cardCount = selectedJokers.Count;
        float cardWidth = 200f;
        float cardSpacing = 250f;
        float totalWidth = (cardCount - 1) * cardSpacing + cardWidth;
        float startX = -totalWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < cardCount && i < jokerViewSpawnPoints.Length; i++)
        {
            if (selectedJokers[i] == null || !selectedJokers[i].IsJoker)
            {
                continue;
            }

            if (jokerViewSpawnPoints[i] == null)
            {
                continue;
            }

            RectTransform spawnRect = jokerViewSpawnPoints[i] as RectTransform;
            if (spawnRect == null)
            {
                spawnRect = jokerViewSpawnPoints[i].GetComponent<RectTransform>();
                if (spawnRect == null)
                {
                    continue;
                }
            }

            spawnRect.anchoredPosition = new Vector2(startX + i * cardSpacing, 0f);
            spawnRect.anchorMin = new Vector2(0.5f, 0.5f);
            spawnRect.anchorMax = new Vector2(0.5f, 0.5f);
            spawnRect.pivot = new Vector2(0.5f, 0.5f);

            CreateJokerViewCard(selectedJokers[i], i);
        }
    }

    private void CreateJokerViewCard(CardData jokerData, int index)
    {
        if (jokerData == null)
        {
            return;
        }

        if (jokerViewSpawnPoints == null || index >= jokerViewSpawnPoints.Length || jokerViewSpawnPoints[index] == null)
        {
            return;
        }

        Canvas canvas = GetCanvas();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        GameObject jokerPanel = new GameObject($"JokerViewCard_{index}");
        jokerPanel.transform.SetParent(jokerViewSpawnPoints[index], false);
        
        RectTransform rectTransform = jokerPanel.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(200f, 300f);
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

        AddTooltipToJokerCard(jokerPanel, jokerData);

        jokerViewUIElements.Add(jokerPanel);
    }

    private void ClearJokerViewCards()
    {
        foreach (GameObject jokerUI in jokerViewUIElements)
        {
            if (jokerUI != null)
            {
                Destroy(jokerUI);
            }
        }

        jokerViewUIElements.Clear();
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

    private Coroutine statusCoroutine;
    
    public void SetStatus(string message, bool restoreDiscardStatus = true)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }
        
        if (restoreDiscardStatus)
        {
            statusCoroutine = StartCoroutine(ShowDiscardStatusAfterDelay(2f));
        }
    }
    
    private System.Collections.IEnumerator ShowDiscardStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (handManager != null)
        {
            UpdateDiscardStatus(handManager.GetRemainingDiscards());
        }
    }
    
    public void UpdateDiscardStatus(int remainingDiscards)
    {
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
            statusCoroutine = null;
        }
        
        if (statusText != null)
        {
            int maxDiscards = handManager != null ? handManager.MaxDiscards : remainingDiscards;
            statusText.text = $"Discard: {remainingDiscards}/{maxDiscards}";
        }

        if (discardButton != null)
        {
            bool shouldBeInteractable = remainingDiscards > 0 && !isGameOverShown;
            discardButton.interactable = shouldBeInteractable;
        }
    }

    public void UpdateScore(int chips, float multiplier)
    {
        if (chipsText != null)
        {
            EnsureTextActive(chipsText);
            chipsText.text = $"Chips: {chips}";
        }

        if (multiplierText != null)
        {
            EnsureTextActive(multiplierText);
            multiplierText.text = $"x{multiplier:F1}";
        }
    }
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            EnsureTextActive(scoreText);

            int requiredScore = handManager != null ? handManager.GetCurrentScoreThreshold() : 0;
            scoreText.text = $"Score: {score}/{requiredScore}";
        }
    }

    public void UpdateDeckCount(int remaining, int total)
    {
        if (deckCountText != null)
        {
            EnsureTextActive(deckCountText);
            deckCountText.text = $"Deck: {remaining}/{total}";
        }
    }

    public void UpdateJokerCount(int count)
    {
        if (jokerCountText != null)
        {
            EnsureTextActive(jokerCountText);
            jokerCountText.text = $"Joker: {count}";
        }
    }

    public void UpdateHandCount(int remaining, int max)
    {
        if (handCountText != null)
        {
            EnsureTextActive(handCountText);
            handCountText.text = $"Hands: {remaining}/{max}";
        }
    }

    public void ShowHandType(PokerHandType handType)
    {
        if (handTypeText != null)
        {
            handTypeText.gameObject.SetActive(true);
            string handTypeName = GetHandTypeName(handType);
            handTypeText.text = $"Hand: {handTypeName}";
        }
    }

    public void HideHandType()
    {
        if (handTypeText != null)
        {
            handTypeText.gameObject.SetActive(false);
        }
    }
    
    private bool isGameOverShown = false;

    // The Thaleah font atlas only contains ASCII glyphs, so these texts must stay ASCII.
    public void ShowGameOver(bool won, int finalScore, int level)
    {
        isGameOverShown = true;

        if (playHandButton != null)
        {
            playHandButton.interactable = false;
        }

        if (discardButton != null)
        {
            discardButton.interactable = false;
        }

        if (deckInfoText != null)
        {
            deckInfoText.gameObject.SetActive(false);
        }

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = won ? "You Win!" : "Game Over";
        }

        if (gameOverDetailText != null)
        {
            int requiredScore = handManager != null ? handManager.GetCurrentScoreThreshold() : 0;
            gameOverDetailText.text = won
                ? $"All {level} levels cleared\nScore: {finalScore}/{requiredScore}"
                : $"Level {level}\nScore: {finalScore}/{requiredScore}";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.SetActive(true);
        }
    }

    public void HideGameOver()
    {
        isGameOverShown = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playHandButton != null)
        {
            playHandButton.interactable = true;
        }

        if (discardButton != null)
        {
            discardButton.interactable = handManager == null || handManager.GetRemainingDiscards() > 0;
        }
    }

    public void ShowJokerSelection()
    {
        HideNormalGameUI();
        ShowJokerSelectionUI();
        
        if (deckInfoText != null)
        {
            deckInfoText.gameObject.SetActive(false);
        }
        
        if (handManager != null)
        {
            handManager.HideHandCards();
        }
        
        if (jokerSelectionManager == null)
        {
            jokerSelectionManager = FindFirstObjectByType<JokerSelectionManager>();
        }
        
        if (jokerSelectionManager != null)
        {
            jokerSelectionManager.ShowJokers();
        }
    }

    public void HideJokerSelection()
    {
        HideJokerSelectionUI();
        ShowNormalGameUI();
        
        HideHandType();
        
        if (handManager != null)
        {
            handManager.ShowHandCards();
        }

        HideGameOver();
    }

    private void ShowNormalGameUI()
    {
        if (normalGameUI != null)
        {
            normalGameUI.SetActive(true);
        }
        
        SetNormalGameUIVisible(true);
        
        if (playHandButton != null)
        {
            playHandButton.gameObject.SetActive(true);
        }
        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(true);
        }
    }

    private void HideNormalGameUI()
    {
        if (normalGameUI != null)
        {
            normalGameUI.SetActive(false);
        }
        SetNormalGameUIVisible(false);
    }

    private void ShowJokerSelectionUI()
    {
        if (jokerSelectionUI != null)
        {
            jokerSelectionUI.SetActive(true);
        }
    }

    private void HideJokerSelectionUI()
    {
        if (jokerSelectionUI != null)
        {
            jokerSelectionUI.SetActive(false);
        }
    }

    private void SetNormalGameUIVisible(bool visible)
    {
        if (playHandButton != null)
        {
            playHandButton.gameObject.SetActive(visible);
        }
        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(visible);
        }
        if (statusText != null)
        {
            statusText.gameObject.SetActive(visible);
        }
        if (discardBackground != null)
        {
            discardBackground.gameObject.SetActive(visible);
        }
        if (chipsText != null)
        {
            chipsText.gameObject.SetActive(visible);
        }
        if (chipsBackground != null)
        {
            chipsBackground.gameObject.SetActive(visible);
        }
        if (multiplierText != null)
        {
            multiplierText.gameObject.SetActive(visible);
        }
        if (multiplierBackground != null)
        {
            multiplierBackground.gameObject.SetActive(visible);
        }
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(visible);
        }
        if (scoreBackground != null)
        {
            scoreBackground.gameObject.SetActive(visible);
        }
        if (handTypeText != null)
        {
            handTypeText.gameObject.SetActive(visible);
        }
        if (handTypeBackground != null)
        {
            handTypeBackground.gameObject.SetActive(visible);
        }
        if (deckCountText != null)
        {
            deckCountText.gameObject.SetActive(visible);
        }
        if (deckCountBackground != null)
        {
            deckCountBackground.gameObject.SetActive(visible);
        }
        if (jokerCountText != null)
        {
            jokerCountText.gameObject.SetActive(visible);
        }
        if (jokerCountBackground != null)
        {
            jokerCountBackground.gameObject.SetActive(visible);
        }
        if (handCountText != null)
        {
            handCountText.gameObject.SetActive(visible);
        }
        if (handCountBackground != null)
        {
            handCountBackground.gameObject.SetActive(visible);
        }
    }

    private string GetHandTypeName(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.HighCard: return "High Card";
            case PokerHandType.Pair: return "Pair";
            case PokerHandType.TwoPair: return "Two Pair";
            case PokerHandType.ThreeOfAKind: return "Three of a Kind";
            case PokerHandType.Straight: return "Straight";
            case PokerHandType.Flush: return "Flush";
            case PokerHandType.FullHouse: return "Full House";
            case PokerHandType.FourOfAKind: return "Four of a Kind";
            case PokerHandType.StraightFlush: return "Straight Flush";
            case PokerHandType.RoyalFlush: return "Royal Flush";
            case PokerHandType.FiveOfAKind: return "Five of a Kind";
            case PokerHandType.FlushHouse: return "Flush House";
            case PokerHandType.FlushFive: return "Flush Five";
            default: return "None";
        }
    }

    private void CreateBackgroundPanelsIfNeeded()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        if (backgroundPanelManager == null)
        {
            TryFindBackgroundPanelManager();
        }
        
        if (backgroundPanelManager == null)
        {
            return;
        }
        
        var backgroundData = new TextBackgroundData[]
        {
            new TextBackgroundData(chipsText, "ChipsBackground"),
            new TextBackgroundData(multiplierText, "MultiplierBackground"),
            new TextBackgroundData(scoreText, "ScoreBackground"),
            new TextBackgroundData(statusText, "DiscardBackground"),
            new TextBackgroundData(handTypeText, "HandTypeBackground"),
            new TextBackgroundData(deckCountText, "DeckCountBackground"),
            new TextBackgroundData(jokerCountText, "JokerCountBackground"),
            new TextBackgroundData(handCountText, "HandCountBackground")
        };
        
        var backgroundRefs = new System.Collections.Generic.Dictionary<string, System.Action<Image>>
        {
            { "ChipsBackground", (img) => chipsBackground = img },
            { "MultiplierBackground", (img) => multiplierBackground = img },
            { "ScoreBackground", (img) => scoreBackground = img },
            { "DiscardBackground", (img) => discardBackground = img },
            { "HandTypeBackground", (img) => handTypeBackground = img },
            { "DeckCountBackground", (img) => deckCountBackground = img },
            { "JokerCountBackground", (img) => jokerCountBackground = img },
            { "HandCountBackground", (img) => handCountBackground = img }
        };
        
        backgroundPanelManager.CreateBackgroundPanelsIfNeeded(backgroundData, backgroundRefs);
    }
    
    private void ShowStartScreen()
    {
        if (normalGameUI != null)
        {
            normalGameUI.SetActive(false);
        }
        
        SetNormalGameUIVisible(false);
        HideAllBackgrounds();
        HideJokerSelectionUI();
        HideJokerViewPanel();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (playHandButton != null)
        {
            playHandButton.gameObject.SetActive(false);
        }
        if (discardButton != null)
        {
            discardButton.gameObject.SetActive(false);
        }
        
        if (handManager != null)
        {
            handManager.HideHandCards();
        }
        
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(true);
        }
        
        if (clickToPlayText != null)
        {
            clickToPlayText.gameObject.SetActive(true);
            clickToPlayText.text = "Click to Play";
        }
    }
    
    private void HideAllBackgrounds()
    {
        string[] backgroundNames = new string[]
        {
            "ChipsBackground",
            "MultiplierBackground",
            "ScoreBackground",
            "DiscardBackground",
            "HandTypeBackground",
            "DeckCountBackground",
            "JokerCountBackground",
            "HandCountBackground"
        };
        
        Canvas canvas = GetCanvas();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child != null && child.gameObject != null)
                {
                    foreach (string bgName in backgroundNames)
                    {
                        if (child.name == bgName)
                        {
                            child.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }
        
        if (chipsBackground != null && chipsBackground.gameObject != null)
        {
            chipsBackground.gameObject.SetActive(false);
        }
        if (multiplierBackground != null && multiplierBackground.gameObject != null)
        {
            multiplierBackground.gameObject.SetActive(false);
        }
        if (scoreBackground != null && scoreBackground.gameObject != null)
        {
            scoreBackground.gameObject.SetActive(false);
        }
        if (discardBackground != null && discardBackground.gameObject != null)
        {
            discardBackground.gameObject.SetActive(false);
        }
        if (handTypeBackground != null && handTypeBackground.gameObject != null)
        {
            handTypeBackground.gameObject.SetActive(false);
        }
        if (deckCountBackground != null && deckCountBackground.gameObject != null)
        {
            deckCountBackground.gameObject.SetActive(false);
        }
        if (jokerCountBackground != null && jokerCountBackground.gameObject != null)
        {
            jokerCountBackground.gameObject.SetActive(false);
        }
        if (handCountBackground != null && handCountBackground.gameObject != null)
        {
            handCountBackground.gameObject.SetActive(false);
        }
    }
    
    private void HideStartScreen()
    {
        if (startScreenUI != null)
        {
            startScreenUI.SetActive(false);
        }
        
        ShowNormalGameUI();
    }
    
    private void SetupStartScreenClick()
    {
        if (startScreenUI != null)
        {
            RectTransform rectTransform = startScreenUI.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = startScreenUI.AddComponent<RectTransform>();
            }
            
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            Image backgroundImage = startScreenUI.GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = startScreenUI.AddComponent<Image>();
                backgroundImage.color = new Color(0f, 0f, 0f, 0f);
            }
            backgroundImage.raycastTarget = true;
            
            UnityEngine.EventSystems.EventTrigger trigger = startScreenUI.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = startScreenUI.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            trigger.triggers.Clear();
            
            UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { HandleStartScreenClicked(); });
            trigger.triggers.Add(entry);
        }
    }
    
    private void HandleStartScreenClicked()
    {
        HideStartScreen();
        
        StartCoroutine(StartGameAfterScreenHide());
    }
    
    private System.Collections.IEnumerator StartGameAfterScreenHide()
    {
        yield return null;

        // The run (and its initial hand) is started from one place only.
        if (handManager != null)
        {
            handManager.StartNewRun();
        }

        StartCoroutine(CreateBackgroundPanelsDelayed());
        StartCoroutine(InitializeScoreUIDelayed());
        
        EnsureTextsVisible();
    }
    
    public bool IsStartScreenActive()
    {
        if (startScreenUI != null)
        {
            return startScreenUI.activeSelf;
        }
        return false;
    }
    
}


