using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides standardized violation reporting for code quality rule violations.
/// Implements the mandatory violation reporting format as specified in the coding rules plan.
/// </summary>
public static class ViolationReporter
{
    /// <summary>
    /// Represents a single code quality rule violation.
    /// </summary>
    public class Violation
    {
        public int RuleNumber { get; set; }
        public string RuleName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string ViolationDescription { get; set; }
        public string Reason { get; set; }

        public Violation(int ruleNumber, string ruleName, string filePath, int lineNumber, string violationDescription, string reason = "")
        {
            RuleNumber = ruleNumber;
            RuleName = ruleName;
            FilePath = filePath;
            LineNumber = lineNumber;
            ViolationDescription = violationDescription;
            Reason = reason;
        }
    }

    private static List<Violation> reportedViolations = new List<Violation>();

    /// <summary>
    /// Gets all reported violations.
    /// </summary>
    public static IReadOnlyList<Violation> GetReportedViolations()
    {
        return reportedViolations.AsReadOnly();
    }

    /// <summary>
    /// Clears all reported violations.
    /// </summary>
    public static void ClearViolations()
    {
        reportedViolations.Clear();
    }

    /// <summary>
    /// Reports a violation using the mandatory format specified in the coding rules plan.
    /// Format:
    /// ⚠️ KURAL İHLALİ: [Kural Numarası] - [Kural Adı]
    /// Dosya: [Dosya Yolu]
    /// Satır: [Satır Numarası]
    /// İhlal: [Ne yapıldı]
    /// Neden: [Neden gerekli/kaçınılmaz]
    /// </summary>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <param name="ruleName">The name of the rule</param>
    /// <param name="filePath">Path to the file where violation occurred</param>
    /// <param name="lineNumber">Line number where violation occurred</param>
    /// <param name="violationDescription">Description of what was violated</param>
    /// <param name="reason">Reason why the violation is necessary/unavoidable (optional)</param>
    /// <returns>Formatted violation report string</returns>
    public static string ReportViolation(int ruleNumber, string ruleName, string filePath, int lineNumber, string violationDescription, string reason = "")
    {
        Violation violation = new Violation(ruleNumber, ruleName, filePath, lineNumber, violationDescription, reason);
        reportedViolations.Add(violation);

        string report = FormatViolationReport(violation);
        
        return report;
    }

    /// <summary>
    /// Reports a violation using rule information from CodeRuleValidationChecklist.
    /// </summary>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <param name="filePath">Path to the file where violation occurred</param>
    /// <param name="lineNumber">Line number where violation occurred</param>
    /// <param name="violationDescription">Description of what was violated</param>
    /// <param name="reason">Reason why the violation is necessary/unavoidable (optional)</param>
    /// <returns>Formatted violation report string</returns>
    public static string ReportViolation(int ruleNumber, string filePath, int lineNumber, string violationDescription, string reason = "")
    {
        var allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
        if (allChecklists.ContainsKey(ruleNumber))
        {
            string ruleName = allChecklists[ruleNumber].RuleName;
            return ReportViolation(ruleNumber, ruleName, filePath, lineNumber, violationDescription, reason);
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Formats a violation according to the mandatory format specified in the plan.
    /// </summary>
    /// <param name="violation">The violation to format</param>
    /// <returns>Formatted violation report string</returns>
    public static string FormatViolationReport(Violation violation)
    {
        if (violation == null)
        {
            return string.Empty;
        }

        string report = $"⚠️ KURAL İHLALİ: {violation.RuleNumber} - {violation.RuleName}\n" +
                       $"Dosya: {violation.FilePath}\n" +
                       $"Satır: {violation.LineNumber}\n" +
                       $"İhlal: {violation.ViolationDescription}\n";

        if (!string.IsNullOrEmpty(violation.Reason))
        {
            report += $"Neden: {violation.Reason}\n";
        }

        return report;
    }

    /// <summary>
    /// Formats a violation report string using the mandatory format.
    /// This is a convenience method that matches the format specified in CodeRuleValidationChecklist.
    /// </summary>
    /// <param name="filePath">Path to the file where violation occurred</param>
    /// <param name="lineNumber">Line number where violation occurred</param>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <param name="ruleName">The name of the rule</param>
    /// <param name="violationDescription">Description of what was violated</param>
    /// <param name="reason">Reason why the violation is necessary/unavoidable (optional)</param>
    /// <returns>Formatted violation report string</returns>
    public static string FormatViolationReport(string filePath, int lineNumber, int ruleNumber, string ruleName, string violationDescription, string reason = "")
    {
        return FormatViolationReport(new Violation(ruleNumber, ruleName, filePath, lineNumber, violationDescription, reason));
    }

    /// <summary>
    /// Prints all reported violations to the Unity console.
    /// </summary>
    public static void PrintAllViolations()
    {
        if (reportedViolations.Count == 0)
        {
            return;
        }
        
        for (int i = 0; i < reportedViolations.Count; i++)
        {
        }
    }

    /// <summary>
    /// Gets a summary of all violations grouped by rule number.
    /// </summary>
    /// <returns>Dictionary with rule number as key and list of violations as value</returns>
    public static Dictionary<int, List<Violation>> GetViolationsByRule()
    {
        Dictionary<int, List<Violation>> violationsByRule = new Dictionary<int, List<Violation>>();
        
        foreach (var violation in reportedViolations)
        {
            if (!violationsByRule.ContainsKey(violation.RuleNumber))
            {
                violationsByRule[violation.RuleNumber] = new List<Violation>();
            }
            violationsByRule[violation.RuleNumber].Add(violation);
        }
        
        return violationsByRule;
    }

    /// <summary>
    /// Gets the count of violations for a specific rule.
    /// </summary>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <returns>Number of violations for the specified rule</returns>
    public static int GetViolationCountForRule(int ruleNumber)
    {
        int count = 0;
        foreach (var violation in reportedViolations)
        {
            if (violation.RuleNumber == ruleNumber)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Generates a formatted violation report string for use in change descriptions, 
    /// commit messages, or documentation. This method follows the mandatory format 
    /// specified in the coding rules plan.
    /// 
    /// Usage example:
    /// string report = ViolationReporter.GenerateViolationReportForDocumentation(
    ///     ruleNumber: 2,
    ///     filePath: "Assets/Scripts/DeckManager.cs",
    ///     lineNumber: 17,
    ///     violationDescription: "FindFirstObjectByType kullanıldı, interface injection tercih edilmeli",
    ///     reason: "Mevcut kod yapısı ile uyumluluk için geçici olarak gerekli"
    /// );
    /// </summary>
    /// <param name="ruleNumber">The rule number (1-6)</param>
    /// <param name="filePath">Path to the file where violation occurred</param>
    /// <param name="lineNumber">Line number where violation occurred</param>
    /// <param name="violationDescription">Description of what was violated</param>
    /// <param name="reason">Reason why the violation is necessary/unavoidable (optional)</param>
    /// <returns>Formatted violation report string ready for use in documentation</returns>
    public static string GenerateViolationReportForDocumentation(
        int ruleNumber, 
        string filePath, 
        int lineNumber, 
        string violationDescription, 
        string reason = "")
    {
        var allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
        string ruleName = allChecklists.ContainsKey(ruleNumber) 
            ? allChecklists[ruleNumber].RuleName 
            : $"Kural {ruleNumber}";

        return FormatViolationReport(filePath, lineNumber, ruleNumber, ruleName, violationDescription, reason);
    }

    /// <summary>
    /// Gets a formatted summary of all violations for use in change descriptions or documentation.
    /// Returns an empty string if no violations have been reported.
    /// </summary>
    /// <returns>Formatted string containing all reported violations</returns>
    public static string GetFormattedViolationSummary()
    {
        if (reportedViolations.Count == 0)
        {
            return string.Empty;
        }

        System.Text.StringBuilder summary = new System.Text.StringBuilder();
        summary.AppendLine($"=== TOPLAM {reportedViolations.Count} İHLAL RAPORU ===");
        summary.AppendLine();

        for (int i = 0; i < reportedViolations.Count; i++)
        {
            summary.AppendLine($"--- İhlal {i + 1} ---");
            summary.AppendLine(FormatViolationReport(reportedViolations[i]));
            summary.AppendLine();
        }

        summary.AppendLine("=== İHLAL RAPORU SONU ===");
        return summary.ToString();
    }
}
