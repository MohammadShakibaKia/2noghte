using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using System.Collections.Generic;
using TMPro;

public class EmdadiUIManager : MonoBehaviour
{
    [Header("--- WALL PANEL (Display 2) ---")]
    public GameObject wallPanel;
    public RTLTextMeshPro wallCenterWordText;
    public RTLTextMeshPro wallTotalScoreText;

    [Header("Tasks (Right Side)")]
    // Assign the 5 Text objects for the tasks here (Top to Bottom)
    public RTLTextMeshPro[] wallTaskTexts; 

    [Header("Selectors (The Yellow Boxes)")]
    // The highlighted box for the Right side (Tasks)
    public RectTransform taskSelectorBox; 
    // The highlighted box for the Left side (Scores)
    public RectTransform scoreSelectorBox; 

    [Header("Row Positions")]
    // Drag the 5 Row GameObjects here. We use their positions to snap the Yellow Boxes.
    public RectTransform[] rowTransforms;

    [Header("--- OPERATOR PANEL (Display 1) ---")]
    public GameObject opPanel;
    public RTLTextMeshPro opCenterWordText;
    public RTLTextMeshPro opTotalScoreText;
    public RTLTextMeshPro opStatusText; // Shows "Row 1" or "Round Over"

    [Header("Controls")]
    public Button btnCorrect;   // "Correct / Next Step"
    public Button btnNextWord;  // "Next Person / Word"
    public Button btnUndo;      // Optional Undo

    public void InitializeUI()
    {
        wallPanel.SetActive(true);
        opPanel.SetActive(true);
        UpdateScore(0);
    }

    public void UpdateWord(string word)
    {
        if (wallCenterWordText) wallCenterWordText.text = word;
        if (opCenterWordText) opCenterWordText.text = word;
    }

    public void UpdateScore(int totalScore)
    {
        if (wallTotalScoreText) wallTotalScoreText.text = totalScore.ToString();
        if (opTotalScoreText) opTotalScoreText.text = "Total: " + totalScore;
    }

    public void UpdateTaskTexts(List<string> tasks)
    {
        for (int i = 0; i < wallTaskTexts.Length; i++)
        {
            if (i < tasks.Count)
                wallTaskTexts[i].text = tasks[i];
            else
                wallTaskTexts[i].text = "";
        }
    }

    public void SetSelectorPosition(int rowIndex)
    {
        // Check if index is valid (0 to 4)
        if (rowIndex >= 0 && rowIndex < rowTransforms.Length)
        {
            // Activate boxes
            if(taskSelectorBox) taskSelectorBox.gameObject.SetActive(true);
            if(scoreSelectorBox) scoreSelectorBox.gameObject.SetActive(true);

            // Move Right Selector (Task)
            if (taskSelectorBox && rowTransforms[rowIndex] != null)
            {
                // Keep X, change Y to match the row
                Vector3 newPos = taskSelectorBox.position;
                newPos.y = rowTransforms[rowIndex].position.y;
                taskSelectorBox.position = newPos;
            }

            // Move Left Selector (Score)
            if (scoreSelectorBox && rowTransforms[rowIndex] != null)
            {
                Vector3 newPos = scoreSelectorBox.position;
                newPos.y = rowTransforms[rowIndex].position.y;
                scoreSelectorBox.position = newPos;
            }

            if(opStatusText) opStatusText.text = $"Current: Row {rowIndex + 1} ({5 - rowIndex} pts)";
        }
        else
        {
            // If index is out of bounds (e.g. round finished), hide selectors
            if (taskSelectorBox) taskSelectorBox.gameObject.SetActive(false);
            if (scoreSelectorBox) scoreSelectorBox.gameObject.SetActive(false);
            
            if(opStatusText) opStatusText.text = "Round Finished. Click Next Word.";
        }
    }
}   