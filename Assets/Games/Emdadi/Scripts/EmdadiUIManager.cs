using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System.Collections.Generic;
using TMPro;

public class EmdadiUIManager : MonoBehaviour
{
    [Header("--- TIMER UI ---")]
    public TMP_InputField timerInputField; 
    public Button timerToggleButton; 
    public RTLTextMeshPro timerStatusText; 
    public KashkoolTimerUI timerUI; // Visual Circle

    [Header("--- SYSTEM BUTTONS ---")]
    public Button restartButton;
    public Button exitButton;

    [Header("--- ANIMATION ---")]
    public TripleStackAnimation introAnimation;

    [Header("--- WALL PANEL (Display 2) ---")]
    public GameObject wallPanel;
    public RTLTextMeshPro wallCenterWordText;
    public RTLTextMeshPro wallTotalScoreText;

    [Header("Tasks (Right Side)")]
    public RTLTextMeshPro[] wallTaskTexts; 

    [Header("Selectors")]
    public RectTransform taskSelectorBox; 
    public RectTransform scoreSelectorBox; 
    public RectTransform[] rowTransforms;

    [Header("--- OPERATOR PANEL (Display 1) ---")]
    public GameObject opPanel;
    public RTLTextMeshPro opCenterWordText;
    public RTLTextMeshPro opTotalScoreText;
    public RTLTextMeshPro opStatusText;

    [Header("Controls")]
    public Button btnCorrect;
    public Button btnNextWord;
    public Button btnUndo;
    public Button btnSkip;   

    private Vector3 initialTaskSelectorPos;
    private Vector3 initialScoreSelectorPos;
    private bool isInitialized = false;

    void Awake()
    {
        if (taskSelectorBox != null) initialTaskSelectorPos = taskSelectorBox.position;
        if (scoreSelectorBox != null) initialScoreSelectorPos = scoreSelectorBox.position;
        isInitialized = true;
    }

    public void InitializeUI()
    {
        wallPanel.SetActive(true);
        opPanel.SetActive(true);
        UpdateScore(0);
        
        if(isInitialized)
        {
             if(taskSelectorBox) taskSelectorBox.position = initialTaskSelectorPos;
             if(scoreSelectorBox) scoreSelectorBox.position = initialScoreSelectorPos;
        }
    }

    // --- TIMER UI HELPERS ---
    public void UpdateTimerStatusUI(bool isRunning)
    {
        if (timerStatusText != null)
            timerStatusText.text = isRunning ? "PAUSE" : "RESUME";
            
        if (timerToggleButton != null)
            timerToggleButton.image.color = isRunning ? Color.red : Color.green;
    }

    public void UpdateWord(string word) { if (wallCenterWordText) wallCenterWordText.text = word; if (opCenterWordText) opCenterWordText.text = word; }
    public void UpdateScore(int totalScore) { if (wallTotalScoreText) wallTotalScoreText.text = totalScore.ToString(); if (opTotalScoreText) opTotalScoreText.text = "Total: " + totalScore; }
    
    public void UpdateTaskTexts(List<string> tasks) 
    {
        for (int i = 0; i < wallTaskTexts.Length; i++)
        {
            if (i < tasks.Count) wallTaskTexts[i].text = tasks[i];
            else wallTaskTexts[i].text = "";
        }
    }

    public void SetSelectorPosition(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rowTransforms.Length)
        {
            if (taskSelectorBox) taskSelectorBox.gameObject.SetActive(false);
            if (scoreSelectorBox) scoreSelectorBox.gameObject.SetActive(false);
            return;
        }
        if (taskSelectorBox) taskSelectorBox.gameObject.SetActive(true);
        if (scoreSelectorBox) scoreSelectorBox.gameObject.SetActive(true);

        if (rowTransforms.Length > 0 && rowTransforms[0] != null && rowTransforms[rowIndex] != null)
        {
            float verticalDiff = rowTransforms[rowIndex].position.y - rowTransforms[0].position.y;
            if (taskSelectorBox) { Vector3 newPos = initialTaskSelectorPos; newPos.y += verticalDiff; taskSelectorBox.position = newPos; }
            if (scoreSelectorBox) { Vector3 newPos = initialScoreSelectorPos; newPos.y += verticalDiff; scoreSelectorBox.position = newPos; }
        }
    }
}