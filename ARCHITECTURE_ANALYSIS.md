# Cardo Proje - Mimari Analiz Raporu

## Genel Bakış

**Cardo**, Unity tabanlı bir poker kart oyunu projesidir. Proje, katmanlı mimari prensiplerine uygun olarak organize edilmiş ve SOLID prensiplerini uygulamaya çalışmaktadır. Bu dokümantasyon, projenin mevcut mimari yapısını detaylı olarak analiz etmektedir.

---

## Mimari Katmanlar

### 1. Core Game Systems (Çekirdek Oyun Sistemleri)

Bu katman, oyunun temel işlevselliğini sağlayan MonoBehaviour ve ScriptableObject sınıflarını içerir.

#### 1.1 CardData.cs
- **Tip**: ScriptableObject
- **Sorumluluk**: Kart veri yapısı (suit, rank, sprite)
- **Özellikler**:
  - `Suit` enum (Clubs, Diamonds, Hearts, Spades)
  - `Rank` enum (Two=2, Three...Ace=14)
  - `NumericValue` property (rank değerini int olarak döndürür)
  - `cardFrontGraphic` (Sprite referansı)
- **Desen**: ScriptableObject Pattern
- **Değerlendirme**: ✅ Temiz veri yapısı, Unity'nin asset sistemiyle uyumlu

#### 1.2 CardObject.cs
- **Tip**: MonoBehaviour
- **Sorumluluk**: Kart görseli, tıklama algılama, seçim durumu yönetimi
- **Önemli Özellikler**:
  - Component caching (SpriteRenderer, Collider) - lazy initialization
  - Topmost card detection (Physics.RaycastAll kullanımı)
  - Selection state management (position, scale, color değişiklikleri)
- **Bağımlılıklar**:
  - `HandManager` (public field, Initialize() ile atanıyor)
- **Potansiyel Sorunlar**:
  - ⚠️ `OnMouseDown()` Unity'nin eski input sistemi (yeni Input System'e geçiş önerilir)
  - ⚠️ `mainCamera` static field (thread-safety sorunu olabilir, ancak Unity'de genelde sorun değil)
- **Değerlendirme**: ✅ İyi component caching, ⚠️ Input sistemi güncellenebilir

#### 1.3 DeckManager.cs
- **Tip**: MonoBehaviour
- **Sorumluluk**: Deste yönetimi, kart çekme, el düzenleme
- **Önemli Metodlar**:
  - `InitializeDeck()`: Deste oluşturma
  - `ShuffleDrawDeck()`: Karıştırma (LINQ OrderBy kullanımı)
  - `DrawCard()` / `DrawCardAtPosition()`: Kart çekme
  - `RearrangeHand()`: El yeniden düzenleme
- **Bağımlılıklar**:
  - `HandManager` (FindFirstObjectByType - Rule 2 ihlali)
  - `UIManager` (FindFirstObjectByType - Rule 2 ihlali)
  - `ICardLayoutCalculator` (interface injection ✅)
- **Desen**: Manager Pattern, Interface Segregation
- **Değerlendirme**: ⚠️ FindFirstObjectByType kullanımı interface injection'a çevrilmeli

#### 1.4 HandManager.cs
- **Tip**: MonoBehaviour
- **Sorumluluk**: El yönetimi, kart seçimi, oyun/discard işlemleri
- **Önemli Metodlar**:
  - `AddCardToHand()`: Karta el ekleme
  - `UpdateSelectedHandType()`: Seçili kartların el tipini güncelleme
  - `PlayHand()`: El oynama ve skor hesaplama
  - `DiscardSelectedAndDraw()`: Kart atma ve yeni kart çekme
- **Bağımlılıklar**:
  - `DeckManager` (public field + FindFirstObjectByType fallback - ⚠️ Rule 2 ihlali)
  - `UIManager` (FindFirstObjectByType - ⚠️ Rule 2 ihlali)
  - `PokerHandEvaluator` (new instance ✅)
  - `HandScoringService` (new instance ✅)
- **Desen**: Manager Pattern, Service Pattern
- **Değerlendirme**: ✅ Service pattern kullanımı iyi, ⚠️ Dependency injection iyileştirilebilir

#### 1.5 UIManager.cs
- **Tip**: MonoBehaviour
- **Sorumluluk**: UI koordinasyonu, skor güncelleme, durum mesajları
- **Önemli Metodlar**:
  - `UpdateScore()`: Skor güncelleme (overload: chips+multiplier veya total score)
  - `ShowHandType()` / `HideHandType()`: El tipi gösterimi
  - `UpdateDiscardStatus()`: Discard durumu güncelleme
  - `SetStatus()`: Durum mesajı (coroutine ile otomatik geri dönüş)
- **Bağımlılıklar**:
  - `HandManager` (public field + FindFirstObjectByType fallback - ⚠️ Rule 2 ihlali)
  - `BackgroundPanelManager` (public field + FindFirstObjectByType fallback)
- **Desen**: Manager Pattern
- **Değerlendirme**: ✅ Background panel yönetimi iyi organize edilmiş, ⚠️ Dependency injection iyileştirilebilir

#### 1.6 PokerHandType.cs
- **Tip**: Enum
- **Sorumluluk**: Poker el tiplerini tanımlama
- **Değerler**: None, HighCard, Pair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush, FiveOfAKind, FlushHouse, FlushFive
- **Değerlendirme**: ✅ Kapsamlı el tipi tanımları

---

### 2. Services Layer (Servis Katmanı)

Bu katman, iş mantığını içeren stateless servis sınıflarını içerir.

#### 2.1 PokerHandEvaluator.cs
- **Tip**: Plain C# Class (stateless service)
- **Sorumluluk**: Poker el tiplerini belirleme algoritması
- **Önemli Metodlar**:
  - `DetermineHand()`: Ana değerlendirme metodu
  - `AnalyzeHand()`: El analizi (rank/suit sayıları)
  - `EvaluateHandType()`: El tipi belirleme (öncelik sırasına göre)
  - `CheckStraight()`: Straight kontrolü (ace-low desteği var)
  - `CheckRoyalFlush()`: Royal flush kontrolü
- **Helper Structs**:
  - `HandAnalysis`: Analiz sonuçları
  - `RankCombinationCounts`: Rank kombinasyon sayıları
- **Desen**: Service Pattern, Strategy Pattern (implicit)
- **Değerlendirme**: ✅ Temiz algoritma yapısı, ✅ Royal flush ve ace-low straight desteği, ✅ İyi organize edilmiş helper structs

#### 2.2 HandScoringService.cs
- **Tip**: Plain C# Class (stateless service)
- **Sorumluluk**: El skorları ve çarpanları
- **Önemli Metodlar**:
  - `GetBaseScore()`: El tipine göre base chip skoru
  - `GetMultiplier()`: El tipine göre çarpan
- **Değerlendirme**: ✅ Basit, odaklanmış servis, ⚠️ Skor değerleri hardcoded (ScriptableObject'a taşınabilir)

---

### 3. Utilities Layer (Yardımcı Sınıflar)

Bu katman, yeniden kullanılabilir yardımcı fonksiyonları içerir.

#### 3.1 CardLayoutCalculator.cs
- **Tip**: Plain C# Class (ICardLayoutCalculator implementasyonu)
- **Sorumluluk**: Kart yerleşim hesaplamaları (arc/fan layout)
- **Önemli Metodlar**:
  - `CalculateCardLayout()`: Arc/fan layout hesaplama
  - `CalculateCardAngles()`: Rotation ve roll açıları hesaplama
- **Konfigürasyon Parametreleri**:
  - `ArcRadius = 1f`
  - `MaxAngle = 10f`
  - `HorizontalSpacing = 1f`
  - `ZOffsetPerCard = 0.01f`
  - `MaxRollAngle = 8f`
- **Desen**: Utility Pattern, Interface Segregation
- **Değerlendirme**: ✅ Interface implementasyonu, ✅ Matematiksel hesaplamalar iyi organize edilmiş, ⚠️ Sabit değerler configurable yapılabilir

#### 3.2 CardTransformHelper.cs
- **Tip**: Static Utility Class
- **Sorumluluk**: Transform veri toplama
- **Önemli Metodlar**:
  - `CollectCardTransforms()`: Kart transform verilerini toplama (position, rotation, sortingOrder)
- **Helper Struct**: `CardTransformData`
- **Desen**: Utility Pattern, DRY Principle
- **Değerlendirme**: ✅ Static utility pattern, ✅ DRY prensibi uygulanmış

---

### 4. Interfaces Layer (Arayüz Katmanı)

Bu katman, bağımlılık yönetimi için interface tanımlarını içerir.

#### 4.1 IUIManager.cs
- **Sorumluluk**: UI yönetimi operasyonları
- **Metodlar**:
  - `SetStatus()`: Durum mesajı
  - `UpdateScore()`: Skor güncelleme (2 overload)
  - `UpdateDiscardStatus()`: Discard durumu
  - `ShowHandType()` / `HideHandType()`: El tipi gösterimi
- **Değerlendirme**: ✅ İyi tanımlanmış interface, ✅ Interface Segregation uygulanmış

#### 4.2 IHandManager.cs
- **Sorumluluk**: El yönetimi operasyonları
- **Properties**:
  - `currentHand` (readonly)
  - `totalChips` (get/set)
  - `totalMultiplier` (get/set)
- **Metodlar**:
  - `AddCardToHand()`
  - `CanSelectCard()`
  - `UpdateSelectedHandType()`
  - `PlayHand()`
  - `DiscardSelectedAndDraw()`
  - `GetRemainingDiscards()`
- **Değerlendirme**: ✅ Kapsamlı interface tanımı

#### 4.3 IDeckManager.cs
- **Sorumluluk**: Deste yönetimi operasyonları
- **Metodlar**:
  - `InitializeDeck()`
  - `ShuffleDrawDeck()`
  - `DrawCard()` / `DrawCardAtPosition()`
  - `RearrangeHand()`
- **Değerlendirme**: ✅ Temiz interface tanımı

#### 4.4 ICardLayoutCalculator.cs
- **Sorumluluk**: Kart yerleşim hesaplama operasyonları
- **Metodlar**:
  - `CalculateCardLayout()`: Layout bilgisi hesaplama
- **Helper Struct**: `CardLayoutInfo` (position, rotation, sortingOrder)
- **Değerlendirme**: ✅ Interface Segregation uygulanmış

---

### 5. UI Layer (Kullanıcı Arayüzü Katmanı)

#### 5.1 BackgroundPanelManager.cs
- **Tip**: MonoBehaviour
- **Sorumluluk**: UI arka plan panel yönetimi
- **Özellikler**:
  - Rounded rectangle sprite oluşturma
  - Text component'leri için background panel oluşturma
  - Editor ve runtime desteği
- **Değerlendirme**: ✅ Kapsamlı UI yönetimi, ✅ Editor mode desteği

---

## Bağımlılık Analizi

### Bağımlılık Grafiği

```
CardData (ScriptableObject)
    ↓
DeckManager (MonoBehaviour)
    ├──→ HandManager (FindFirstObjectByType ⚠️)
    ├──→ UIManager (FindFirstObjectByType ⚠️)
    └──→ ICardLayoutCalculator (Interface Injection ✅)
        ↓
    CardLayoutCalculator
        ↓
    CardObject (MonoBehaviour)
        ↓
    HandManager (MonoBehaviour)
        ├──→ DeckManager (FindFirstObjectByType ⚠️)
        ├──→ UIManager (FindFirstObjectByType ⚠️)
        ├──→ PokerHandEvaluator (new instance ✅)
        └──→ HandScoringService (new instance ✅)
            ↓
        UIManager (MonoBehaviour)
            ├──→ HandManager (FindFirstObjectByType ⚠️)
            └──→ BackgroundPanelManager (FindFirstObjectByType)
```

### Bağımlılık Yönetimi Sorunları

#### Rule 2 İhlalleri (Dependency Management)

1. **DeckManager.Awake()**:
   ```csharp
   handManager = FindFirstObjectByType<HandManager>();  // ⚠️
   uiManager = FindFirstObjectByType<UIManager>();      // ⚠️
   ```
   **Önerilen Çözüm**: Interface injection veya [SerializeField] ile atama

2. **HandManager.Awake()**:
   ```csharp
   deckManager = FindFirstObjectByType<DeckManager>();  // ⚠️
   uiManager = FindFirstObjectByType<UIManager>();       // ⚠️
   ```
   **Not**: `deckManager` public field olarak da tanımlı, bu iyi bir başlangıç.

3. **UIManager.Awake()**:
   ```csharp
   handManager = FindFirstObjectByType<HandManager>();   // ⚠️
   ```
   **Not**: `handManager` public field olarak da tanımlı, bu iyi bir başlangıç.

#### İyi Uygulamalar

1. **ICardLayoutCalculator Interface Injection**:
   ```csharp
   layoutCalculator = new CardLayoutCalculator();  // ✅
   ```
   Interface üzerinden kullanılıyor, bu iyi bir pratik.

2. **Service Instantiation**:
   ```csharp
   handEvaluator = new PokerHandEvaluator();      // ✅
   scoringService = new HandScoringService();     // ✅
   ```
   Stateless servisler için new instance kullanımı uygun.

---

## Mimari Desenler

### Kullanılan Desenler

1. **ScriptableObject Pattern**: CardData için
2. **Service Pattern**: PokerHandEvaluator, HandScoringService
3. **Interface Segregation**: IDeckManager, IHandManager, IUIManager, ICardLayoutCalculator
4. **Utility Pattern**: CardTransformHelper, CardLayoutCalculator
5. **Manager Pattern**: DeckManager, HandManager, UIManager
6. **Component Caching**: CardObject (SpriteRenderer, Collider)
7. **Lazy Initialization**: CardObject component properties

### Eksik/İyileştirilebilir Desenler

1. **Dependency Injection**: FindObjectOfType yerine constructor/property injection
2. **Observer Pattern**: UI güncellemeleri için event system (C# events veya UnityEvents)
3. **State Pattern**: Oyun durumu yönetimi için (oyun durumları: Playing, Discarding, etc.)
4. **Command Pattern**: Undo/redo için (gelecekte gerekirse)
5. **Factory Pattern**: CardObject instantiation için (şu an DeckManager içinde)

---

## Veri Akışı

### Oyun Akışı

```
1. Initialization
   CardData (ScriptableObject assets)
       ↓
   DeckManager.InitializeDeck()
       ↓
   DeckManager.ShuffleDrawDeck()
       ↓
   DeckManager.DrawInitialHand(8)
       ↓
   CardObject.Instantiate() + Initialize()
       ↓
   HandManager.AddCardToHand()

2. Card Selection
   CardObject.OnMouseDown()
       ↓
   CardObject.DetectClick()
       ↓
   HandManager.UpdateSelectedHandType()
       ↓
   PokerHandEvaluator.DetermineHand()
       ↓
   HandScoringService.GetBaseScore() + GetMultiplier()
       ↓
   UIManager.UpdateScore() + ShowHandType()

3. Play Hand
   UIManager.HandlePlayHandClicked()
       ↓
   HandManager.PlayHand()
       ↓
   PokerHandEvaluator.DetermineHand()
       ↓
   HandScoringService.GetBaseScore() + GetMultiplier()
       ↓
   CardTransformHelper.CollectCardTransforms()
       ↓
   HandManager.ResetSelection()
       ↓
   DeckManager.DrawCardAtPosition() (replacement cards)
       ↓
   UIManager.UpdateScore()

4. Discard
   UIManager.HandleDiscardClicked()
       ↓
   HandManager.DiscardSelectedAndDraw()
       ↓
   CardTransformHelper.CollectCardTransforms()
       ↓
   DeckManager.DrawCardAtPosition() (replacement cards)
       ↓
   UIManager.UpdateDiscardStatus()
```

---

## Kod Kalitesi Değerlendirmesi

### Güçlü Yönler ✅

1. **Interface Kullanımı**: IDeckManager, IHandManager, IUIManager, ICardLayoutCalculator
2. **Component Caching**: CardObject, DeckManager
3. **Service Layer Ayrımı**: PokerHandEvaluator, HandScoringService
4. **Utility Sınıfları**: CardLayoutCalculator, CardTransformHelper (DRY prensibi)
5. **Kod Kalite Kontrol Sistemi**: CodeReviewProcess, ViolationReporter mevcut
6. **Documentation**: XML comments mevcut
7. **Struct Kullanımı**: HandAnalysis, RankCombinationCounts, CardLayoutInfo, CardTransformData

### İyileştirme Gereken Alanlar ⚠️

#### Rule 2 İhlalleri (Dependency Management)

- **DeckManager**: FindFirstObjectByType kullanımı
- **HandManager**: FindFirstObjectByType kullanımı
- **UIManager**: FindFirstObjectByType kullanımı

**Önerilen Çözüm**:
```csharp
// Örnek: DeckManager için
[SerializeField] private IHandManager handManager;
[SerializeField] private IUIManager uiManager;

void Awake()
{
    if (handManager == null)
        handManager = FindFirstObjectByType<HandManager>();
    if (uiManager == null)
        uiManager = FindFirstObjectByType<UIManager>();
}
```

#### Rule 3 Potansiyel İhlalleri (Performance)

- **CardObject.IsTopmostCardAtPosition()**: Physics.RaycastAll her tıklamada
  - **Not**: Bu kabul edilebilir çünkü event-driven (Update içinde değil)
  - **İyileştirme**: Raycast sonuçlarını cache'leyebilir (ancak gerekli değil)

#### Rule 4 Kontrolleri (Null Checks)

- Genel olarak iyi null kontrolleri var
- Bazı yerlerde daha erken kontrol edilebilir (Awake/Start)

#### Rule 5 (Access Modifiers)

- Bazı public field'lar property'ye dönüştürülebilir
- Örnek: `HandManager.currentHand` (readonly property olabilir)

#### Diğer İyileştirmeler

1. **Input System**: OnMouseDown() yerine yeni Input System
2. **Config System**: Skor değerleri, layout parametreleri için ScriptableObject
3. **Event System**: UI güncellemeleri için C# events veya UnityEvents
4. **Error Handling**: Try-catch blokları ve error logging

---

## Öneriler

### Kısa Vadeli (Yüksek Öncelik)

1. ✅ **FindObjectOfType kullanımlarını interface injection'a çevir**
   - [SerializeField] ile interface atama
   - FindFirstObjectByType sadece fallback olarak

2. ✅ **Public field'ları property'lere dönüştür** (gerekli yerlerde)
   - `HandManager.currentHand` → readonly property
   - Encapsulation iyileştirmesi

3. ⚠️ **Input System'e geçiş** (OnMouseDown yerine)
   - Unity'nin yeni Input System kullanımı
   - Daha esnek input yönetimi

### Orta Vadeli (Orta Öncelik)

1. **Event system ekle** (UI güncellemeleri için)
   - C# events veya UnityEvents
   - Loose coupling için

2. **Config system** (skor değerleri, layout parametreleri için)
   - ScriptableObject tabanlı config
   - HandScoringService değerlerini config'den al

3. **Unit test framework ekle**
   - NUnit veya Unity Test Framework
   - PokerHandEvaluator için testler

### Uzun Vadeli (Düşük Öncelik)

1. **State management pattern** (oyun durumları için)
   - State machine veya state pattern
   - Oyun durumları: Playing, Discarding, GameOver, etc.

2. **Save/Load system**
   - PlayerPrefs veya JSON serialization
   - Oyun durumu kaydetme/yükleme

3. **Multiplayer desteği** (eğer planlanıyorsa)
   - Network architecture
   - Synchronization

---

## Sonuç

**Cardo** projesi, genel olarak iyi organize edilmiş bir Unity kart oyunu projesidir. Kod kalite kontrol sistemi dikkat çekicidir ve proje içinde kullanılmaktadır. 

### Genel Değerlendirme

- **Mimari Organizasyon**: ✅ İyi (katmanlı yapı mevcut)
- **Interface Kullanımı**: ✅ İyi (4 interface tanımlı)
- **Service Ayrımı**: ✅ İyi (PokerHandEvaluator, HandScoringService)
- **Dependency Management**: ⚠️ İyileştirilebilir (FindObjectOfType kullanımı)
- **Kod Kalitesi**: ✅ İyi (component caching, null checks, documentation)

### Ana İyileştirme Alanı

Ana iyileştirme alanı **dependency management (Rule 2)** konusundadır. FindFirstObjectByType kullanımları interface injection'a çevrilmelidir. Bunun dışında, genel olarak temiz kod prensipleri uygulanmış ve service/utility ayrımı yapılmıştır.

---

## Ek Notlar

- Proje, Unity 2021+ versiyonları için tasarlanmış görünüyor
- TextMeshPro kullanımı mevcut (modern UI)
- ExecuteInEditMode kullanımı (editor tooling)
- Coroutine kullanımı (async işlemler için)

---

**Analiz Tarihi**: 2024
**Analiz Eden**: AI Assistant
**Proje Versiyonu**: Mevcut kod tabanı
