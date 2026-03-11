using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Unity Editor window for accessing the Code Rule Validation Checklist.
/// Provides an easy way to review the 6 mandatory coding rules before making code changes.
/// </summary>
public class CodeRuleValidationWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<int, CodeRuleValidationChecklist.RuleChecklistInfo> allChecklists;
    private int selectedRule = 1;

    [MenuItem("Cardo/Code Quality/Validation Checklist")]
    public static void ShowWindow()
    {
        GetWindow<CodeRuleValidationWindow>("Code Rule Validation Checklist");
    }

    private void OnEnable()
    {
        allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
    }

    private void OnGUI()
    {
        if (allChecklists == null)
        {
            allChecklists = CodeRuleValidationChecklist.GetAllChecklists();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Kod Kalite Kontrol Listesi", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Her kod değişikliğinde bu 6 kural için kontrol listesi uygulanmalıdır.", MessageType.Info);
        EditorGUILayout.Space(5);

        // Rule selection tabs
        EditorGUILayout.BeginHorizontal();
        for (int i = 1; i <= 6; i++)
        {
            if (GUILayout.Toggle(selectedRule == i, $"Kural {i}", EditorStyles.miniButton))
            {
                selectedRule = i;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Display selected rule
        if (allChecklists.ContainsKey(selectedRule))
        {
            var ruleInfo = allChecklists[selectedRule];
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Rule name
            EditorGUILayout.LabelField($"KURAL {selectedRule}: {ruleInfo.RuleName}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Checklist
            EditorGUILayout.LabelField("Kontrol Listesi:", EditorStyles.boldLabel);
            for (int i = 0; i < ruleInfo.Checklist.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Toggle(false, GUILayout.Width(20));
                EditorGUILayout.LabelField($"{i + 1}. {ruleInfo.Checklist[i]}", EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            // Guidelines
            EditorGUILayout.LabelField("Uygulama Rehberi:", EditorStyles.boldLabel);
            foreach (var guideline in ruleInfo.Guidelines)
            {
                EditorGUILayout.LabelField($"  • {guideline}", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.Space(10);

            // Common violations
            EditorGUILayout.LabelField("Yaygın İhlaller:", EditorStyles.boldLabel);
            foreach (var violation in ruleInfo.CommonViolations)
            {
                EditorGUILayout.HelpBox($"⚠ {violation}", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(10);

        // Violation Reporting Format Section
        EditorGUILayout.LabelField("İhlal Raporlama Formatı", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Eğer bir kural ihlal edilirse, değişiklik açıklamasında şu formatta belirtilmelidir:\n\n" +
            "⚠️ KURAL İHLALİ: [Kural Numarası] - [Kural Adı]\n" +
            "Dosya: [Dosya Yolu]\n" +
            "Satır: [Satır Numarası]\n" +
            "İhlal: [Ne yapıldı]\n" +
            "Neden: [Neden gerekli/kaçınılmaz]",
            MessageType.Info
        );

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Raporlanan İhlalleri Görüntüle", GUILayout.Height(25)))
        {
            ViolationReporter.PrintAllViolations();
        }
        if (GUILayout.Button("İhlalleri Temizle", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("İhlalleri Temizle", 
                "Tüm raporlanan ihlalleri temizlemek istediğinize emin misiniz?", 
                "Evet", "Hayır"))
            {
                ViolationReporter.ClearViolations();
            }
        }
        EditorGUILayout.EndHorizontal();

        // Show violation count
        var violations = ViolationReporter.GetReportedViolations();
        if (violations.Count > 0)
        {
            EditorGUILayout.HelpBox(
                $"Toplam {violations.Count} ihlal raporlanmış. Console'da detayları görüntüleyebilirsiniz.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space(10);

        // Action buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tüm Kuralları Console'a Yazdır", GUILayout.Height(30)))
        {
            CodeRuleValidationChecklist.PrintChecklistToConsole();
        }
        if (GUILayout.Button("Kapat", GUILayout.Height(30)))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }
}
