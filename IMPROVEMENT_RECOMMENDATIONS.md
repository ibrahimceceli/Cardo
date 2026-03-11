# Cardo Proje İyileştirme Önerileri

Bu belge, Cardo projesi için üç ana iyileştirme alanında detaylı öneriler sunmaktadır:
1. Dependency Injection (Bağımlılık Enjeksiyonu)
2. Event System (Olay Sistemi)
3. Config System (Yapılandırma Sistemi)

---

## 1. Dependency Injection (Bağımlılık Enjeksiyonu)

### Mevcut Durum

Projede şu anda `FindFirstObjectByType<T>()` kullanımı yaygındır ve Rule 2 ihlali oluşturmaktadır:

- **DeckManager**: `HandManager` ve `UIManager` için FindFirstObjectByType kullanıyor
- **HandManager**: `DeckManager` ve `UIManager` için FindFirstObjectByType kullanıyor
- **UIManager**: `HandManager` için FindFirstObjectByType kullanıyor

### Sorunlar

1. **Tight Coupling**: Concrete class'lara doğrudan bağımlılık
2. **Test Edilebilirlik**: Unit test yazımı zorlaşıyor
3. **Performans**: FindFirstObjectByType her çağrıda scene'i tarar
4. **Bağımlılık Sırası**: Awake/Start sıralamasına bağımlılık

### Önerilen Çözüm: Interface Injection + SerializeField

#### Adım 1: Interface'leri Kullan

Mevcut interface'ler zaten tanımlı:
- `IDeckManager`
- `IHandManager`
- `IUIManager`

#### Adım 2: DeckManager.cs İyileştirmesi

**Önce:**
```csharp
public class DeckManager : MonoBehaviour
{
    private HandManager handManager;
    private UIManager uiManager;
    
    void Awake()
    {
        handManager = FindFirstObjectByType<HandManager>();
        uiManager = FindFirstObjectByType<UIManager>();
    }
}
```

**Sonra:**
```csharp
public class DeckManager : MonoBehaviour, IDeckManager
{
    [Header("Dependencies")]
    [SerializeField] private IHandManager handManager;
    [SerializeField] private IUIManager uiManager;
    
    private ICardLayoutCalculator layoutCalculator;
    
    void Awake()
    {
        // Interface injection - Unity Inspector'dan atanır
        // Fallback olarak sadece gerekirse FindFirstObjectByType kullan
        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
            if (handManager == null)
            {
                Debug.LogError("DeckManager: IHandManager dependency not found!");
            }
        }
        
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("DeckManager: IUIManager dependency not found!");
            }
        }
        
        layoutCalculator = new CardLayoutCalculator();
    }
    
    // NotifyStatus metodunu güncelle
    private void NotifyStatus(string message)
    {
        uiManager?.SetStatus(message);
    }
}
```

**Not**: Unity Inspector'da interface field'ları doğrudan gösterilmez. Bu durumda iki seçenek var:

**Seçenek A: Object Reference (Önerilen)**
```csharp
[Header("Dependencies")]
[SerializeField] private MonoBehaviour handManagerObject;
[SerializeField] private MonoBehaviour uiManagerObject;
    
private IHandManager handManager;
private IUIManager uiManager;

void Awake()
{
    // Interface'e cast et
    handManager = handManagerObject as IHandManager;
    uiManager = uiManagerObject as IUIManager;
    
    // Validation
    if (handManager == null)
    {
        Debug.LogError("DeckManager: handManagerObject must implement IHandManager!");
    }
    
    if (uiManager == null)
    {
        Debug.LogError("DeckManager: uiManagerObject must implement IUIManager!");
    }
}
```

**Seçenek B: Custom Property Drawer (Gelişmiş)**
Unity Editor script'i ile interface field'ları için özel property drawer oluşturulabilir.

#### Adım 3: HandManager.cs İyileştirmesi

**Önce:**
```csharp
public class HandManager : MonoBehaviour
{
    public DeckManager deckManager;
    private UIManager uiManager;
    
    void Awake()
    {
        if (deckManager == null)
        {
            deckManager = FindFirstObjectByType<DeckManager>();
        }
        uiManager = FindFirstObjectByType<UIManager>();
    }
}
```

**Sonra:**
```csharp
public class HandManager : MonoBehaviour, IHandManager
{
    [Header("Dependencies")]
    [SerializeField] private MonoBehaviour deckManagerObject;
    [SerializeField] private MonoBehaviour uiManagerObject;
    
    private IDeckManager deckManager;
    private IUIManager uiManager;
    
    private PokerHandEvaluator handEvaluator;
    private HandScoringService scoringService;
    
    void Awake()
    {
        // Interface injection
        deckManager = deckManagerObject as IDeckManager;
        uiManager = uiManagerObject as IUIManager;
        
        // Fallback
        if (deckManager == null && deckManagerObject == null)
        {
            deckManager = FindFirstObjectByType<DeckManager>();
        }
        
        if (uiManager == null && uiManagerObject == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }
        
        // Validation
        if (deckManager == null)
        {
            Debug.LogError("HandManager: IDeckManager dependency not found!");
        }
        
        if (uiManager == null)
        {
            Debug.LogError("HandManager: IUIManager dependency not found!");
        }
        
        handEvaluator = new PokerHandEvaluator();
        scoringService = new HandScoringService();
    }
    
    // Tüm deckManager kullanımlarını güncelle
    public bool PlayHand()
    {
        // ...
        for (int i = 0; i < transformData.positions.Count; i++)
        {
            CardObject newCard = deckManager.DrawCardAtPosition(
                transformData.positions[i], 
                transformData.rotations[i], 
                transformData.sortingOrders[i]
            );
        }
        // ...
    }
}
```

#### Adım 4: UIManager.cs İyileştirmesi

**Önce:**
```csharp
public class UIManager : MonoBehaviour
{
    public HandManager handManager;
    
    void Awake()
    {
        if (handManager == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }
    }
}
```

**Sonra:**
```csharp
public class UIManager : MonoBehaviour, IUIManager
{
    [Header("Dependencies")]
    [SerializeField] private MonoBehaviour handManagerObject;
    
    private IHandManager handManager;
    
    void Awake()
    {
        handManager = handManagerObject as IHandManager;
        
        if (handManager == null && handManagerObject == null)
        {
            handManager = FindFirstObjectByType<HandManager>();
        }
        
        if (handManager == null)
        {
            Debug.LogError("UIManager: IHandManager dependency not found!");
        }
        
        EnsureBackgroundPanelManager();
    }
    
    // Tüm handManager kullanımlarını güncelle
    public void HandlePlayHandClicked()
    {
        if (handManager == null) return;
        
        handManager.PlayHand();
        UpdateDiscardStatus(handManager.GetRemainingDiscards());
    }
}
```

### Avantajlar

1. ✅ **Loose Coupling**: Concrete class'lara değil interface'lere bağımlılık
2. ✅ **Test Edilebilirlik**: Mock interface'ler ile kolay test
3. ✅ **Inspector Kontrolü**: Unity Editor'dan bağımlılıkları görsel olarak atama
4. ✅ **Performans**: FindFirstObjectByType sadece fallback olarak kullanılır
5. ✅ **Hata Kontrolü**: Null check'ler ve error logging

### Uygulama Önceliği

1. **Yüksek Öncelik**: DeckManager, HandManager, UIManager için interface injection
2. **Orta Öncelik**: Custom property drawer oluşturma (opsiyonel)
3. **Düşük Öncelik**: Dependency injection container kullanımı (Zenject, VContainer gibi - eğer proje büyürse)

---

## 2. Event System (Olay Sistemi)

### Mevcut Durum

Şu anda manager'lar arasında doğrudan method çağrıları yapılıyor:

- `HandManager` → `UIManager.UpdateScore()` doğrudan çağırıyor
- `DeckManager` → `UIManager.SetStatus()` doğrudan çağırıyor
- `CardObject` → `HandManager.UpdateSelectedHandType()` doğrudan çağırıyor

### Sorunlar

1. **Tight Coupling**: Manager'lar birbirini doğrudan bilmek zorunda
2. **Çoklu Dinleyici**: Aynı olayı birden fazla sistem dinleyemez
3. **Debug Zorluğu**: Olay akışını takip etmek zor
4. **Genişletilebilirlik**: Yeni sistemler eklemek için mevcut kodları değiştirmek gerekir

### Önerilen Çözüm: C# Events + Event Manager Pattern

#### Adım 1: Game Events Tanımlama

Yeni dosya: `Assets/Scripts/Events/GameEvents.cs`

```csharp
using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central event system for game-wide events.
/// Uses C# events for type-safe, decoupled communication.
/// </summary>
public static class GameEvents
{
    // Card Events
    public static event Action<CardObject> OnCardSelected;
    public static event Action<CardObject> OnCardDeselected;
    public static event Action<List<CardObject>> OnCardsSelectedChanged;
    
    // Hand Events
    public static event Action<List<CardObject>> OnHandUpdated;
    public static event Action<PokerHandType> OnHandTypeChanged;
    public static event Action<int, int> OnScoreCalculated; // chips, multiplier
    public static event Action<int> OnTotalScoreChanged;
    public static event Action<bool> OnHandPlayed; // success
    public static event Action OnHandDiscarded;
    
    // Deck Events
    public static event Action OnDeckInitialized;
    public static event Action OnDeckShuffled;
    public static event Action<CardObject> OnCardDrawn;
    public static event Action OnDeckEmpty;
    
    // UI Events
    public static event Action<string> OnStatusMessageChanged;
    public static event Action<int> OnDiscardCountChanged;
    
    // Card Selection Events
    public static event Action<int> OnSelectionCountChanged; // current selected count
    
    // Event Invocation Methods (Public API)
    public static void InvokeCardSelected(CardObject card)
    {
        OnCardSelected?.Invoke(card);
    }
    
    public static void InvokeCardDeselected(CardObject card)
    {
        OnCardDeselected?.Invoke(card);
    }
    
    public static void InvokeCardsSelectedChanged(List<CardObject> selectedCards)
    {
        OnCardsSelectedChanged?.Invoke(selectedCards);
    }
    
    public static void InvokeHandUpdated(List<CardObject> hand)
    {
        OnHandUpdated?.Invoke(hand);
    }
    
    public static void InvokeHandTypeChanged(PokerHandType handType)
    {
        OnHandTypeChanged?.Invoke(handType);
    }
    
    public static void InvokeScoreCalculated(int chips, int multiplier)
    {
        OnScoreCalculated?.Invoke(chips, multiplier);
    }
    
    public static void InvokeTotalScoreChanged(int totalScore)
    {
        OnTotalScoreChanged?.Invoke(totalScore);
    }
    
    public static void InvokeHandPlayed(bool success)
    {
        OnHandPlayed?.Invoke(success);
    }
    
    public static void InvokeHandDiscarded()
    {
        OnHandDiscarded?.Invoke();
    }
    
    public static void InvokeDeckInitialized()
    {
        OnDeckInitialized?.Invoke();
    }
    
    public static void InvokeDeckShuffled()
    {
        OnDeckShuffled?.Invoke();
    }
    
    public static void InvokeCardDrawn(CardObject card)
    {
        OnCardDrawn?.Invoke(card);
    }
    
    public static void InvokeDeckEmpty()
    {
        OnDeckEmpty?.Invoke();
    }
    
    public static void InvokeStatusMessageChanged(string message)
    {
        OnStatusMessageChanged?.Invoke(message);
    }
    
    public static void InvokeDiscardCountChanged(int remainingDiscards)
    {
        OnDiscardCountChanged?.Invoke(remainingDiscards);
    }
    
    public static void InvokeSelectionCountChanged(int count)
    {
        OnSelectionCountChanged?.Invoke(count);
    }
    
    // Cleanup method (call in OnDestroy)
    public static void ClearAllEvents()
    {
        OnCardSelected = null;
        OnCardDeselected = null;
        OnCardsSelectedChanged = null;
        OnHandUpdated = null;
        OnHandTypeChanged = null;
        OnScoreCalculated = null;
        OnTotalScoreChanged = null;
        OnHandPlayed = null;
        OnHandDiscarded = null;
        OnDeckInitialized = null;
        OnDeckShuffled = null;
        OnCardDrawn = null;
        OnDeckEmpty = null;
        OnStatusMessageChanged = null;
        OnDiscardCountChanged = null;
        OnSelectionCountChanged = null;
    }
}
```

#### Adım 2: CardObject.cs Güncelleme

**Önce:**
```csharp
public void DetectClick()
{
    isSelected = !isSelected;
    // ... visual updates ...
    if (handManager != null)
    {
        handManager.UpdateSelectedHandType();
    }
}
```

**Sonra:**
```csharp
public void DetectClick()
{
    isSelected = !isSelected;
    
    // ... visual updates ...
    
    // Event-based communication
    if (isSelected)
    {
        GameEvents.InvokeCardSelected(this);
    }
    else
    {
        GameEvents.InvokeCardDeselected(this);
    }
    
    // Legacy support (optional, can be removed after migration)
    if (handManager != null)
    {
        handManager.UpdateSelectedHandType();
    }
}
```

#### Adım 3: HandManager.cs Güncelleme

**Önce:**
```csharp
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
            int multiplier = scoringService.GetMultiplier(handType);
            uiManager.UpdateScore(baseChipScore, multiplier);
        }
    }
}
```

**Sonra:**
```csharp
void OnEnable()
{
    // Subscribe to events
    GameEvents.OnCardSelected += HandleCardSelected;
    GameEvents.OnCardDeselected += HandleCardDeselected;
    GameEvents.OnCardsSelectedChanged += HandleCardsSelectedChanged;
}

void OnDisable()
{
    // Unsubscribe from events
    GameEvents.OnCardSelected -= HandleCardSelected;
    GameEvents.OnCardDeselected -= HandleCardDeselected;
    GameEvents.OnCardsSelectedChanged -= HandleCardsSelectedChanged;
}

private void HandleCardSelected(CardObject card)
{
    UpdateSelectedHandType();
}

private void HandleCardDeselected(CardObject card)
{
    UpdateSelectedHandType();
}

private void HandleCardsSelectedChanged(List<CardObject> selectedCards)
{
    UpdateSelectedHandType();
}

public void UpdateSelectedHandType()
{
    List<CardObject> selectedCards = GetSelectedCards();
    
    // Emit selection count event
    GameEvents.InvokeSelectionCountChanged(selectedCards.Count);
    
    if (selectedCards.Count == 0 || selectedCards.Count > 5)
    {
        GameEvents.InvokeHandTypeChanged(PokerHandType.None);
        GameEvents.InvokeScoreCalculated(0, 0);
        return;
    }
    
    PokerHandType handType = handEvaluator.DetermineHand(selectedCards);
    GameEvents.InvokeHandTypeChanged(handType);
    
    if (handType != PokerHandType.None)
    {
        int baseChipScore = scoringService.GetBaseScore(handType);
        int multiplier = scoringService.GetMultiplier(handType);
        GameEvents.InvokeScoreCalculated(baseChipScore, multiplier);
    }
    else
    {
        GameEvents.InvokeScoreCalculated(0, 0);
    }
}

public bool PlayHand()
{
    List<CardObject> selectedCards = GetSelectedCards();
    
    if (selectedCards.Count == 0 || selectedCards.Count > 5)
    {
        GameEvents.InvokeHandPlayed(false);
        return false;
    }
    
    PokerHandType handType = handEvaluator.DetermineHand(selectedCards);
    int baseChipScore = scoringService.GetBaseScore(handType);
    int multiplier = scoringService.GetMultiplier(handType);
    
    int cardChipTotal = 0;
    foreach (var card in selectedCards)
    {
        if (card != null && card.cardInfo != null)
        {
            cardChipTotal += card.cardInfo.NumericValue;
        }
    }
    
    int totalChipsForHand = baseChipScore + cardChipTotal;
    int finalScore = totalChipsForHand * multiplier;
    
    totalScore += finalScore;
    totalChips += finalScore;
    totalMultiplier = multiplier;
    
    // Emit events
    GameEvents.InvokeScoreCalculated(totalChipsForHand, multiplier);
    GameEvents.InvokeTotalScoreChanged(totalScore);
    
    CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);
    ResetSelection(selectedCards);
    
    for (int i = 0; i < transformData.positions.Count; i++)
    {
        CardObject newCard = deckManager.DrawCardAtPosition(
            transformData.positions[i], 
            transformData.rotations[i], 
            transformData.sortingOrders[i]
        );
    }
    
    GameEvents.InvokeHandPlayed(true);
    UpdateSelectedHandType();
    
    return true;
}

public void DiscardSelectedAndDraw()
{
    List<CardObject> selectedCards = GetSelectedCards();
    
    if (selectedCards.Count == 0 || selectedCards.Count > 5)
    {
        return;
    }
    
    if (remainingDiscards <= 0)
    {
        return;
    }
    
    remainingDiscards--;
    
    CardTransformData transformData = CardTransformHelper.CollectCardTransforms(selectedCards);
    
    foreach (CardObject card in selectedCards)
    {
        currentHand.Remove(card);
        Destroy(card.gameObject);
    }
    
    for (int i = 0; i < transformData.positions.Count; i++)
    {
        CardObject newCard = deckManager.DrawCardAtPosition(
            transformData.positions[i], 
            transformData.rotations[i], 
            transformData.sortingOrders[i]
        );
    }
    
    GameEvents.InvokeHandDiscarded();
    GameEvents.InvokeDiscardCountChanged(remainingDiscards);
}

public void AddCardToHand(CardObject newCard)
{
    currentHand.Add(newCard);
    GameEvents.InvokeHandUpdated(currentHand);
}
```

#### Adım 4: UIManager.cs Güncelleme

**Önce:**
```csharp
public void UpdateScore(int chips, int multiplier)
{
    if (chipsText != null)
    {
        chipsText.text = $"Chips: {chips}";
    }
    if (multiplierText != null)
    {
        multiplierText.text = $"x{multiplier}";
    }
}
```

**Sonra:**
```csharp
void OnEnable()
{
    // Subscribe to events
    GameEvents.OnHandTypeChanged += HandleHandTypeChanged;
    GameEvents.OnScoreCalculated += HandleScoreCalculated;
    GameEvents.OnTotalScoreChanged += HandleTotalScoreChanged;
    GameEvents.OnStatusMessageChanged += HandleStatusMessageChanged;
    GameEvents.OnDiscardCountChanged += HandleDiscardCountChanged;
    GameEvents.OnHandPlayed += HandleHandPlayed;
    GameEvents.OnHandDiscarded += HandleHandDiscarded;
    
    // Button listeners
    if (playHandButton != null)
    {
        playHandButton.onClick.AddListener(HandlePlayHandClicked);
    }
    if (discardButton != null)
    {
        discardButton.onClick.AddListener(HandleDiscardClicked);
    }
}

void OnDisable()
{
    // Unsubscribe from events
    GameEvents.OnHandTypeChanged -= HandleHandTypeChanged;
    GameEvents.OnScoreCalculated -= HandleScoreCalculated;
    GameEvents.OnTotalScoreChanged -= HandleTotalScoreChanged;
    GameEvents.OnStatusMessageChanged -= HandleStatusMessageChanged;
    GameEvents.OnDiscardCountChanged -= HandleDiscardCountChanged;
    GameEvents.OnHandPlayed -= HandleHandPlayed;
    GameEvents.OnHandDiscarded -= HandleHandDiscarded;
    
    // Button listeners
    if (playHandButton != null)
    {
        playHandButton.onClick.RemoveListener(HandlePlayHandClicked);
    }
    if (discardButton != null)
    {
        discardButton.onClick.RemoveListener(HandleDiscardClicked);
    }
}

private void HandleHandTypeChanged(PokerHandType handType)
{
    if (handType != PokerHandType.None)
    {
        ShowHandType(handType);
    }
    else
    {
        HideHandType();
    }
}

private void HandleScoreCalculated(int chips, int multiplier)
{
    UpdateScore(chips, multiplier);
}

private void HandleTotalScoreChanged(int totalScore)
{
    UpdateScore(totalScore);
}

private void HandleStatusMessageChanged(string message)
{
    SetStatus(message);
}

private void HandleDiscardCountChanged(int remainingDiscards)
{
    UpdateDiscardStatus(remainingDiscards);
}

private void HandleHandPlayed(bool success)
{
    if (success && handManager != null)
    {
        UpdateDiscardStatus(handManager.GetRemainingDiscards());
    }
}

private void HandleHandDiscarded()
{
    if (handManager != null)
    {
        UpdateDiscardStatus(handManager.GetRemainingDiscards());
    }
}

// Public methods remain for direct calls (backward compatibility)
public void UpdateScore(int chips, int multiplier)
{
    if (chipsText != null)
    {
        EnsureTextActive(chipsText);
        chipsText.text = $"Chips: {chips}";
    }
    if (multiplierText != null)
    {
        EnsureTextActive(multiplierText);
        multiplierText.text = $"x{multiplier}";
    }
}

public void UpdateScore(int score)
{
    if (scoreText != null)
    {
        EnsureTextActive(scoreText);
        scoreText.text = $"Score: {score}";
    }
}
```

#### Adım 5: DeckManager.cs Güncelleme

```csharp
public void InitializeDeck()
{
    drawDeck.Clear();
    drawDeck.AddRange(allCardData);
    GameEvents.InvokeDeckInitialized();
}

public void ShuffleDrawDeck()
{
    System.Random rng = new System.Random();
    drawDeck = drawDeck.OrderBy(a => rng.Next()).ToList();
    GameEvents.InvokeDeckShuffled();
}

private CardObject DrawCardInternal(Vector3 spawnPosition, Quaternion spawnRotation, int sortingOrder, bool updateOriginalPosition)
{
    EnsureDeckHasCards();
    
    if (drawDeck.Count == 0)
    {
        GameEvents.InvokeDeckEmpty();
        return null;
    }
    
    CardData drawnCardData = drawDeck[0];
    drawDeck.RemoveAt(0);
    
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
        GameEvents.InvokeCardDrawn(cardObject);
        return cardObject;
    }
    
    return null;
}

private void NotifyStatus(string message)
{
    GameEvents.InvokeStatusMessageChanged(message);
}
```

### Avantajlar

1. ✅ **Loose Coupling**: Manager'lar birbirini bilmek zorunda değil
2. ✅ **Çoklu Dinleyici**: Aynı olayı birden fazla sistem dinleyebilir
3. ✅ **Genişletilebilirlik**: Yeni sistemler eklemek için mevcut kodu değiştirmek gerekmez
4. ✅ **Debug Kolaylığı**: Event flow'u takip etmek kolay
5. ✅ **Test Edilebilirlik**: Event'leri mock'layarak test yazılabilir

### Uygulama Önceliği

1. **Yüksek Öncelik**: GameEvents.cs oluşturma ve temel event'ler
2. **Orta Öncelik**: HandManager ve UIManager event migration
3. **Düşük Öncelik**: Legacy method çağrılarını kaldırma (backward compatibility için bir süre tutulabilir)

---

## 3. Config System (Yapılandırma Sistemi)

### Mevcut Durum

Şu anda birçok değer hardcoded:

- **HandScoringService**: Base score ve multiplier değerleri switch-case içinde
- **CardLayoutCalculator**: ArcRadius, MaxAngle, HorizontalSpacing gibi layout parametreleri const olarak tanımlı
- **HandManager**: remainingDiscards = 4 hardcoded
- **CardObject**: selectionOffset, selectionScale, selectedColor hardcoded

### Sorunlar

1. **Değiştirilebilirlik**: Değerleri değiştirmek için kod değişikliği gerekir
2. **Test Edilebilirlik**: Farklı config'lerle test yazmak zor
3. **Designer Kontrolü**: Designer'lar Unity Editor'dan değerleri değiştiremez
4. **Balance Tuning**: Oyun dengesini ayarlamak için kod bilgisi gerekir

### Önerilen Çözüm: ScriptableObject-Based Config System

#### Adım 1: GameConfig ScriptableObject

Yeni dosya: `Assets/Scripts/Config/GameConfig.cs`

```csharp
using UnityEngine;

/// <summary>
/// Central configuration ScriptableObject for game settings.
/// Create instances via: Right-click → Create → Cardo → Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Cardo/Game Config", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("Hand Settings")]
    [Tooltip("Maximum number of cards that can be selected")]
    public int maxSelectableCards = 5;
    
    [Tooltip("Initial number of cards drawn")]
    public int initialHandSize = 8;
    
    [Tooltip("Maximum number of discards allowed")]
    public int maxDiscards = 4;
    
    [Header("Card Selection Visuals")]
    [Tooltip("Vertical offset when card is selected")]
    public float cardSelectionOffset = 0.5f;
    
    [Tooltip("Scale multiplier when card is selected")]
    public float cardSelectionScale = 1.1f;
    
    [Tooltip("Color tint when card is selected")]
    public Color cardSelectedColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    [Header("Card Layout")]
    [Tooltip("Radius of the arc for card fan layout")]
    public float arcRadius = 1f;
    
    [Tooltip("Maximum angle spread for card fan")]
    public float maxAngle = 10f;
    
    [Tooltip("Horizontal spacing between cards")]
    public float horizontalSpacing = 1f;
    
    [Tooltip("Z offset per card for depth sorting")]
    public float zOffsetPerCard = 0.01f;
    
    [Tooltip("Maximum roll angle for fan effect")]
    public float maxRollAngle = 8f;
    
    [Header("Scoring")]
    [Tooltip("Base scores for each hand type")]
    public HandScoreConfig handScores;
    
    [Tooltip("Multipliers for each hand type")]
    public HandMultiplierConfig handMultipliers;
    
    // Validation
    private void OnValidate()
    {
        maxSelectableCards = Mathf.Max(1, maxSelectableCards);
        initialHandSize = Mathf.Max(1, initialHandSize);
        maxDiscards = Mathf.Max(0, maxDiscards);
        cardSelectionScale = Mathf.Max(1f, cardSelectionScale);
        arcRadius = Mathf.Max(0.1f, arcRadius);
        maxAngle = Mathf.Clamp(maxAngle, 0f, 180f);
        horizontalSpacing = Mathf.Max(0f, horizontalSpacing);
        maxRollAngle = Mathf.Clamp(maxRollAngle, 0f, 90f);
    }
}

/// <summary>
/// Configuration for hand base scores.
/// </summary>
[System.Serializable]
public class HandScoreConfig
{
    public int flushFive = 160;
    public int flushHouse = 140;
    public int fiveOfAKind = 120;
    public int royalFlush = 100;
    public int straightFlush = 100;
    public int fourOfAKind = 60;
    public int fullHouse = 40;
    public int flush = 35;
    public int straight = 30;
    public int threeOfAKind = 30;
    public int twoPair = 20;
    public int pair = 10;
    public int highCard = 5;
    public int none = 1;
    
    public int GetScore(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return flushFive;
            case PokerHandType.FlushHouse: return flushHouse;
            case PokerHandType.FiveOfAKind: return fiveOfAKind;
            case PokerHandType.RoyalFlush: return royalFlush;
            case PokerHandType.StraightFlush: return straightFlush;
            case PokerHandType.FourOfAKind: return fourOfAKind;
            case PokerHandType.FullHouse: return fullHouse;
            case PokerHandType.Flush: return flush;
            case PokerHandType.Straight: return straight;
            case PokerHandType.ThreeOfAKind: return threeOfAKind;
            case PokerHandType.TwoPair: return twoPair;
            case PokerHandType.Pair: return pair;
            case PokerHandType.HighCard: return highCard;
            case PokerHandType.None: return none;
            default: return 0;
        }
    }
}

/// <summary>
/// Configuration for hand multipliers.
/// </summary>
[System.Serializable]
public class HandMultiplierConfig
{
    public int flushFive = 16;
    public int flushHouse = 14;
    public int fiveOfAKind = 12;
    public int royalFlush = 8;
    public int straightFlush = 8;
    public int fourOfAKind = 7;
    public int fullHouse = 4;
    public int flush = 4;
    public int straight = 4;
    public int threeOfAKind = 3;
    public int twoPair = 2;
    public int pair = 2;
    public int highCard = 1;
    public int none = 1;
    
    public int GetMultiplier(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return flushFive;
            case PokerHandType.FlushHouse: return flushHouse;
            case PokerHandType.FiveOfAKind: return fiveOfAKind;
            case PokerHandType.RoyalFlush: return royalFlush;
            case PokerHandType.StraightFlush: return straightFlush;
            case PokerHandType.FourOfAKind: return fourOfAKind;
            case PokerHandType.FullHouse: return fullHouse;
            case PokerHandType.Flush: return flush;
            case PokerHandType.Straight: return straight;
            case PokerHandType.ThreeOfAKind: return threeOfAKind;
            case PokerHandType.TwoPair: return twoPair;
            case PokerHandType.Pair: return pair;
            case PokerHandType.HighCard: return highCard;
            case PokerHandType.None: return none;
            default: return 1;
        }
    }
}
```

#### Adım 2: Config Manager Singleton

Yeni dosya: `Assets/Scripts/Config/ConfigManager.cs`

```csharp
using UnityEngine;

/// <summary>
/// Singleton manager for accessing game configuration.
/// Ensures config is loaded and accessible throughout the game.
/// </summary>
public class ConfigManager : MonoBehaviour
{
    private static ConfigManager instance;
    private static GameConfig config;
    
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    
    public static GameConfig Config
    {
        get
        {
            if (config == null && instance != null)
            {
                config = instance.gameConfig;
            }
            
            if (config == null)
            {
                Debug.LogError("ConfigManager: GameConfig is not assigned! Please assign a GameConfig asset in the ConfigManager component.");
            }
            
            return config;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            config = gameConfig;
            DontDestroyOnLoad(gameObject);
            
            if (config == null)
            {
                Debug.LogError("ConfigManager: GameConfig is not assigned!");
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            config = null;
        }
    }
    
    // Helper method for runtime config access
    public static bool IsConfigLoaded()
    {
        return config != null;
    }
}
```

#### Adım 3: HandScoringService.cs Güncelleme

**Önce:**
```csharp
public class HandScoringService
{
    public int GetBaseScore(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return 160;
            // ... hardcoded values
        }
    }
}
```

**Sonra:**
```csharp
public class HandScoringService
{
    private GameConfig config;
    
    public HandScoringService()
    {
        // Config will be loaded when needed
    }
    
    public HandScoringService(GameConfig config)
    {
        this.config = config;
    }
    
    private GameConfig GetConfig()
    {
        if (config == null)
        {
            config = ConfigManager.Config;
        }
        return config;
    }
    
    public int GetBaseScore(PokerHandType handType)
    {
        GameConfig cfg = GetConfig();
        if (cfg != null && cfg.handScores != null)
        {
            return cfg.handScores.GetScore(handType);
        }
        
        // Fallback to default values if config not available
        return GetDefaultBaseScore(handType);
    }
    
    public int GetMultiplier(PokerHandType handType)
    {
        GameConfig cfg = GetConfig();
        if (cfg != null && cfg.handMultipliers != null)
        {
            return cfg.handMultipliers.GetMultiplier(handType);
        }
        
        // Fallback to default values if config not available
        return GetDefaultMultiplier(handType);
    }
    
    // Fallback methods (can be removed after full migration)
    private int GetDefaultBaseScore(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return 160;
            case PokerHandType.FlushHouse: return 140;
            case PokerHandType.FiveOfAKind: return 120;
            case PokerHandType.RoyalFlush: return 100;
            case PokerHandType.StraightFlush: return 100;
            case PokerHandType.FourOfAKind: return 60;
            case PokerHandType.FullHouse: return 40;
            case PokerHandType.Flush: return 35;
            case PokerHandType.Straight: return 30;
            case PokerHandType.ThreeOfAKind: return 30;
            case PokerHandType.TwoPair: return 20;
            case PokerHandType.Pair: return 10;
            case PokerHandType.HighCard: return 5;
            case PokerHandType.None: return 1;
            default: return 0;
        }
    }
    
    private int GetDefaultMultiplier(PokerHandType handType)
    {
        switch (handType)
        {
            case PokerHandType.FlushFive: return 16;
            case PokerHandType.FlushHouse: return 14;
            case PokerHandType.FiveOfAKind: return 12;
            case PokerHandType.RoyalFlush: return 8;
            case PokerHandType.StraightFlush: return 8;
            case PokerHandType.FourOfAKind: return 7;
            case PokerHandType.FullHouse: return 4;
            case PokerHandType.Flush: return 4;
            case PokerHandType.Straight: return 4;
            case PokerHandType.ThreeOfAKind: return 3;
            case PokerHandType.TwoPair: return 2;
            case PokerHandType.Pair: return 2;
            case PokerHandType.HighCard: return 1;
            case PokerHandType.None: return 1;
            default: return 1;
        }
    }
}
```

#### Adım 4: CardLayoutCalculator.cs Güncelleme

**Önce:**
```csharp
public class CardLayoutCalculator : ICardLayoutCalculator
{
    private const float ArcRadius = 1f;
    private const float MaxAngle = 10f;
    // ... hardcoded constants
}
```

**Sonra:**
```csharp
public class CardLayoutCalculator : ICardLayoutCalculator
{
    private GameConfig config;
    
    // Default values (fallback)
    private const float DefaultArcRadius = 1f;
    private const float DefaultMaxAngle = 10f;
    private const float DefaultHorizontalSpacing = 1f;
    private const float DefaultZOffsetPerCard = 0.01f;
    private const float DefaultMaxRollAngle = 8f;
    
    public CardLayoutCalculator()
    {
        // Config will be loaded when needed
    }
    
    public CardLayoutCalculator(GameConfig config)
    {
        this.config = config;
    }
    
    private GameConfig GetConfig()
    {
        if (config == null)
        {
            config = ConfigManager.Config;
        }
        return config;
    }
    
    private float GetArcRadius()
    {
        GameConfig cfg = GetConfig();
        return cfg != null ? cfg.arcRadius : DefaultArcRadius;
    }
    
    private float GetMaxAngle()
    {
        GameConfig cfg = GetConfig();
        return cfg != null ? cfg.maxAngle : DefaultMaxAngle;
    }
    
    private float GetHorizontalSpacing()
    {
        GameConfig cfg = GetConfig();
        return cfg != null ? cfg.horizontalSpacing : DefaultHorizontalSpacing;
    }
    
    private float GetZOffsetPerCard()
    {
        GameConfig cfg = GetConfig();
        return cfg != null ? cfg.zOffsetPerCard : DefaultZOffsetPerCard;
    }
    
    private float GetMaxRollAngle()
    {
        GameConfig cfg = GetConfig();
        return cfg != null ? cfg.maxRollAngle : DefaultMaxRollAngle;
    }
    
    public CardLayoutInfo CalculateCardLayout(int cardIndex, int totalCards, Vector3 centerPosition, Quaternion baseRotation)
    {
        if (totalCards <= 0)
        {
            return new CardLayoutInfo(centerPosition, baseRotation, 0);
        }
        
        float arcRadius = GetArcRadius();
        float maxAngle = GetMaxAngle();
        float horizontalSpacing = GetHorizontalSpacing();
        float zOffsetPerCard = GetZOffsetPerCard();
        float maxRollAngle = GetMaxRollAngle();
        
        // Calculate center index for symmetric layout
        float centerIndex = (totalCards - 1) / 2f;
        
        // Calculate angle step based on total cards
        float angleStep = totalCards > 1 ? maxAngle / (totalCards - 1) : 0f;
        
        // Calculate angle for this card
        float angle = (cardIndex - centerIndex) * angleStep;
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate position using arc math
        float x = centerPosition.x + Mathf.Sin(angleRad) * arcRadius + (cardIndex - centerIndex) * horizontalSpacing;
        float y = centerPosition.y + Mathf.Cos(angleRad) * arcRadius;
        float z = centerPosition.z + (cardIndex * zOffsetPerCard);
        
        Vector3 position = new Vector3(x, y, z);
        
        // Calculate rotation angles
        float rotationAngle = angle;
        float rollAngle = 0f;
        
        // Calculate roll angle for fan effect
        if (centerIndex > 0)
        {
            rollAngle = -(cardIndex - centerIndex) / centerIndex * maxRollAngle;
        }
        
        // Compose rotations: base * fan * roll
        Quaternion fanRotation = Quaternion.Euler(0, rotationAngle, 0);
        Quaternion rollRotation = Quaternion.Euler(0, 0, rollAngle);
        Quaternion finalRotation = baseRotation * fanRotation * rollRotation;
        
        // Sorting order is the card index
        int sortingOrder = cardIndex;
        
        return new CardLayoutInfo(position, finalRotation, sortingOrder);
    }
    
    public CardLayoutAngles CalculateCardAngles(int cardIndex, int totalCards)
    {
        if (totalCards <= 0)
        {
            return new CardLayoutAngles(0f, 0f);
        }
        
        float maxAngle = GetMaxAngle();
        float maxRollAngle = GetMaxRollAngle();
        
        float centerIndex = (totalCards - 1) / 2f;
        float angleStep = totalCards > 1 ? maxAngle / (totalCards - 1) : 0f;
        
        float angle = (cardIndex - centerIndex) * angleStep;
        float rotationAngle = angle;
        
        float rollAngle = 0f;
        if (centerIndex > 0)
        {
            rollAngle = -(cardIndex - centerIndex) / centerIndex * maxRollAngle;
        }
        
        return new CardLayoutAngles(rotationAngle, rollAngle);
    }
}
```

#### Adım 5: HandManager.cs Güncelleme

```csharp
public class HandManager : MonoBehaviour, IHandManager
{
    private int remainingDiscards;
    
    void Awake()
    {
        // ... dependency injection code ...
        
        // Load config values
        GameConfig config = ConfigManager.Config;
        if (config != null)
        {
            remainingDiscards = config.maxDiscards;
        }
        else
        {
            remainingDiscards = 4; // Fallback
        }
        
        handEvaluator = new PokerHandEvaluator();
        scoringService = new HandScoringService(config);
    }
    
    public bool CanSelectCard(CardObject card)
    {
        if (card != null && card.isSelected)
        {
            return true;
        }
        
        int selectedCount = GetSelectedCards().Count;
        GameConfig config = ConfigManager.Config;
        int maxSelectable = config != null ? config.maxSelectableCards : 5;
        return selectedCount < maxSelectable;
    }
}
```

#### Adım 6: CardObject.cs Güncelleme

```csharp
public class CardObject : MonoBehaviour
{
    private float selectionOffset;
    private float selectionScale;
    private Color selectedColor;
    
    void Start()
    {
        // Load config values
        GameConfig config = ConfigManager.Config;
        if (config != null)
        {
            selectionOffset = config.cardSelectionOffset;
            selectionScale = config.cardSelectionScale;
            selectedColor = config.cardSelectedColor;
        }
        else
        {
            // Fallback values
            selectionOffset = 0.5f;
            selectionScale = 1.1f;
            selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
        }
        
        originalPosition = transform.position;
        originalScale = transform.localScale;
        
        // ... rest of Start() ...
    }
}
```

#### Adım 7: DeckManager.cs Güncelleme

```csharp
public void DrawInitialHand(int count)
{
    // Use config value if available
    GameConfig config = ConfigManager.Config;
    int handSize = config != null ? config.initialHandSize : count;
    
    Quaternion baseRotation = cardPrefab.transform.rotation;
    
    for (int i = 0; i < handSize; i++)
    {
        CardLayoutInfo layoutInfo = layoutCalculator.CalculateCardLayout(i, handSize, handSpawnPoint.position, baseRotation);
        CardLayoutAngles angles = ((CardLayoutCalculator)layoutCalculator).CalculateCardAngles(i, handSize);
        
        DrawCard(layoutInfo.position, angles.rotationAngle, angles.rollAngle, layoutInfo.sortingOrder);
    }
}
```

### Kullanım: Unity Editor'da Config Oluşturma

1. **GameConfig Asset Oluşturma**:
   - Unity Editor'da: Right-click → Create → Cardo → Game Config
   - Dosya adı: `DefaultGameConfig.asset`
   - Konum: `Assets/Config/` (klasör oluştur)

2. **ConfigManager Setup**:
   - Scene'de boş GameObject oluştur: "ConfigManager"
   - `ConfigManager` component ekle
   - `Game Config` field'ına `DefaultGameConfig.asset`'i sürükle

3. **Değerleri Ayarlama**:
   - `DefaultGameConfig.asset`'i seç
   - Inspector'da tüm değerleri ayarla
   - Değişiklikler runtime'da otomatik yansır (ScriptableObject)

### Avantajlar

1. ✅ **Designer Kontrolü**: Unity Editor'dan tüm değerler değiştirilebilir
2. ✅ **Kod Değişikliği Yok**: Balance tuning için kod bilgisi gerekmez
3. ✅ **Çoklu Config**: Farklı zorluk seviyeleri için farklı config'ler
4. ✅ **Test Edilebilirlik**: Farklı config'lerle test yazılabilir
5. ✅ **Version Control**: Config dosyaları Git'te takip edilebilir

### Uygulama Önceliği

1. **Yüksek Öncelik**: GameConfig ScriptableObject ve ConfigManager
2. **Orta Öncelik**: HandScoringService ve CardLayoutCalculator migration
3. **Düşük Öncelik**: Diğer hardcoded değerlerin migration'ı

---

## Uygulama Sırası ve Öncelikler

### Faz 1: Dependency Injection (1-2 gün)
1. DeckManager interface injection
2. HandManager interface injection
3. UIManager interface injection
4. Unity Inspector'da bağımlılıkları atama

### Faz 2: Event System (2-3 gün)
1. GameEvents.cs oluşturma
2. HandManager event migration
3. UIManager event migration
4. DeckManager event migration
5. CardObject event migration
6. Legacy method çağrılarını kaldırma (opsiyonel)

### Faz 3: Config System (1-2 gün)
1. GameConfig ScriptableObject oluşturma
2. ConfigManager singleton
3. HandScoringService migration
4. CardLayoutCalculator migration
5. HandManager ve CardObject config kullanımı
6. Default config asset oluşturma

### Toplam Tahmini Süre: 4-7 gün

---

## Sonuç

Bu üç iyileştirme, Cardo projesinin kod kalitesini, bakım kolaylığını ve genişletilebilirliğini önemli ölçüde artıracaktır. Her bir iyileştirme bağımsız olarak uygulanabilir, ancak birlikte kullanıldığında güçlü bir mimari oluşturur.

**Önerilen Uygulama Sırası**: Dependency Injection → Event System → Config System

Bu sıra, her adımda bir önceki adımın faydalarını kullanarak ilerlemeyi sağlar.
