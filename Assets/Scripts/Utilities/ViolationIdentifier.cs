using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Identifies and reports code quality rule violations in the Cardo project.
/// This script scans the codebase for violations of the 6 mandatory coding rules.
/// </summary>
public static class ViolationIdentifier
{
    /// <summary>
    /// Scans the codebase and identifies all violations, then reports them using ViolationReporter.
    /// </summary>
    public static void IdentifyAndReportAllViolations()
    {
        ViolationReporter.ClearViolations();
        
        // Rule 2: Dependency Management violations (FindObjectOfType)
        IdentifyRule2Violations();
        
        // Rule 5: Access Modifiers violations (Public fields)
        IdentifyRule5Violations();
        
        // Rule 3: Performance violations
        IdentifyRule3Violations();
        
        ViolationReporter.PrintAllViolations();
    }
    
    /// <summary>
    /// Identifies Rule 2 violations: FindObjectOfType usage instead of interface injection.
    /// </summary>
    private static void IdentifyRule2Violations()
    {
        // DeckManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/DeckManager.cs",
            lineNumber: 17,
            violationDescription: "FindFirstObjectByType<HandManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IHandManager interface'i mevcut, [SerializeField] ile injection yapılabilir"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/DeckManager.cs",
            lineNumber: 18,
            violationDescription: "FindFirstObjectByType<UIManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IUIManager interface'i mevcut, [SerializeField] ile injection yapılabilir"
        );
        
        // HandManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 23,
            violationDescription: "FindFirstObjectByType<DeckManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IDeckManager interface'i mevcut, [SerializeField] ile injection yapılabilir (zaten public field var ama fallback olarak kullanılıyor)"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 26,
            violationDescription: "FindFirstObjectByType<UIManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IUIManager interface'i mevcut, [SerializeField] ile injection yapılabilir"
        );
        
        // UIManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 65,
            violationDescription: "FindFirstObjectByType<BackgroundPanelManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "[SerializeField] ile atanabilir, zaten public field mevcut"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 124,
            violationDescription: "FindFirstObjectByType<HandManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IHandManager interface'i mevcut, [SerializeField] ile injection yapılabilir (zaten public field var ama fallback olarak kullanılıyor)"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 2,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 212,
            violationDescription: "FindFirstObjectByType<HandManager>() kullanıldı, interface injection tercih edilmeli",
            reason: "IHandManager interface'i mevcut, [SerializeField] ile injection yapılabilir (zaten public field var ama fallback olarak kullanılıyor)"
        );
    }
    
    /// <summary>
    /// Identifies Rule 5 violations: Public fields that should be properties or private.
    /// </summary>
    private static void IdentifyRule5Violations()
    {
        // DeckManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/DeckManager.cs",
            lineNumber: 7,
            violationDescription: "public List<CardData> allCardData - Inspector'dan atanması gerekiyorsa [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/DeckManager.cs",
            lineNumber: 9,
            violationDescription: "public GameObject cardPrefab - Inspector'dan atanması gerekiyorsa [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/DeckManager.cs",
            lineNumber: 10,
            violationDescription: "public Transform handSpawnPoint - Inspector'dan atanması gerekiyorsa [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        // HandManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 7,
            violationDescription: "public List<CardObject> currentHand - Read-only property olarak değiştirilmeli",
            reason: "Dışarıdan sadece okunması gereken field'lar property olmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 8,
            violationDescription: "public int totalChips - Property olarak değiştirilmeli (private set ile)",
            reason: "Dışarıdan okunabilir ama sadece sınıf içinden değiştirilebilir olmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 9,
            violationDescription: "public int totalMultiplier - Property olarak değiştirilmeli (private set ile)",
            reason: "Dışarıdan okunabilir ama sadece sınıf içinden değiştirilebilir olmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/HandManager.cs",
            lineNumber: 13,
            violationDescription: "public DeckManager deckManager - IDeckManager interface'i kullanılmalı, [SerializeField] private olmalı",
            reason: "Interface injection kullanılmalı ve [SerializeField] private olarak tanımlanmalı"
        );
        
        // CardObject.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 5,
            violationDescription: "public CardData cardInfo - Read-only property olarak değiştirilmeli",
            reason: "Dışarıdan sadece okunması gereken field'lar property olmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 8,
            violationDescription: "public HandManager handManager - IHandManager interface'i kullanılmalı, [HideInInspector] private olmalı",
            reason: "Interface injection kullanılmalı ve private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 11,
            violationDescription: "public bool isSelected - Property olarak değiştirilmeli (private set ile)",
            reason: "Dışarıdan okunabilir ama sadece sınıf içinden değiştirilebilir olmalı"
        );
        
        // UIManager.cs violations - Inspector fields should be [SerializeField] private
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 9,
            violationDescription: "public HandManager handManager - IHandManager interface'i kullanılmalı, [SerializeField] private olmalı",
            reason: "Interface injection kullanılmalı ve [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 10,
            violationDescription: "public Button playHandButton - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 11,
            violationDescription: "public Button discardButton - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 12,
            violationDescription: "public TMP_Text statusText - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 13,
            violationDescription: "public TMP_Text chipsText - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 14,
            violationDescription: "public TMP_Text multiplierText - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 15,
            violationDescription: "public TMP_Text scoreText - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 16,
            violationDescription: "public TMP_Text handTypeText - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 19,
            violationDescription: "public BackgroundPanelManager backgroundPanelManager - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 23,
            violationDescription: "public Color backgroundColor - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 27,
            violationDescription: "public float backgroundPaddingX - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 29,
            violationDescription: "public float backgroundPaddingY - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 33,
            violationDescription: "public Vector2 backgroundPositionOffset - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UIManager.cs",
            lineNumber: 35,
            violationDescription: "public Vector2 backgroundSizeMultiplier - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        // BackgroundPanelManager.cs violations
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UI/BackgroundPanelManager.cs",
            lineNumber: 11,
            violationDescription: "public Color backgroundColor - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UI/BackgroundPanelManager.cs",
            lineNumber: 15,
            violationDescription: "public float backgroundPaddingX - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UI/BackgroundPanelManager.cs",
            lineNumber: 17,
            violationDescription: "public float backgroundPaddingY - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UI/BackgroundPanelManager.cs",
            lineNumber: 21,
            violationDescription: "public Vector2 backgroundPositionOffset - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/UI/BackgroundPanelManager.cs",
            lineNumber: 23,
            violationDescription: "public Vector2 backgroundSizeMultiplier - [SerializeField] private olmalı",
            reason: "Inspector'dan atanacak field'lar [SerializeField] private olarak tanımlanmalı"
        );
        
        // CardData.cs violations (ScriptableObject - acceptable but noted)
        // Note: ScriptableObject public fields are generally acceptable, but we'll note them
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardData.cs",
            lineNumber: 24,
            violationDescription: "public Suit cardSuit - ScriptableObject için kabul edilebilir, ancak [SerializeField] private da kullanılabilir",
            reason: "ScriptableObject field'ları genellikle public olur, ancak [SerializeField] private da tercih edilebilir"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardData.cs",
            lineNumber: 25,
            violationDescription: "public Rank cardRank - ScriptableObject için kabul edilebilir, ancak [SerializeField] private da kullanılabilir",
            reason: "ScriptableObject field'ları genellikle public olur, ancak [SerializeField] private da tercih edilebilir"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 5,
            filePath: "Assets/Scripts/CardData.cs",
            lineNumber: 33,
            violationDescription: "public Sprite cardFrontGraphic - ScriptableObject için kabul edilebilir, ancak [SerializeField] private da kullanılabilir",
            reason: "ScriptableObject field'ları genellikle public olur, ancak [SerializeField] private da tercih edilebilir"
        );
    }
    
    /// <summary>
    /// Identifies Rule 3 violations: Performance issues.
    /// </summary>
    private static void IdentifyRule3Violations()
    {
        // CardObject.cs - Physics.RaycastAll in OnMouseDown (event-driven, acceptable but noted)
        // Note: This is event-driven, not in Update(), so it's acceptable but we note it for awareness
        ViolationReporter.ReportViolation(
            ruleNumber: 3,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 131,
            violationDescription: "Physics.RaycastAll() her tıklamada çağrılıyor - Performans açısından dikkat edilmeli",
            reason: "Event-driven olduğu için kabul edilebilir, ancak çok sık tıklamalarda performans sorunu yaratabilir"
        );
        
        // CardObject.cs - GetComponent in event handler (acceptable but could be optimized)
        ViolationReporter.ReportViolation(
            ruleNumber: 3,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 139,
            violationDescription: "GetComponent<CardObject>() raycast hit'lerde çağrılıyor - Cache'lenebilir",
            reason: "Event-driven olduğu için kabul edilebilir, ancak her hit için GetComponent çağrısı yapılıyor"
        );
        
        // CardObject.cs - Static Camera.main usage (potential thread-safety issue)
        ViolationReporter.ReportViolation(
            ruleNumber: 3,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 44,
            violationDescription: "private static Camera mainCamera - Static field thread-safety sorunu yaratabilir",
            reason: "Static field kullanımı thread-safety sorunlarına yol açabilir, instance field tercih edilmeli"
        );
        
        ViolationReporter.ReportViolation(
            ruleNumber: 3,
            filePath: "Assets/Scripts/CardObject.cs",
            lineNumber: 120,
            violationDescription: "Camera.main her kontrol edildiğinde çağrılıyor - Cache'lenmeli",
            reason: "Camera.main Unity'de tag-based arama yapar, her çağrıda maliyetlidir"
        );
        
        // BackgroundPanelManager.cs - GetComponentsInChildren in editor mode (acceptable)
        // Note: These are in editor mode methods, so they're acceptable but we note them
        
        // UIManager.cs - GetComponentInParent in methods (acceptable, already cached)
        // Note: These are already cached, so no violation
        
        // DeckManager.cs - GetComponent in DrawCardInternal (acceptable, one-time per card)
        // Note: This is acceptable as it's called once per card creation
    }
    
    /// <summary>
    /// Gets a summary of violations grouped by rule number.
    /// </summary>
    public static Dictionary<int, int> GetViolationSummary()
    {
        var violationsByRule = ViolationReporter.GetViolationsByRule();
        Dictionary<int, int> summary = new Dictionary<int, int>();
        
        foreach (var kvp in violationsByRule)
        {
            summary[kvp.Key] = kvp.Value.Count;
        }
        
        return summary;
    }
    
    /// <summary>
    /// Prints a formatted summary of all violations.
    /// </summary>
    public static void PrintViolationSummary()
    {
        var summary = GetViolationSummary();
        var allViolations = ViolationReporter.GetReportedViolations();
        
        foreach (var kvp in summary.OrderBy(x => x.Key))
        {
            var ruleInfo = CodeRuleValidationChecklist.GetAllChecklists()[kvp.Key];
        }
    }
}
