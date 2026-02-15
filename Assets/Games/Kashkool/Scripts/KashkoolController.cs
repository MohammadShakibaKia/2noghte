using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using RTLTMPro;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(KashkoolDataManager))]
[RequireComponent(typeof(KashkoolUIManager))]
public class KashkoolController : MonoBehaviour
{
    [Header("Configuration")]
    public float revealStartDelay = 2.0f;
    public float fadeSpeed = 1.5f;

    [Header("System References")]
    public StackableAnimation bgSystem;

    private KashkoolDataManager dataManager;
    private KashkoolUIManager uiManager;

    private bool isBusy = false;
    private List<CategoryData> loadedCategories = new List<CategoryData>();
    private CategoryData currentCategory;

    // Game State
    private int currentCategoryIndex = -1;
    private int currentQIndex;
    private int score;
    private List<bool> answerHistory = new List<bool>();

    [Header("Timer Settings")]
    [SerializeField] private float defaultTimerDuration = 10f;
    [SerializeField] private float timerRemaining;
    [SerializeField] private bool isTimerRunning = false;

    // --- ADDED: Track max time for the progress bar ---
    private float timerTotalDuration = 60f;

    void Awake()
    {
        dataManager = GetComponent<KashkoolDataManager>();
        uiManager = GetComponent<KashkoolUIManager>();
    }

    void Start()
    {


        // --- ADD THIS LOGIC ---
        timerTotalDuration = defaultTimerDuration;
        timerRemaining = timerTotalDuration;

        // Show the default value in the Input Field so the operator sees it
        if (uiManager.timerInputField != null)
        {
            uiManager.timerInputField.text = timerTotalDuration.ToString();
        }
        Debug.Log("Displays connected: " + Display.displays.Length);

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }


        uiManager.ResetVisibility();

        // Connect Buttons
        if (uiManager.timerToggleButton != null)
        {
            uiManager.timerToggleButton.onClick.RemoveAllListeners();
            uiManager.timerToggleButton.onClick.AddListener(OnClick_ToggleTimer);
        }

        if (uiManager.timerInputField != null)
        {
            uiManager.timerInputField.onEndEdit.AddListener(delegate { OnClick_SetTimer(); });
        }

        if (uiManager.undoButton != null)
        {
            uiManager.undoButton.onClick.RemoveAllListeners();
            uiManager.undoButton.onClick.AddListener(OnClick_Undo);
        }

        if (dataManager.HasSaveData())
        {
            RestoreGame();
        }
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
                OnTimerEnd();
            }
            uiManager.UpdateTimerDisplay(timerRemaining);
        }
    }

    // --- TIMER LOGIC ---

    public void OnClick_SetTimer()
    {
        if (uiManager.timerInputField == null) return;
        string input = uiManager.timerInputField.text;

        if (int.TryParse(input, out int seconds))
        {
            timerTotalDuration = (float)seconds; // Set Max
            timerRemaining = timerTotalDuration;

            // Init Visuals (Fill Bar full)
            uiManager.InitializeVisualTimer(timerTotalDuration);
            uiManager.UpdateTimerDisplay(timerRemaining);

            isTimerRunning = false;
            uiManager.UpdateTimerStatusUI(false);

            Debug.Log($"[Kashkool] Timer Set: {timerRemaining}");
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

    private void OnTimerEnd()
    {
        Debug.Log("Time is up!");
        // Play sound here if needed
        OnClick_Answer(false);

    }

    // --- REVEAL SEQUENCE ---
    // (Same as before, abbreviated for clarity)
    public void OnClick_RevealCategories()
    {
        if (isBusy) return;
        dataManager.ClearSave();
        StartCoroutine(RevealSequenceRoutine());
    }

    IEnumerator RevealSequenceRoutine()
    {
        isBusy = true;

        // Ensure the grid is working again for the new setup
        if (uiManager.wallCategoryImages.Length > 0)
        {
            Transform gridParent = uiManager.wallCategoryImages[0].transform.parent;
            LayoutGroup layout = gridParent.GetComponent<LayoutGroup>();
            if (layout != null)
            {
                layout.enabled = true; // Re-enable sorting
            }
        }
        // ----------------------------
        if (bgSystem != null && bgSystem.IsAnimating) yield return new WaitUntil(() => !bgSystem.IsAnimating);

        bgSystem.TriggerForward();
        loadedCategories = dataManager.LoadCategoriesFromDisk();

        // Setup UI Loop (Images, Buttons) - Same as previous script
        for (int i = 0; i < loadedCategories.Count; i++)
        {
            Sprite catSprite = dataManager.LoadSpriteFromFile(loadedCategories[i].iconFileName);
            string catName = loadedCategories[i].categoryName;

            // Setup Wall & Operator (Code from previous step goes here - standard setup)
            if (i < uiManager.wallCategoryImages.Length)
            {
                Image wallImg = uiManager.wallCategoryImages[i];
                wallImg.sprite = catSprite;
                Transform titleT = wallImg.transform.Find("Title");
                if (titleT != null)
                {
                    Transform rtlT = titleT.Find("Text - RTLTMP");
                    if (rtlT != null) rtlT.GetComponent<RTLTextMeshPro>().text = catName;
                }
            }
            if (i < uiManager.opCategoryButtons.Length)
            {
                Button opBtn = uiManager.opCategoryButtons[i];
                Image opImg = opBtn.GetComponent<Image>();
                if (opImg == null) opImg = opBtn.GetComponentInChildren<Image>(true);
                if (opImg != null) opImg.sprite = catSprite;
                RTLTextMeshPro opRtl = opBtn.GetComponentInChildren<RTLTextMeshPro>(true);
                if (opRtl != null) opRtl.text = catName;
                int index = i;
                opBtn.onClick.RemoveAllListeners();
                opBtn.onClick.AddListener(() => OnClick_SelectCategory(index));
                opBtn.gameObject.SetActive(true);
            }
        }

        yield return new WaitUntil(() => !bgSystem.IsAnimating);
        yield return new WaitForSeconds(revealStartDelay);

        uiManager.opSelectionPanel.SetActive(true);

        // Fade In
        for (int i = 0; i < loadedCategories.Count; i++)
        {
            if (i < uiManager.wallCategoryImages.Length)
            {
                uiManager.wallCategoryImages[i].gameObject.SetActive(true);
                CanvasGroup wallCG = uiManager.wallCategoryImages[i].GetComponent<CanvasGroup>();
                if (!wallCG) wallCG = uiManager.wallCategoryImages[i].gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(uiManager.FadeCanvasGroup(wallCG, 0, 1, fadeSpeed));
            }
            if (i < uiManager.opCategoryButtons.Length)
            {
                CanvasGroup opCG = uiManager.opCategoryButtons[i].GetComponent<CanvasGroup>();
                if (!opCG) opCG = uiManager.opCategoryButtons[i].gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(uiManager.FadeCanvasGroup(opCG, 0, 1, fadeSpeed));
            }
            yield return new WaitForSeconds(0.2f);
        }
        isBusy = false;
    }

    // --- GAME LOGIC ---

    public void OnClick_SelectCategory(int index)
    {
        if (isBusy) return;

        Debug.Log($"[DEBUG] Category {index} Clicked. Checking Timer...");

        // 1. Force read the input field text
        if (uiManager.timerInputField != null)
        {
            string text = uiManager.timerInputField.text;
            Debug.Log($"[DEBUG] Input Field Text is: '{text}'");

            if (int.TryParse(text, out int secondsInput) && secondsInput > 0)
            {
                timerTotalDuration = secondsInput; // Update Max Time
                timerRemaining = secondsInput;     // Update Current Time
                Debug.Log($"[DEBUG] Time Set Successfully to: {timerRemaining}");
            }
            else
            {
                Debug.LogWarning("[DEBUG] Could not parse time or time is 0. Timer will NOT start.");
            }
        }

        currentCategoryIndex = index;
        currentCategory = loadedCategories[index];
        currentQIndex = 0;
        score = 0;
        answerHistory.Clear();
        uiManager.ResetProgressBars();

        // 2. Initialize the Wall Timer Visuals explicitly here
        uiManager.InitializeVisualTimer(timerTotalDuration);

        StartCoroutine(EnterGameModeRoutine(index));
    }

    IEnumerator EnterGameModeRoutine(int selectedIndex)
    {
        isBusy = true;
        List<Coroutine> fadeRoutines = new List<Coroutine>();

        // 1. Fade OUT the unselected ones
        for (int i = 0; i < uiManager.wallCategoryImages.Length; i++)
        {
            if (uiManager.wallCategoryImages[i] == null || !uiManager.wallCategoryImages[i].gameObject.activeSelf) continue;

            if (i != selectedIndex)
            {
                CanvasGroup wCG = uiManager.wallCategoryImages[i].GetComponent<CanvasGroup>();
                if (!wCG) wCG = uiManager.wallCategoryImages[i].gameObject.AddComponent<CanvasGroup>();
                // Just fade the alpha to 0, do NOT deactivate yet
                fadeRoutines.Add(StartCoroutine(uiManager.FadeCanvasGroup(wCG, wCG.alpha, 0, fadeSpeed * 3f)));
            }
        }

        // Wait for those fades to finish
        foreach (var r in fadeRoutines) yield return r;

        // --- REMOVE OR COMMENT OUT THIS LOOP BELOW ---
        /* 
        for (int i = 0; i < uiManager.wallCategoryImages.Length; i++)
            if (i != selectedIndex && uiManager.wallCategoryImages[i] != null) 
                 uiManager.wallCategoryImages[i].gameObject.SetActive(false); 
        */
        // ----------------------------------------------

        yield return new WaitForSeconds(1.0f);

        // 2. Fade out the SELECTED one (the one that stayed in its spot)
        if (selectedIndex < uiManager.wallCategoryImages.Length)
        {
            CanvasGroup selWallCG = uiManager.wallCategoryImages[selectedIndex].GetComponent<CanvasGroup>();
            if (selWallCG != null) yield return StartCoroutine(uiManager.FadeCanvasGroup(selWallCG, 1, 0, fadeSpeed * 2f));
        }

        // 3. NOW deactivate the entire panel at once
        // This is where everything gets cleaned up safely
        uiManager.opSelectionPanel.SetActive(false);
        uiManager.wallSelectionPanel.SetActive(false);

        // Turn off individual images now that the whole panel is hidden
        for (int i = 0; i < uiManager.wallCategoryImages.Length; i++)
        {
            if (uiManager.wallCategoryImages[i] != null)
                uiManager.wallCategoryImages[i].gameObject.SetActive(false);
        }

        // ... continue with starting the game panel ...
        uiManager.opGamePanel.SetActive(true);
        if (bgSystem != null) yield return bgSystem.TriggerBackAll();
        uiManager.wallGamePanel.SetActive(true);

        ShowQuestion();

        yield return StartCoroutine(uiManager.FadeCanvasGroup(uiManager.wallGamePanelCG, 0, 1, fadeSpeed));

        // --- FIX: Force Update Visuals AFTER Panel is Visible ---
        Debug.Log($"[DEBUG] Game Panel Active. Timer Remaining: {timerRemaining}");

        // 1. Ensure the circle bar knows the total time
        uiManager.InitializeVisualTimer(timerTotalDuration);

        // 2. Force the text/fill to update immediately
        uiManager.UpdateTimerDisplay(timerRemaining);

        // // 3. Auto-Start Logic
        // if (timerRemaining > 0)
        // {
        //     isTimerRunning = true;
        //     uiManager.UpdateTimerStatusUI(true);
        //     Debug.Log("[DEBUG] Timer Started Automatically.");
        // }
        // else
        // {
        //     Debug.LogWarning("[DEBUG] Timer did not start because timerRemaining is 0.");
        // }

        isBusy = false;
        SaveProgress();
    }

    void ShowQuestion()
{
    if (currentCategory != null && currentQIndex < currentCategory.questions.Count)
    {
        // 1. Reset the time values
        timerRemaining = timerTotalDuration;
        uiManager.UpdateTimerDisplay(timerRemaining);

        // 2. AUTOMATICALLY START THE TIMER
        isTimerRunning = true; 
        
        // 3. Update the Operator UI button to show "PAUSE" (Red)
        uiManager.UpdateTimerStatusUI(true);

        // 4. Update the text
        string qText = currentCategory.questions[currentQIndex].text;
        uiManager.UpdateScoreUI(qText, score);
    }
    else
    {
        // If game is over, stop the timer
        isTimerRunning = false;
        uiManager.UpdateTimerStatusUI(false);
        EndGame();
    }
}   

    public void OnClick_Answer(bool isYes)
    {
        if (isBusy) return;
        bool correctAnswer = currentCategory.questions[currentQIndex].isYes;
        bool isUserCorrect = (isYes == correctAnswer);
        answerHistory.Add(isUserCorrect);
        uiManager.SetProgressBar(currentQIndex, isUserCorrect);
        if (isUserCorrect) score++;
        currentQIndex++;
        SaveProgress();
        ShowQuestion();
    }

    public void OnClick_Undo()
    {
        if (isBusy || currentQIndex <= 0) return;
        currentQIndex--;
        bool lastWasCorrect = answerHistory[currentQIndex];
        if (lastWasCorrect) score--;
        answerHistory.RemoveAt(currentQIndex);
        uiManager.wallProgressBars[currentQIndex].sprite = uiManager.iconDefault;
        uiManager.wallProgressBars[currentQIndex].color = Color.white;
        SaveProgress();
        ShowQuestion();
    }

    void EndGame()
    {
        uiManager.opQuestionText.text = "Game Over. Total Score: " + score;
        uiManager.wallQuestionText.text = "";
        uiManager.wallScoreText.text = score.ToString();
        dataManager.ClearSave();
    }

    public void OnClick_RestartGame()
    {
        // Stop everything
        StopAllCoroutines();
        isBusy = false;
        isTimerRunning = false;

        // Clear saved data
        dataManager.ClearSave();

        // Reload the current scene completely
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void OnClick_ExitGame()
    {
        Debug.Log("Exiting Game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- SAVE / LOAD ---
    void SaveProgress()
    {
        // GameSaveData data = new GameSaveData();
        // data.categoryIndex = currentCategoryIndex;
        // data.questionIndex = currentQIndex;
        // data.score = score;
        // data.timerRemaining = timerRemaining;
        // data.timerTotal = timerTotalDuration; // Save Max Time too!
        // data.answerHistory = answerHistory;
        // dataManager.SaveGame(data);
    }

    void RestoreGame()
    {
        // GameSaveData data = dataManager.LoadGame();
        // if (data == null) return;

        // loadedCategories = dataManager.LoadCategoriesFromDisk();
        // if (data.categoryIndex >= loadedCategories.Count) { dataManager.ClearSave(); return; }

        // currentCategoryIndex = data.categoryIndex;
        // currentCategory = loadedCategories[currentCategoryIndex];
        // currentQIndex = data.questionIndex;
        // score = data.score;
        // timerRemaining = data.timerRemaining;
        // timerTotalDuration = data.timerTotal; // Restore Max Time
        // answerHistory = data.answerHistory;

        // uiManager.ResetVisibility();
        // uiManager.opSelectionPanel.SetActive(false);
        // uiManager.opGamePanel.SetActive(true);
        // uiManager.wallSelectionPanel.SetActive(false);
        // uiManager.wallGamePanel.SetActive(true);
        // if(uiManager.wallGamePanelCG) uiManager.wallGamePanelCG.alpha = 1;

        // uiManager.ResetProgressBars();
        // for(int i=0; i<answerHistory.Count; i++) uiManager.SetProgressBar(i, answerHistory[i]);

        // // Init Visual Timer on Load
        // uiManager.InitializeVisualTimer(timerTotalDuration);
        // uiManager.UpdateTimerDisplay(timerRemaining);

        // isTimerRunning = false;
        // uiManager.UpdateTimerStatusUI(false);
        // ShowQuestion();
    }
}