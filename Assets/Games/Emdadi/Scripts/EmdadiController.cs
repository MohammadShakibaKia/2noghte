using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EmdadiController : MonoBehaviour
{
    private EmdadiDataManager dataManager;
    private EmdadiUIManager uiManager;

    private EmdadiData gameData;
    private List<string> currentTasks;

    // Game State
    private int currentWordIndex = 0;
    private int currentRowIndex = 0;
    private int totalScore = 0;
    private bool isRoundActive = false;

    // Timer State
    [Header("Timer Settings")]
    private float timerRemaining;
    public float timerTotalDuration = 60f;
    private bool isTimerRunning = false;

    void Awake()
    {
        dataManager = GetComponent<EmdadiDataManager>();
        uiManager = GetComponent<EmdadiUIManager>();
    }

    void Start()
    {
        // Secondary Display Activation
        if (Display.displays.Length > 1) Display.displays[1].Activate();

        gameData = dataManager.LoadData();
        if (gameData.words == null || gameData.words.Count == 0) return;

        currentTasks = new List<string>(gameData.initialTasks);

        // --- CONNECT BUTTONS ---
        uiManager.btnCorrect.onClick.AddListener(OnClick_Correct);
        uiManager.btnNextWord.onClick.AddListener(OnClick_NextWord);
        if (uiManager.btnUndo) uiManager.btnUndo.onClick.AddListener(OnClick_Undo);
        if (uiManager.btnSkip) 
    {
        uiManager.btnSkip.onClick.RemoveAllListeners();
        uiManager.btnSkip.onClick.AddListener(OnClick_Skip);
    }

        // Timer Controls
        if (uiManager.timerToggleButton) uiManager.timerToggleButton.onClick.AddListener(OnClick_ToggleTimer);
        if (uiManager.timerInputField) uiManager.timerInputField.onEndEdit.AddListener(delegate { OnClick_SetTimer(); });

        // System Controls
        if (uiManager.restartButton) uiManager.restartButton.onClick.AddListener(OnClick_RestartGame);
        if (uiManager.exitButton) uiManager.exitButton.onClick.AddListener(OnClick_ExitGame);

        // Intro Animation Listener
        if (uiManager.introAnimation != null)
            uiManager.introAnimation.onSequenceComplete.AddListener(OnAnimationFinished);

        uiManager.InitializeUI();
        LoadCurrentState();

        // Start Intro
        if (uiManager.introAnimation != null) uiManager.introAnimation.PlayFullSequence();
        else OnAnimationFinished();
    }

    void Update()
    {
        if (isTimerRunning && timerRemaining > 0)
        {
            timerRemaining -= Time.deltaTime;
            if (timerRemaining <= 0)
            {
                timerRemaining = 0;
                isTimerRunning = false;
                uiManager.UpdateTimerStatusUI(false);
            }
            if (uiManager.timerUI) uiManager.timerUI.UpdateVisuals(timerRemaining);
        }
    }

    // --- TIMER LOGIC ---

    public void OnClick_SetTimer()
    {
        if (uiManager.timerInputField == null) return;
        if (int.TryParse(uiManager.timerInputField.text, out int seconds) && seconds > 0)
        {
            timerTotalDuration = (float)seconds;
            timerRemaining = timerTotalDuration;
            if (uiManager.timerUI) uiManager.timerUI.InitializeTimer(timerTotalDuration);
            isTimerRunning = false;
            uiManager.UpdateTimerStatusUI(false);
        }
    }

    public void OnClick_ToggleTimer()
    {
        if (timerRemaining <= 0) OnClick_SetTimer();
        if (timerRemaining > 0)
        {
            isTimerRunning = !isTimerRunning;
            uiManager.UpdateTimerStatusUI(isTimerRunning);
        }
    }

    public void OnAnimationFinished()
    {
        isRoundActive = true;
        // Read input field for initial timer setup if empty/default
        OnClick_SetTimer();
        isTimerRunning = true;
        uiManager.UpdateTimerStatusUI(true);
    }

    // --- GAME LOGIC ---

    void LoadCurrentState()
    {
        if (currentWordIndex >= gameData.words.Count)
        {
            uiManager.UpdateWord("پایان");
            isRoundActive = false;
            isTimerRunning = false;
            uiManager.SetSelectorPosition(-1);
            return;
        }

        uiManager.UpdateWord(gameData.words[currentWordIndex]);
        uiManager.UpdateTaskTexts(currentTasks);
        currentRowIndex = 0;
        uiManager.SetSelectorPosition(currentRowIndex);
    }

   public void OnClick_Correct()
{
    if (!isRoundActive) return;

    // Add points
    int pointsToAdd = 5 - currentRowIndex;
    totalScore += pointsToAdd;
    uiManager.UpdateScore(totalScore);

    AdvanceRow();
}

public void OnClick_Skip()
{
    if (!isRoundActive) return;

    // No points added
    AdvanceRow();
}

private void AdvanceRow()
{
    currentRowIndex++;

    if (currentRowIndex >= 5)
    {
        // Round for this word is finished
        OnClick_NextWord();
    }
    else
    {
        // Move to next task
        uiManager.SetSelectorPosition(currentRowIndex);
    }
}

    public void OnClick_NextWord()
    {
        if (currentTasks.Count > 0)
        {
            string lastItem = currentTasks[currentTasks.Count - 1];
            currentTasks.RemoveAt(currentTasks.Count - 1);
            currentTasks.Insert(0, lastItem);
        }

        currentWordIndex++;
        LoadCurrentState();

        // Reset Timer for new word automatically
        timerRemaining = timerTotalDuration;
        if (uiManager.timerUI) uiManager.timerUI.UpdateVisuals(timerRemaining);
        isTimerRunning = true;
        uiManager.UpdateTimerStatusUI(true);
    }

   
    public void OnClick_Undo()
    {
        if (currentRowIndex > 0)
        {
            currentRowIndex--;
            totalScore -= (5 - currentRowIndex);
            uiManager.UpdateScore(totalScore);
            uiManager.SetSelectorPosition(currentRowIndex);
        }
    }

    // --- SYSTEM LOGIC ---

    public void OnClick_RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClick_ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}