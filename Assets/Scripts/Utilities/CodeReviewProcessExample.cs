using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Example usage of the CodeReviewProcess system.
/// This demonstrates how to use the 3-step code review process.
/// 
/// This file is for reference only and can be deleted if not needed.
/// </summary>
public class CodeReviewProcessExample : MonoBehaviour
{
    [ContextMenu("Example: Start Review Session")]
    private void ExampleStartReviewSession()
    {
        // ADIM 1: DEĞİŞİKLİK PLANLAMA
        CodeReviewProcess.StartReviewSession("Örnek: Yeni özellik ekleme");
        
        // Plan changes
        CodeReviewProcess.PlanChange(
            "Assets/Scripts/DeckManager.cs",
            "DrawCard metoduna null kontrolü eklendi",
            "Debug"
        );
        
        CodeReviewProcess.PlanChange(
            "Assets/Scripts/HandManager.cs",
            "Yeni PlayHand metodu eklendi",
            "Yeni Özellik"
        );
        
        // Complete planning phase
        CodeReviewProcess.CompletePlanningPhase();
    }

    [ContextMenu("Example: Record Actual Changes")]
    private void ExampleRecordActualChanges()
    {
        // ADIM 2: KOD YAZMA
        // After making code changes, record them:
        
        CodeReviewProcess.RecordActualChange(
            "Assets/Scripts/DeckManager.cs",
            "DrawCard metoduna null kontrolü eklendi. cardPrefab null kontrolü yapılıyor.",
            "Debug"
        );
        
        CodeReviewProcess.RecordActualChange(
            "Assets/Scripts/HandManager.cs",
            "PlayHand metodu eklendi. Seçili kartlar oynanıyor ve yeni kartlar çekiliyor.",
            "Yeni Özellik"
        );
    }

    [ContextMenu("Example: Post-Change Validation")]
    private void ExamplePostChangeValidation()
    {
        // ADIM 3: DEĞİŞİKLİK SONRASI KONTROL
        
        List<string> changedFiles = new List<string>
        {
            "Assets/Scripts/DeckManager.cs",
            "Assets/Scripts/HandManager.cs"
        };
        
        // Perform validation
        CodeReviewProcess.PerformPostChangeValidation(changedFiles, "Yeni özellik ekleme");
        
        // If violations are found, report them:
        // ViolationReporter.ReportViolation(
        //     ruleNumber: 4,
        //     filePath: "Assets/Scripts/DeckManager.cs",
        //     lineNumber: 86,
        //     violationDescription: "cardPrefab null kontrolü eksik",
        //     reason: "Geçici olarak gerekli, sonra düzeltilecek"
        // );
    }

    [ContextMenu("Example: Complete Review Session")]
    private void ExampleCompleteReviewSession()
    {
        // Complete the review session and get final report
        string report = CodeReviewProcess.CompleteReviewSession();
    }

    [ContextMenu("Example: Full Workflow")]
    private void ExampleFullWorkflow()
    {
        // Complete workflow example
        
        // Step 1: Planning
        CodeReviewProcess.StartReviewSession("Örnek: Tam workflow");
        CodeReviewProcess.PlanChange("Assets/Scripts/Example.cs", "Örnek değişiklik", "Değişiklik");
        CodeReviewProcess.CompletePlanningPhase();
        
        // Step 2: Code Writing (simulated)
        CodeReviewProcess.RecordActualChange("Assets/Scripts/Example.cs", "Örnek değişiklik yapıldı", "Değişiklik");
        
        // Step 3: Post-Change Control
        CodeReviewProcess.PerformPostChangeValidation(
            new List<string> { "Assets/Scripts/Example.cs" },
            "Örnek değişiklik"
        );
        
        // Complete
        CodeReviewProcess.CompleteReviewSession();
    }

    [ContextMenu("Print Quick Start Guide")]
    private void PrintQuickStartGuide()
    {
        CodeReviewProcess.PrintQuickStartGuide();
    }
}
