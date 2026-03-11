using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unity Editor window for managing code review process sessions.
/// Provides a GUI interface for the 3-step code review process.
/// </summary>
public class CodeReviewProcessWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private Vector2 plannedChangesScroll;
    private Vector2 actualChangesScroll;
    private Vector2 violationsScroll;
    
    private string sessionDescription = "";
    private string newFilePath = "";
    private string newChangeDescription = "";
    private string newChangeType = "Değişiklik";
    private string[] changeTypes = { "Değişiklik", "Refactoring", "Debug", "Yeni Özellik", "Optimizasyon", "Düzeltme" };
    private int selectedChangeTypeIndex = 0;
    
    // Actual changes input fields
    private string actualFilePath = "";
    private string actualChangeDescription = "";
    private int actualChangeTypeIndex = 0;

    private CodeReviewProcess.ReviewSession currentSession = null;

    [MenuItem("Cardo/Code Quality/Code Review Process")]
    public static void ShowWindow()
    {
        GetWindow<CodeReviewProcessWindow>("Code Review Process");
    }

    private void OnEnable()
    {
        RefreshCurrentSession();
    }

    private void OnFocus()
    {
        RefreshCurrentSession();
    }

    private void RefreshCurrentSession()
    {
        currentSession = CodeReviewProcess.GetCurrentSession();
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Kod İnceleme Süreci", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Bu pencere, kod değişikliklerini 3 adımda takip etmenize yardımcı olur:\n" +
            "1. Değişiklik Planlama\n" +
            "2. Kod Yazma\n" +
            "3. Değişiklik Sonrası Kontrol",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Session Management
        DrawSessionManagement();

        EditorGUILayout.Space(10);

        if (currentSession != null)
        {
            // Step 1: Planning
            DrawPlanningPhase();

            EditorGUILayout.Space(10);

            // Step 2: Code Writing
            DrawCodeWritingPhase();

            EditorGUILayout.Space(10);

            // Step 3: Post-Change Control
            DrawPostChangeControl();

            EditorGUILayout.Space(10);

            // Session Summary
            DrawSessionSummary();
        }
        else
        {
            EditorGUILayout.HelpBox("Yeni bir review session başlatmak için yukarıdaki 'Yeni Session Başlat' butonunu kullanın.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSessionManagement()
    {
        EditorGUILayout.LabelField("Session Yönetimi", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        if (currentSession == null)
        {
            EditorGUILayout.LabelField("Aktif Session:", "Yok");
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Session Açıklaması:", EditorStyles.miniLabel);
            sessionDescription = EditorGUILayout.TextField(sessionDescription);
            
            if (GUILayout.Button("Yeni Session Başlat", GUILayout.Height(30)))
            {
                if (string.IsNullOrEmpty(sessionDescription))
                {
                    EditorUtility.DisplayDialog("Eksik Bilgi", 
                        "Lütfen session açıklaması girin.", "Tamam");
                }
                else
                {
                    CodeReviewProcess.StartReviewSession(sessionDescription);
                    RefreshCurrentSession();
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("Aktif Session:", currentSession.SessionId.Substring(0, 8) + "...");
            EditorGUILayout.LabelField("Açıklama:", currentSession.SessionDescription);
            
            if (GUILayout.Button("Session'ı Tamamla", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Session'ı Tamamla",
                    "Review session'ı tamamlamak ve rapor oluşturmak istediğinize emin misiniz?",
                    "Evet", "Hayır"))
                {
                    CodeReviewProcess.CompleteReviewSession();
                    RefreshCurrentSession();
                    sessionDescription = "";
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Hızlı Başlangıç Rehberi", GUILayout.Height(25)))
        {
            CodeReviewProcess.PrintQuickStartGuide();
        }
        if (GUILayout.Button("Tüm Session'ları Temizle", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Temizle",
                "Tüm review session'ları temizlemek istediğinize emin misiniz?",
                "Evet", "Hayır"))
            {
                CodeReviewProcess.ClearAllSessions();
                RefreshCurrentSession();
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPlanningPhase()
    {
        EditorGUILayout.LabelField("ADIM 1: DEĞİŞİKLİK PLANLAMA", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Yeni Planlanan Değişiklik Ekle:", EditorStyles.miniLabel);
        newFilePath = EditorGUILayout.TextField("Dosya Yolu:", newFilePath);
        newChangeDescription = EditorGUILayout.TextArea(newChangeDescription, GUILayout.Height(60));
        selectedChangeTypeIndex = EditorGUILayout.Popup("Değişiklik Tipi:", selectedChangeTypeIndex, changeTypes);
        newChangeType = changeTypes[selectedChangeTypeIndex];

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Değişiklik Ekle", GUILayout.Height(25)))
        {
            if (!string.IsNullOrEmpty(newFilePath) && !string.IsNullOrEmpty(newChangeDescription))
            {
                CodeReviewProcess.PlanChange(newFilePath, newChangeDescription, newChangeType);
                newFilePath = "";
                newChangeDescription = "";
                RefreshCurrentSession();
            }
            else
            {
                EditorUtility.DisplayDialog("Eksik Bilgi", 
                    "Lütfen dosya yolu ve değişiklik açıklaması girin.", "Tamam");
            }
        }
        if (GUILayout.Button("Planlama Fazını Tamamla", GUILayout.Height(25)))
        {
            CodeReviewProcess.CompletePlanningPhase();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Planned Changes List
        if (currentSession.PlannedChanges.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Planlanan Değişiklikler ({currentSession.PlannedChanges.Count}):", EditorStyles.miniLabel);
            plannedChangesScroll = EditorGUILayout.BeginScrollView(plannedChangesScroll, GUILayout.Height(150));
            
            for (int i = 0; i < currentSession.PlannedChanges.Count; i++)
            {
                var change = currentSession.PlannedChanges[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{i + 1}. {change.FilePath} ({change.ChangeType})", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(change.ChangeDescription, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"Etkilenen Kurallar: {string.Join(", ", change.AffectedRules.Select(r => $"Kural {r}"))}", 
                    EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawCodeWritingPhase()
    {
        EditorGUILayout.LabelField("ADIM 2: KOD YAZMA", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField("Gerçekleştirilen Değişiklik Kaydet:", EditorStyles.miniLabel);
        actualFilePath = EditorGUILayout.TextField("Dosya Yolu:", actualFilePath);
        actualChangeDescription = EditorGUILayout.TextArea(actualChangeDescription, GUILayout.Height(60));
        actualChangeTypeIndex = EditorGUILayout.Popup("Değişiklik Tipi:", actualChangeTypeIndex, changeTypes);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Değişiklik Kaydet", GUILayout.Height(25)))
        {
            if (!string.IsNullOrEmpty(actualFilePath) && !string.IsNullOrEmpty(actualChangeDescription))
            {
                CodeReviewProcess.RecordActualChange(actualFilePath, actualChangeDescription, changeTypes[actualChangeTypeIndex]);
                actualFilePath = "";
                actualChangeDescription = "";
                RefreshCurrentSession();
            }
            else
            {
                EditorUtility.DisplayDialog("Eksik Bilgi", 
                    "Lütfen dosya yolu ve değişiklik açıklaması girin.", "Tamam");
            }
        }
        if (GUILayout.Button("Etkilenen Kurallar İçin Kontrol Listesi", GUILayout.Height(25)))
        {
            CodeReviewProcess.PrintChecklistsForAffectedRules();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Actual Changes List
        if (currentSession.ActualChanges.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Gerçekleştirilen Değişiklikler ({currentSession.ActualChanges.Count}):", EditorStyles.miniLabel);
            actualChangesScroll = EditorGUILayout.BeginScrollView(actualChangesScroll, GUILayout.Height(150));
            
            for (int i = 0; i < currentSession.ActualChanges.Count; i++)
            {
                var change = currentSession.ActualChanges[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{i + 1}. {change.FilePath} ({change.ChangeType})", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(change.ChangeDescription, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawPostChangeControl()
    {
        EditorGUILayout.LabelField("ADIM 3: DEĞİŞİKLİK SONRASI KONTROL", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.HelpBox(
            "Değişiklik sonrası kontrol için tüm 6 kural için kontrol listesi çalıştırılacaktır.\n" +
            "İhlaller tespit edilirse ViolationReporter ile raporlanmalıdır.",
            MessageType.Info
        );

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Post-Change Validation Çalıştır", GUILayout.Height(30)))
        {
            CodeReviewProcess.PerformPostChangeValidation();
            RefreshCurrentSession();
        }
        if (GUILayout.Button("Raporlanan İhlalleri Görüntüle", GUILayout.Height(30)))
        {
            ViolationReporter.PrintAllViolations();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // Validation Results
        if (currentSession.PostChangeReport != null)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Valıdasyon Sonuçları:", EditorStyles.boldLabel);
            
            bool allCompliant = currentSession.PostChangeReport.AllRulesCompliant;
            EditorGUILayout.HelpBox(
                $"Tüm Kurallar Uyumlu: {(allCompliant ? "✅ Evet" : "❌ Hayır")}\n" +
                $"Toplam İhlal: {currentSession.ReportedViolations.Count}\n" +
                $"Toplam Uyarı: {currentSession.PostChangeReport.TotalWarnings}",
                allCompliant ? MessageType.Info : MessageType.Warning
            );
        }

        // Violations List
        if (currentSession.ReportedViolations.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"Raporlanan İhlaller ({currentSession.ReportedViolations.Count}):", EditorStyles.boldLabel);
            violationsScroll = EditorGUILayout.BeginScrollView(violationsScroll, GUILayout.Height(200));
            
            for (int i = 0; i < currentSession.ReportedViolations.Count; i++)
            {
                var violation = currentSession.ReportedViolations[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"İhlal {i + 1}: Kural {violation.RuleNumber} - {violation.RuleName}", 
                    EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Dosya: {violation.FilePath}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Satır: {violation.LineNumber}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"İhlal: {violation.ViolationDescription}", EditorStyles.wordWrappedLabel);
                if (!string.IsNullOrEmpty(violation.Reason))
                {
                    EditorGUILayout.LabelField($"Neden: {violation.Reason}", EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawSessionSummary()
    {
        EditorGUILayout.LabelField("Session Özeti", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"Session ID: {currentSession.SessionId.Substring(0, 8)}...");
        EditorGUILayout.LabelField($"Başlangıç: {currentSession.StartTime:yyyy-MM-dd HH:mm:ss}");
        if (currentSession.EndTime.HasValue)
        {
            EditorGUILayout.LabelField($"Bitiş: {currentSession.EndTime.Value:yyyy-MM-dd HH:mm:ss}");
            TimeSpan duration = currentSession.EndTime.Value - currentSession.StartTime;
            EditorGUILayout.LabelField($"Süre: {duration.TotalMinutes:F2} dakika");
        }
        else
        {
            EditorGUILayout.LabelField("Durum: Devam ediyor");
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Planlanan: {currentSession.PlannedChanges.Count} değişiklik");
        EditorGUILayout.LabelField($"Gerçekleştirilen: {currentSession.ActualChanges.Count} değişiklik");
        EditorGUILayout.LabelField($"Raporlanan İhlal: {currentSession.ReportedViolations.Count}");

        EditorGUILayout.EndVertical();
    }
}

