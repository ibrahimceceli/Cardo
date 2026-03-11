using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window for identifying and reporting code quality violations.
/// </summary>
public class ViolationIdentifierWindow : EditorWindow
{
    [MenuItem("Cardo/Code Quality/Identify Violations")]
    public static void ShowWindow()
    {
        GetWindow<ViolationIdentifierWindow>("Violation Identifier");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Code Quality Violation Identifier", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Identify All Violations", GUILayout.Height(30)))
        {
            ViolationIdentifier.IdentifyAndReportAllViolations();
            ViolationIdentifier.PrintViolationSummary();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Print Violation Summary", GUILayout.Height(30)))
        {
            ViolationIdentifier.PrintViolationSummary();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Clear Violations", GUILayout.Height(30)))
        {
            ViolationReporter.ClearViolations();
        }
        
        GUILayout.Space(20);
        
        var violations = ViolationReporter.GetReportedViolations();
        GUILayout.Label($"Total Violations: {violations.Count}", EditorStyles.boldLabel);
        
        if (violations.Count > 0)
        {
            var summary = ViolationIdentifier.GetViolationSummary();
            foreach (var kvp in summary)
            {
                var ruleInfo = CodeRuleValidationChecklist.GetAllChecklists()[kvp.Key];
                GUILayout.Label($"Rule {kvp.Key} ({ruleInfo.RuleName}): {kvp.Value} violations");
            }
        }
    }
}
