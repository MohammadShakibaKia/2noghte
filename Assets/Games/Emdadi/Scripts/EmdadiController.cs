using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EmdadiDataManager))]
[RequireComponent(typeof(EmdadiUIManager))]
public class EmdadiController : MonoBehaviour
{
    private EmdadiDataManager dataManager;
    private EmdadiUIManager uiManager;

    private EmdadiData gameData;
    private List<string> currentTasks; // The rotating list of tasks

    // State Variables
    private int currentWordIndex = 0;
    private int currentRowIndex = 0; // 0 = Top (5pts), 4 = Bottom (1pt)
    private int totalScore = 0;
    private bool isRoundActive = true;

    void Awake()
    {
        dataManager = GetComponent<EmdadiDataManager>();
        uiManager = GetComponent<EmdadiUIManager>();
    }

    void Start()
    {
        // 1. Load Data
        gameData = dataManager.LoadData();
        
        if(gameData.words == null || gameData.words.Count == 0)
        {
            Debug.LogError("No data loaded!");
            return;
        }

        // 2. Init Tasks List
        currentTasks = new List<string>(gameData.initialTasks);

        // 3. Connect Buttons
        uiManager.btnCorrect.onClick.AddListener(OnClick_Correct);
        uiManager.btnNextWord.onClick.AddListener(OnClick_NextWord);
        if(uiManager.btnUndo) uiManager.btnUndo.onClick.AddListener(OnClick_Undo);

        // 4. Start Game
        uiManager.InitializeUI();
        LoadCurrentState();
    }

    void LoadCurrentState()
    {
        // Check if we finished all words
        if (currentWordIndex >= gameData.words.Count)
        {
            uiManager.UpdateWord("پایان"); // End
            isRoundActive = false;
            uiManager.SetSelectorPosition(-1); // Hide selectors
            return;
        }

        // Set Word
        uiManager.UpdateWord(gameData.words[currentWordIndex]);

        // Set Tasks (Visuals)
        uiManager.UpdateTaskTexts(currentTasks);

        // Reset Row to Top
        currentRowIndex = 0;
        isRoundActive = true;
        uiManager.SetSelectorPosition(currentRowIndex);
    }

    public void OnClick_Correct()
    {
        if (!isRoundActive) return;

        // 1. Add Score
        // Row 0 = 5 pts, Row 1 = 4 pts, etc.
        int pointsToAdd = 5 - currentRowIndex;
        totalScore += pointsToAdd;
        uiManager.UpdateScore(totalScore);

        // 2. Move to Next Row
        currentRowIndex++;

        // 3. Check if round is over (after 5th row)
        if (currentRowIndex >= 5)
        {
            isRoundActive = false;
            uiManager.SetSelectorPosition(-1); // Hide Selectors
        }
        else
        {
            uiManager.SetSelectorPosition(currentRowIndex);
        }
    }

    public void OnClick_NextWord()
    {
        // THIS IS THE SHIFT LOGIC YOU REQUESTED
        // "Move all tasks on right by one, last one becomes first"
        if (currentTasks.Count > 0)
        {
            string lastItem = currentTasks[currentTasks.Count - 1];
            currentTasks.RemoveAt(currentTasks.Count - 1); // Remove last
            currentTasks.Insert(0, lastItem);              // Put it at start
        }

        // Increment Word
        currentWordIndex++;

        // Reload UI
        LoadCurrentState();
    }

    public void OnClick_Undo()
    {
        // Simple undo logic just in case operator clicks by mistake
        if (currentRowIndex > 0 && isRoundActive)
        {
            currentRowIndex--;
            int pointsToRemove = 5 - currentRowIndex;
            totalScore -= pointsToRemove;
            uiManager.UpdateScore(totalScore);
            uiManager.SetSelectorPosition(currentRowIndex);
        }
        else if (!isRoundActive && currentRowIndex == 5)
        {
            // Undo finishing the round
            isRoundActive = true;
            currentRowIndex--;
            int pointsToRemove = 5 - currentRowIndex;
            totalScore -= pointsToRemove;
            uiManager.UpdateScore(totalScore);
            uiManager.SetSelectorPosition(currentRowIndex);
        }
    }
}