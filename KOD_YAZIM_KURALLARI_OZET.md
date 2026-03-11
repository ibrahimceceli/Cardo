# Kod Yazım Kuralları Özeti

Bu dokümantasyon, ARCHITECTURE_ANALYSIS.md dosyasında bahsedilen 6 adet kaliteli kod yazım kuralının özetini içermektedir.

---

## Kural 1: Tek Sorumluluk Prensibi (SRP)

### Temel İlkeler
- Her sınıfın tek bir sorumluluğu olmalı
- CardObject sadece görünüm yönetmeli
- Manager sınıfları sadece koordinasyon yapmalı
- Service sınıfları sadece iş mantığı içermeli
- Utility sınıfları sadece yardımcı fonksiyonlar içermeli

### Amaç
Kodun bakımını kolaylaştırmak, test edilebilirliği artırmak ve değişikliklerin etkisini sınırlandırmak.

---

## Kural 2: Bağımlılık Yönetimi (DIP/IOC)

### Temel İlkeler
- `FindObjectOfType` yerine interface injection kullanılmalı
- Constructor injection veya `[SerializeField]` ile atama yapılmalı
- Interface'ler (IDeckManager, IHandManager, IUIManager) kullanılmalı
- `FindObjectOfType` sadece fallback olarak kullanılmalı

### Mevcut İhlaller
- **DeckManager**: `FindFirstObjectByType` doğrudan kullanılıyor
- **HandManager**: `FindFirstObjectByType` doğrudan kullanılıyor
- **UIManager**: `FindFirstObjectByType` doğrudan kullanılıyor

### Önerilen Çözüm
```csharp
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

### Amaç
Bağımlılıkları gevşek tutmak, test edilebilirliği artırmak ve kodun esnekliğini sağlamak.

---

## Kural 3: Performans Kısıtlamaları

### Temel İlkeler
- `Update()`, `LateUpdate()`, `FixedUpdate()` içinde `GetComponent()` çağrısı olmamalı
- `Update()` içinde `FindObjectOfType()` çağrısı olmamalı
- `Update()` içinde `Instantiate()` veya `Destroy()` çağrısı olmamalı
- Tüm component'ler `Awake()` veya `Start()` içinde cache'lenmeli

### Proje Durumu
✅ **Uyumlu**: Projede `Update()` metodları kullanılmamış, performans sorunu yok.

### Amaç
Oyun performansını korumak ve frame rate düşüşlerini önlemek.

---

## Kural 4: Null Kontrolü

### Temel İlkeler
- Tüm public field'lar kullanılmadan önce null kontrol edilmeli
- Tüm `[SerializeField]` field'lar kullanılmadan önce null kontrol edilmeli
- Inspector'dan atanan referanslar kontrol edilmeli
- Kritik referanslar `Awake()` veya `Start()` içinde kontrol edilmeli

### Proje Durumu
⚠️ **Kısmen Uyumlu**: Genel olarak iyi null kontrolleri var, bazı yerlerde iyileştirme gerekebilir.

### Amaç
Runtime hatalarını önlemek ve kodun güvenilirliğini artırmak.

---

## Kural 5: Erişim Belirteçleri (Access Modifiers)

### Temel İlkeler
- Tüm field'lar mümkün olduğunca private veya protected olmalı
- Sadece dışarıdan erişilmesi gereken özellikler public olmalı
- Property kullanımı gerekiyorsa property kullanılmalı
- Gereksiz public field'lar property ile değiştirilmeli
- `[SerializeField]` field'lar private olarak tanımlanmalı

### Örnek İyileştirme
```csharp
// Önce
public List<CardObject> currentHand = new List<CardObject>();

// Sonra
public List<CardObject> currentHand { get; private set; } = new List<CardObject>();
```

### Amaç
Encapsulation sağlamak ve kodun güvenliğini artırmak.

---

## Kural 6: Kod Tekrarı (DRY - Don't Repeat Yourself)

### Temel İlkeler
- Aynı kod bloğu 2+ yerde tekrarlanmamalı
- Vector3 hesaplamaları tekrarlanmamalı
- Kart listesinden çıkarma işlemi tekrarlanmamalı
- Tekrarlanan kod utility sınıflarına taşınmalı

### Proje Durumu
✅ **Uyumlu**: Projede CardTransformHelper ve CardLayoutCalculator ile DRY prensibi uygulanmış.

### İyi Uygulamalar
- **CardTransformHelper**: Transform toplama işlemi merkezileştirilmiş
- **CardLayoutCalculator**: Kart yerleşim hesaplamaları merkezileştirilmiş
- **PokerHandEvaluator**: Poker el değerlendirme mantığı merkezileştirilmiş
- **HandScoringService**: Skor hesaplama mantığı merkezileştirilmiş

### Amaç
Kod tekrarını azaltmak, bakımı kolaylaştırmak ve tutarlılığı sağlamak.

---

## Genel Değerlendirme

### Öncelikli İyileştirme Alanı
**Rule 2 (Bağımlılık Yönetimi)** en kritik iyileştirme alanıdır. `FindFirstObjectByType` kullanımları interface injection'a çevrilmelidir.

### Diğer Kurallar
- **Rule 1 (SRP)**: Kısmen uyumlu, bazı iyileştirmeler gerekebilir
- **Rule 3 (Performans)**: Tamamen uyumlu
- **Rule 4 (Null Checks)**: Kısmen uyumlu, bazı iyileştirmeler gerekebilir
- **Rule 5 (Access Modifiers)**: Kısmen uyumlu, bazı iyileştirmeler gerekebilir
- **Rule 6 (DRY)**: Tamamen uyumlu

---

## Sonuç

Bu 6 kural, kaliteli ve sürdürülebilir kod yazmak için temel prensiplerdir. Proje genel olarak bu kurallara uyumlu olsa da, özellikle **Rule 2 (Bağımlılık Yönetimi)** konusunda iyileştirmeler yapılması önerilmektedir.

---

**Kaynak**: ARCHITECTURE_ANALYSIS.md  
**Oluşturulma Tarihi**: 2024
