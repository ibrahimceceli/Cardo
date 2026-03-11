using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Code review process system that tracks changes before and after modifications.
/// Implements the 3-step process from the mandatory coding rules compliance plan:
/// 1. Değişiklik Planlama (Change Planning)
/// 2. Kod Yazma (Code Writing)
/// 3. Değişiklik Sonrası Kontrol (Post-Change Control)
/// </summary>
public class CodeReviewProcess
{
    /// <summary>
    /// Represents a planned or actual code change.
    /// </summary>
    public class CodeChange
    {
        public string FilePath { get; set; }
        public string ChangeDescription { get; set; }
        public List<int> AffectedRules { get; set; } = new List<int>();
        public DateTime Timestamp { get; set; }
        public string ChangeType { get; set; } // "Refactoring", "Debug", "Yeni Özellik", etc.

        public CodeChange(string filePath, string changeDescription, string changeType = "Değişiklik")
        {
            FilePath = filePath;
            ChangeDescription = changeDescription;
            ChangeType = changeType;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Represents a complete code review session with before and after states.
    /// </summary>
    public class ReviewSession
    {
        public string SessionId { get; set; }
        public string SessionDescription { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<CodeChange> PlannedChanges { get; set; } = new List<CodeChange>();
        public List<CodeChange> ActualChanges { get; set; } = new List<CodeChange>();
        public CodeRuleValidationChecklist.ValidationReport PreChangeReport { get; set; }
        public CodeRuleValidationChecklist.ValidationReport PostChangeReport { get; set; }
        public List<ViolationReporter.Violation> ReportedViolations { get; set; } = new List<ViolationReporter.Violation>();
        public bool IsCompleted { get; set; }

        public ReviewSession(string sessionDescription)
        {
            SessionId = Guid.NewGuid().ToString();
            SessionDescription = sessionDescription;
            StartTime = DateTime.Now;
            IsCompleted = false;
        }
    }

    private static List<ReviewSession> reviewSessions = new List<ReviewSession>();
    private static ReviewSession currentSession = null;

    #region Step 1: Change Planning (Değişiklik Planlama)

    /// <summary>
    /// Starts a new code review session and begins the planning phase.
    /// This is Step 1: Değişiklik Planlama (Change Planning)
    /// </summary>
    /// <param name="sessionDescription">Description of what will be changed</param>
    /// <returns>The created review session</returns>
    public static ReviewSession StartReviewSession(string sessionDescription)
    {
        if (currentSession != null && !currentSession.IsCompleted)
        {
        }

        currentSession = new ReviewSession(sessionDescription);
        reviewSessions.Add(currentSession);

        return currentSession;
    }

    /// <summary>
    /// Plans a code change during the planning phase.
    /// Analyzes which rules might be affected based on the change description.
    /// </summary>
    /// <param name="filePath">Path to the file that will be changed</param>
    /// <param name="changeDescription">Description of what will be changed</param>
    /// <param name="changeType">Type of change: "Refactoring", "Debug", "Yeni Özellik", etc.</param>
    /// <param name="affectedRules">Optional list of rule numbers that might be affected</param>
    /// <returns>The created planned change</returns>
    public static CodeChange PlanChange(string filePath, string changeDescription, string changeType = "Değişiklik", List<int> affectedRules = null)
    {
        if (currentSession == null)
        {
            StartReviewSession("Otomatik başlatılan session");
        }

        if (currentSession.IsCompleted)
        {
            StartReviewSession("Yeni session");
        }

        CodeChange plannedChange = new CodeChange(filePath, changeDescription, changeType);

        // Auto-detect affected rules if not provided
        if (affectedRules == null || affectedRules.Count == 0)
        {
            affectedRules = DetectAffectedRules(changeDescription, filePath);
        }
        plannedChange.AffectedRules = affectedRules;

        currentSession.PlannedChanges.Add(plannedChange);

        return plannedChange;
    }

    /// <summary>
    /// Analyzes change description and file path to detect which rules might be affected.
    /// </summary>
    private static List<int> DetectAffectedRules(string changeDescription, string filePath)
    {
        List<int> affectedRules = new List<int>();
        string lowerDesc = changeDescription.ToLower();
        string lowerPath = filePath.ToLower();

        // Rule 1: SRP - Single Responsibility
        if (lowerDesc.Contains("sınıf") || lowerDesc.Contains("class") || 
            lowerDesc.Contains("manager") || lowerDesc.Contains("service") ||
            lowerDesc.Contains("utility") || lowerDesc.Contains("responsibility"))
        {
            affectedRules.Add(1);
        }

        // Rule 2: DIP/IOC - Dependency Management
        if (lowerDesc.Contains("dependency") || lowerDesc.Contains("bağımlılık") ||
            lowerDesc.Contains("interface") || lowerDesc.Contains("findobjectoftype") ||
            lowerDesc.Contains("injection") || lowerPath.Contains("manager"))
        {
            affectedRules.Add(2);
        }

        // Rule 3: Performance
        if (lowerDesc.Contains("update") || lowerDesc.Contains("performance") ||
            lowerDesc.Contains("performans") || lowerDesc.Contains("getcomponent") ||
            lowerDesc.Contains("cache") || lowerDesc.Contains("optimize"))
        {
            affectedRules.Add(3);
        }

        // Rule 4: Null Checks
        if (lowerDesc.Contains("null") || lowerDesc.Contains("kontrol") ||
            lowerDesc.Contains("check") || lowerDesc.Contains("validation"))
        {
            affectedRules.Add(4);
        }

        // Rule 5: Access Modifiers
        if (lowerDesc.Contains("public") || lowerDesc.Contains("private") ||
            lowerDesc.Contains("property") || lowerDesc.Contains("field") ||
            lowerDesc.Contains("erişim") || lowerDesc.Contains("access"))
        {
            affectedRules.Add(5);
        }

        // Rule 6: DRY - Code Duplication
        if (lowerDesc.Contains("duplicate") || lowerDesc.Contains("tekrar") ||
            lowerDesc.Contains("dry") || lowerDesc.Contains("refactor") ||
            lowerDesc.Contains("helper") || lowerDesc.Contains("utility"))
        {
            affectedRules.Add(6);
        }

        // If no rules detected, assume all might be affected (conservative approach)
        if (affectedRules.Count == 0)
        {
            affectedRules.AddRange(new[] { 1, 2, 3, 4, 5, 6 });
        }

        return affectedRules.Distinct().OrderBy(r => r).ToList();
    }

    /// <summary>
    /// Completes the planning phase and provides a summary of planned changes.
    /// </summary>
    /// <returns>Summary of planned changes</returns>
    public static string CompletePlanningPhase()
    {
        if (currentSession == null)
        {
            return "⚠️ Review session başlatılmamış.";
        }

        if (currentSession.PlannedChanges.Count == 0)
        {
            return "Hiç planlanan değişiklik yok.";
        }

        System.Text.StringBuilder summary = new System.Text.StringBuilder();
        summary.AppendLine("\n=== PLANLAMA FAZİ SONU ÖZETİ ===");
        summary.AppendLine($"Toplam Planlanan Değişiklik: {currentSession.PlannedChanges.Count}");
        summary.AppendLine();

        var changesByType = currentSession.PlannedChanges.GroupBy(c => c.ChangeType);
        foreach (var group in changesByType)
        {
            summary.AppendLine($"{group.Key}: {group.Count()} dosya");
        }

        summary.AppendLine("\nPlanlanan Dosyalar:");
        foreach (var change in currentSession.PlannedChanges)
        {
            summary.AppendLine($"  • {change.FilePath}");
            summary.AppendLine($"    Etkilenen Kurallar: {string.Join(", ", change.AffectedRules.Select(r => $"Kural {r}"))}");
        }

        summary.AppendLine("\nADIM 2: KOD YAZMA");
        summary.AppendLine("Kod değişikliklerini yapın. Her kural için kontrol listesini kullanın.");
        summary.AppendLine("Kod yazma tamamlandığında RecordActualChange() metodunu kullanın.");

        return summary.ToString();
    }

    #endregion

    #region Step 2: Code Writing (Kod Yazma)

    /// <summary>
    /// Records an actual code change that was made.
    /// This should be called after making code changes.
    /// </summary>
    /// <param name="filePath">Path to the file that was changed</param>
    /// <param name="changeDescription">Description of what was actually changed</param>
    /// <param name="changeType">Type of change: "Refactoring", "Debug", "Yeni Özellik", etc.</param>
    /// <returns>The recorded actual change</returns>
    public static CodeChange RecordActualChange(string filePath, string changeDescription, string changeType = "Değişiklik")
    {
        if (currentSession == null)
        {
            StartReviewSession("Otomatik başlatılan session");
        }

        CodeChange actualChange = new CodeChange(filePath, changeDescription, changeType);
        currentSession.ActualChanges.Add(actualChange);

        return actualChange;
    }

    /// <summary>
    /// Gets the checklist for a specific rule to help during code writing.
    /// </summary>
    /// <param name="ruleNumber">Rule number (1-6)</param>
    /// <returns>Checklist information for the rule</returns>
    public static CodeRuleValidationChecklist.RuleChecklistInfo GetChecklistForRule(int ruleNumber)
    {
        var allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
        if (allChecklists.ContainsKey(ruleNumber))
        {
            return allChecklists[ruleNumber];
        }
        return null;
    }

    /// <summary>
    /// Prints checklists for all affected rules in the current session.
    /// Useful during code writing phase.
    /// </summary>
    public static void PrintChecklistsForAffectedRules()
    {
        if (currentSession == null || currentSession.PlannedChanges.Count == 0)
        {
            return;
        }

        var allAffectedRules = currentSession.PlannedChanges
            .SelectMany(c => c.AffectedRules)
            .Distinct()
            .OrderBy(r => r)
            .ToList();

        var allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
        foreach (int ruleNumber in allAffectedRules)
        {
            if (allChecklists.ContainsKey(ruleNumber))
            {
                var ruleInfo = allChecklists[ruleNumber];
            }
        }
    }

    #endregion

    #region Step 3: Post-Change Control (Değişiklik Sonrası Kontrol)

    /// <summary>
    /// Performs post-change validation by running checklists for all 6 rules.
    /// This is Step 3: Değişiklik Sonrası Kontrol (Post-Change Control)
    /// </summary>
    /// <param name="changedFiles">List of files that were actually changed</param>
    /// <param name="changeDescription">Overall description of changes</param>
    /// <returns>Validation report</returns>
    public static CodeRuleValidationChecklist.ValidationReport PerformPostChangeValidation(List<string> changedFiles = null, string changeDescription = "")
    {
        if (currentSession == null)
        {
            StartReviewSession("Otomatik başlatılan session");
        }

        if (changedFiles == null && currentSession.ActualChanges.Count > 0)
        {
            changedFiles = currentSession.ActualChanges.Select(c => c.FilePath).Distinct().ToList();
        }
        else if (changedFiles == null)
        {
            changedFiles = new List<string>();
        }

        if (string.IsNullOrEmpty(changeDescription) && currentSession != null)
        {
            changeDescription = currentSession.SessionDescription;
        }

        // Run validation
        CodeRuleValidationChecklist.ValidationReport report = 
            CodeRuleValidationChecklist.ValidateCodeChanges(changedFiles, changeDescription);

        // Store report
        currentSession.PostChangeReport = report;

        // Get reported violations
        currentSession.ReportedViolations = ViolationReporter.GetReportedViolations().ToList();

        if (currentSession.ReportedViolations.Count > 0)
        {
            ViolationReporter.PrintAllViolations();
        }

        return report;
    }

    /// <summary>
    /// Completes the review session and generates a final report.
    /// </summary>
    /// <returns>Final review report</returns>
    public static string CompleteReviewSession()
    {
        if (currentSession == null)
        {
            return "⚠️ Review session başlatılmamış.";
        }

        if (!currentSession.IsCompleted)
        {
            // Perform post-change validation if not done
            if (currentSession.PostChangeReport == null)
            {
                PerformPostChangeValidation();
            }

            currentSession.EndTime = DateTime.Now;
            currentSession.IsCompleted = true;
        }

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("\n=== KOD İNCELEME SÜRECİ RAPORU ===\n");
        report.AppendLine($"Session ID: {currentSession.SessionId}");
        report.AppendLine($"Açıklama: {currentSession.SessionDescription}");
        report.AppendLine($"Başlangıç: {currentSession.StartTime:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Bitiş: {currentSession.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Devam ediyor"}");

        if (currentSession.StartTime != null && currentSession.EndTime != null)
        {
            TimeSpan duration = currentSession.EndTime.Value - currentSession.StartTime;
            report.AppendLine($"Süre: {duration.TotalMinutes:F2} dakika");
        }

        report.AppendLine("\n--- PLANLANAN DEĞİŞİKLİKLER ---");
        report.AppendLine($"Toplam: {currentSession.PlannedChanges.Count}");
        foreach (var change in currentSession.PlannedChanges)
        {
            report.AppendLine($"  • {change.FilePath} ({change.ChangeType})");
            report.AppendLine($"    {change.ChangeDescription}");
        }

        report.AppendLine("\n--- GERÇEKLEŞTİRİLEN DEĞİŞİKLİKLER ---");
        report.AppendLine($"Toplam: {currentSession.ActualChanges.Count}");
        foreach (var change in currentSession.ActualChanges)
        {
            report.AppendLine($"  • {change.FilePath} ({change.ChangeType})");
            report.AppendLine($"    {change.ChangeDescription}");
        }

        report.AppendLine("\n--- VALİDASYON SONUÇLARI ---");
        if (currentSession.PostChangeReport != null)
        {
            report.AppendLine($"Tüm Kurallar Uyumlu: {(currentSession.PostChangeReport.AllRulesCompliant ? "✅ Evet" : "❌ Hayır")}");
            report.AppendLine($"Toplam İhlal: {currentSession.ReportedViolations.Count}");
            report.AppendLine($"Toplam Uyarı: {currentSession.PostChangeReport.TotalWarnings}");
        }
        else
        {
            report.AppendLine("⚠️ Post-change validation yapılmamış.");
        }

        if (currentSession.ReportedViolations.Count > 0)
        {
            report.AppendLine("\n--- RAPORLANAN İHLALLER ---");
            foreach (var violation in currentSession.ReportedViolations)
            {
                report.AppendLine(ViolationReporter.FormatViolationReport(violation));
                report.AppendLine();
            }
        }

        report.AppendLine("\n=== RAPOR SONU ===\n");

        // Clear current session
        currentSession = null;

        return report.ToString();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets the current active review session.
    /// </summary>
    public static ReviewSession GetCurrentSession()
    {
        return currentSession;
    }

    /// <summary>
    /// Gets all review sessions.
    /// </summary>
    public static List<ReviewSession> GetAllSessions()
    {
        return reviewSessions.ToList();
    }

    /// <summary>
    /// Gets a specific review session by ID.
    /// </summary>
    public static ReviewSession GetSessionById(string sessionId)
    {
        return reviewSessions.FirstOrDefault(s => s.SessionId == sessionId);
    }

    /// <summary>
    /// Clears all review sessions (useful for testing or reset).
    /// </summary>
    public static void ClearAllSessions()
    {
        reviewSessions.Clear();
        currentSession = null;
    }

    /// <summary>
    /// Generates a quick start guide for using the code review process.
    /// </summary>
    public static void PrintQuickStartGuide()
    {
    }

    #endregion
}
