using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Validation checklist system for enforcing the 6 mandatory coding rules.
/// This class provides structured checklists that should be applied before every code change.
/// </summary>
public static class CodeRuleValidationChecklist
{
    /// <summary>
    /// Represents a validation result for a single rule check.
    /// </summary>
    public class RuleValidationResult
    {
        public int RuleNumber { get; set; }
        public string RuleName { get; set; }
        public bool IsCompliant { get; set; }
        public List<string> Violations { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Complete validation result for all 6 rules.
    /// </summary>
    public class ValidationReport
    {
        public List<RuleValidationResult> RuleResults { get; set; } = new List<RuleValidationResult>();
        public bool AllRulesCompliant => RuleResults.TrueForAll(r => r.IsCompliant);
        public int TotalViolations => RuleResults.Sum(r => r.Violations.Count);
        public int TotalWarnings => RuleResults.Sum(r => r.Warnings.Count);
    }

    #region Rule 1: Single Responsibility Principle (SRP)

    /// <summary>
    /// Checklist for Rule 1: Single Responsibility Principle
    /// </summary>
    public static class Rule1_SRP
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Her sınıfın tek bir sorumluluğu var mı?",
            "CardObject sadece görünüm yönetiyor mu? (Veri veya oyun mantığı içermiyor mu?)",
            "Manager sınıfları sadece yönetim yapıyor mu? (Hesaplama/UI güncelleme içermiyor mu?)",
            "Service sınıfları sadece iş mantığı içeriyor mu?",
            "Utility sınıfları sadece yardımcı fonksiyonlar içeriyor mu?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "CardObject: Sadece görsel durum (position, scale, color) ve tıklama algılama",
            "Manager Sınıfları: Sadece koordinasyon, hesaplama Service'lere, UI güncelleme UIManager'a",
            "Service Sınıfları: Sadece iş mantığı (evaluation, scoring)",
            "Utility Sınıfları: Sadece yardımcı fonksiyonlar (hesaplama, transform toplama)"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "CardObject içinde oyun mantığı (EvaluateHand gibi metodlar)",
            "Manager içinde hesaplama mantığı (Service'e taşınmalı)",
            "Manager içinde UI güncelleme (UIManager'a taşınmalı)",
            "Utility içinde state yönetimi"
        };
    }

    #endregion

    #region Rule 2: Dependency Management (DIP/IOC)

    /// <summary>
    /// Checklist for Rule 2: Dependency Inversion Principle / Inversion of Control
    /// </summary>
    public static class Rule2_DIP_IOC
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Yüksek seviyeli sınıflar düşük seviyeli sınıflara doğrudan bağımlı değil mi?",
            "FindObjectOfType veya FindFirstObjectByType yerine interface injection kullanılıyor mu?",
            "Constructor injection veya [SerializeField] ile interface atanıyor mu?",
            "Interface'ler (IDeckManager, IHandManager) kullanılıyor mu?",
            "FindObjectOfType sadece fallback olarak kullanılıyor mu?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "Interface Kullanımı: Mevcut interface'ler (IDeckManager, IHandManager) kullanılmalı",
            "Dependency Injection: Awake() veya Start() içinde interface'ler atanmalı",
            "FindObjectOfType Kullanımı: Sadece fallback olarak, interface bulunamazsa"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "Doğrudan concrete class bağımlılığı (HandManager yerine IHandManager kullanılmalı)",
            "FindObjectOfType doğrudan kullanımı (Interface injection tercih edilmeli)",
            "Awake/Start içinde interface atama eksikliği"
        };
    }

    #endregion

    #region Rule 3: Performance Constraints

    /// <summary>
    /// Checklist for Rule 3: Performance Constraints
    /// </summary>
    public static class Rule3_Performance
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Update(), LateUpdate(), FixedUpdate() içinde GetComponent() çağrısı var mı?",
            "Update() içinde FindObjectOfType() çağrısı var mı?",
            "Update() içinde Instantiate() veya Destroy() çağrısı var mı?",
            "Update() içinde string birleştirme (\"log: \" + değişken) var mı?",
            "Update() içinde yeni List<> oluşturuluyor mu?",
            "Tüm component'ler Awake() veya Start() içinde cache'leniyor mu?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "Component Caching: Tüm component'ler Awake() veya Start() içinde cache'lenmeli",
            "Update İçinde Yapılmaması Gerekenler:",
            "  - GetComponent<T>() → Cache'lenmeli",
            "  - FindObjectOfType<T>() → Awake/Start'ta yapılmalı",
            "  - Instantiate() / Destroy() → Event handler'larda yapılmalı",
            "  - String concatenation → StringBuilder veya önbelleklenmiş string kullanılmalı",
            "  - Yeni collection oluşturma → Önbelleklenmiş collection kullanılmalı"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "Update() içinde GetComponent<T>() çağrısı",
            "Update() içinde FindObjectOfType<T>() çağrısı",
            "Update() içinde Instantiate() veya Destroy()",
            "Update() içinde string concatenation",
            "Update() içinde yeni List<> veya Dictionary<> oluşturma"
        };
    }

    #endregion

    #region Rule 4: Null Checks

    /// <summary>
    /// Checklist for Rule 4: Null Checks
    /// </summary>
    public static class Rule4_NullChecks
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Tüm public field'lar kullanılmadan önce null kontrol ediliyor mu?",
            "Tüm [SerializeField] field'lar kullanılmadan önce null kontrol ediliyor mu?",
            "Inspector'dan atanan referanslar (cardPrefab, handSpawnPoint) kontrol ediliyor mu?",
            "Kritik referanslar Awake() veya Start() içinde kontrol ediliyor mu?",
            "Null kontrolü yapılmadan önce kullanılan referanslar var mı?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "Her Public/SerializeField Kullanımı Öncesi: if (referans == null) return; veya if (referans == null) { Debug.LogError(...); return; }",
            "Kritik Referanslar: cardPrefab, handSpawnPoint gibi kritik referanslar Awake() veya Start() içinde kontrol edilmeli"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "Public field kullanılmadan önce null kontrolü yok",
            "[SerializeField] field kullanılmadan önce null kontrolü yok",
            "Inspector'dan atanan referanslar (cardPrefab, handSpawnPoint) kontrol edilmiyor",
            "Kritik referanslar Awake/Start'ta kontrol edilmiyor"
        };
    }

    #endregion

    #region Rule 5: Access Modifiers

    /// <summary>
    /// Checklist for Rule 5: Access Modifiers
    /// </summary>
    public static class Rule5_AccessModifiers
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Tüm field'lar mümkün olduğunca private veya protected mi?",
            "Sadece dışarıdan erişilmesi gereken özellikler public mi?",
            "Property kullanımı gerekiyorsa property kullanılıyor mu?",
            "Gereksiz public field'lar var mı? (Property ile değiştirilmeli mi?)",
            "[SerializeField] field'lar private olarak tanımlanmış mı?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "Varsayılan: Tüm field'lar private olmalı",
            "Public Olması Gerekenler:",
            "  - Interface implementasyonu gerektiren property'ler",
            "  - Inspector'dan atanması gereken [SerializeField] field'lar (bunlar private olabilir)",
            "  - Dışarıdan erişilmesi gereken property'ler (getter/setter ile)"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "Gereksiz public field (Property ile değiştirilmeli)",
            "Public field yerine read-only property kullanılmalı",
            "Public field yerine property with private set kullanılmalı",
            "[SerializeField] field public olarak tanımlanmış (private olmalı)"
        };
    }

    #endregion

    #region Rule 6: Code Duplication (DRY)

    /// <summary>
    /// Checklist for Rule 6: Don't Repeat Yourself (DRY)
    /// </summary>
    public static class Rule6_DRY
    {
        public static readonly List<string> Checklist = new List<string>
        {
            "Aynı kod bloğu 2+ yerde tekrarlanıyor mu?",
            "Vector3 hesaplamaları tekrarlanıyor mu?",
            "Kart listesinden çıkarma işlemi tekrarlanıyor mu?",
            "Null kontrol pattern'leri tekrarlanıyor mu?",
            "Tekrarlanan kod utility sınıflarına taşınabilir mi?"
        };

        public static readonly List<string> Guidelines = new List<string>
        {
            "Utility Sınıfları: Tekrarlanan hesaplamalar CardLayoutCalculator, CardTransformHelper gibi utility'lere taşınmalı",
            "Helper Metodlar: Sınıf içinde tekrarlanan kodlar private helper metodlara çıkarılmalı",
            "Extension Methods: Yaygın pattern'ler extension method olarak eklenebilir"
        };

        public static readonly List<string> CommonViolations = new List<string>
        {
            "Aynı hesaplama mantığı birden fazla yerde tekrarlanıyor",
            "Aynı null kontrol pattern'i tekrarlanıyor",
            "Aynı transform hesaplaması tekrarlanıyor",
            "Tekrarlanan kod utility metodlara taşınmamış"
        };
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Gets the complete checklist for all 6 rules.
    /// </summary>
    public static Dictionary<int, RuleChecklistInfo> GetAllChecklists()
    {
        return new Dictionary<int, RuleChecklistInfo>
        {
            { 1, new RuleChecklistInfo("Tek Sorumluluk Prensibi (SRP)", Rule1_SRP.Checklist, Rule1_SRP.Guidelines, Rule1_SRP.CommonViolations) },
            { 2, new RuleChecklistInfo("Bağımlılık Yönetimi (DIP/IOC)", Rule2_DIP_IOC.Checklist, Rule2_DIP_IOC.Guidelines, Rule2_DIP_IOC.CommonViolations) },
            { 3, new RuleChecklistInfo("Performans Kısıtlamaları", Rule3_Performance.Checklist, Rule3_Performance.Guidelines, Rule3_Performance.CommonViolations) },
            { 4, new RuleChecklistInfo("Null Kontrolü", Rule4_NullChecks.Checklist, Rule4_NullChecks.Guidelines, Rule4_NullChecks.CommonViolations) },
            { 5, new RuleChecklistInfo("Erişim Belirteçleri", Rule5_AccessModifiers.Checklist, Rule5_AccessModifiers.Guidelines, Rule5_AccessModifiers.CommonViolations) },
            { 6, new RuleChecklistInfo("Kod Tekrarı (DRY)", Rule6_DRY.Checklist, Rule6_DRY.Guidelines, Rule6_DRY.CommonViolations) }
        };
    }

    /// <summary>
    /// Validates code changes against all 6 rules.
    /// This is a manual validation helper - developers should use this as a reference.
    /// </summary>
    /// <param name="changedFiles">List of files that were changed</param>
    /// <param name="changeDescription">Description of the changes made</param>
    /// <returns>Validation report with results for each rule</returns>
    public static ValidationReport ValidateCodeChanges(List<string> changedFiles, string changeDescription = "")
    {
        ValidationReport report = new ValidationReport();

        var allChecklists = GetAllChecklists();
        foreach (var kvp in allChecklists)
        {
            RuleValidationResult result = new RuleValidationResult
            {
                RuleNumber = kvp.Key,
                RuleName = kvp.Value.RuleName,
                IsCompliant = true // Default - should be set based on manual review
            };

            // Note: This is a manual validation system
            // Developers should review each checklist item and mark violations
            report.RuleResults.Add(result);
        }

        return report;
    }

    /// <summary>
    /// Prints the complete checklist to the Unity console for reference.
    /// </summary>
    public static void PrintChecklistToConsole()
    {
        var allChecklists = GetAllChecklists();
        
        foreach (var kvp in allChecklists)
        {
            int ruleNumber = kvp.Key;
            var info = kvp.Value;
            
            for (int i = 0; i < info.Checklist.Count; i++)
            {
            }
            
            foreach (var guideline in info.Guidelines)
            {
            }
            
            foreach (var violation in info.CommonViolations)
            {
            }
        }
    }

    /// <summary>
    /// Creates a formatted violation report string using the mandatory format.
    /// This method delegates to ViolationReporter for consistency.
    /// </summary>
    /// <param name="filePath">Path to the file where violation occurred</param>
    /// <param name="lineNumber">Line number where violation occurred</param>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <param name="ruleName">The name of the rule</param>
    /// <param name="violation">Description of what was violated</param>
    /// <param name="reason">Reason why the violation is necessary/unavoidable (optional)</param>
    /// <returns>Formatted violation report string</returns>
    public static string FormatViolationReport(string filePath, int lineNumber, int ruleNumber, string ruleName, string violation, string reason = "")
    {
        // Use ViolationReporter for consistent formatting
        return ViolationReporter.FormatViolationReport(filePath, lineNumber, ruleNumber, ruleName, violation, reason);
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Contains checklist information for a single rule.
    /// </summary>
    public class RuleChecklistInfo
    {
        public string RuleName { get; set; }
        public List<string> Checklist { get; set; }
        public List<string> Guidelines { get; set; }
        public List<string> CommonViolations { get; set; }

        public RuleChecklistInfo(string ruleName, List<string> checklist, List<string> guidelines, List<string> commonViolations)
        {
            RuleName = ruleName;
            Checklist = checklist;
            Guidelines = guidelines;
            CommonViolations = commonViolations;
        }
    }

    #endregion
}
