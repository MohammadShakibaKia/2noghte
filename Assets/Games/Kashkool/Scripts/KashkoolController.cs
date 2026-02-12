using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using RTLTMPro;

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
    private int currentQIndex;
    private int score;

    void Awake()
    {
        dataManager = GetComponent<KashkoolDataManager>();
        uiManager = GetComponent<KashkoolUIManager>();
    }

    void Start()
    {
        uiManager.ResetVisibility();
    }

    // --- REVEAL TRIGGER ---
    public void OnClick_RevealCategories()
    {
        if (isBusy) return;
        StartCoroutine(RevealSequenceRoutine());
    }

 IEnumerator RevealSequenceRoutine()
{
    isBusy = true;

    // 1. SAFETY: Wait for BG Animation
    if (bgSystem != null && bgSystem.IsAnimating)
    {
        yield return new WaitUntil(() => !bgSystem.IsAnimating);
    }

    // 2. Trigger Transition
    bgSystem.TriggerForward();

    // 3. Load Data
    loadedCategories = dataManager.LoadCategoriesFromDisk();
    if (loadedCategories.Count == 0) 
    { 
        Debug.LogError("No categories loaded!");
        isBusy = false; 
        yield break; 
    }

    // 4. Setup BOTH Wall and Operator UI
    for (int i = 0; i < loadedCategories.Count; i++)
    {
        Sprite catSprite = dataManager.LoadSpriteFromFile(loadedCategories[i].iconFileName);
        string catName = loadedCategories[i].categoryName;

        // --- A. SETUP WALL UI ---
        if (i < uiManager.wallCategoryImages.Length)
        {
            Image wallImg = uiManager.wallCategoryImages[i];
            wallImg.sprite = catSprite;
            wallImg.preserveAspect = true;

            Transform titleT = wallImg.transform.Find("Title");
            if (titleT != null)
            {
                Transform rtlT = titleT.Find("Text - RTLTMP");
                if (rtlT != null) rtlT.GetComponent<RTLTextMeshPro>().text = catName;
            }
        }

        // --- B. SETUP OPERATOR UI (UPDATED & ROBUST) ---
        if (i < uiManager.opCategoryButtons.Length)
        {
            Button opBtn = uiManager.opCategoryButtons[i];

            if (opBtn != null)
            {
                // 1. Set Image (Try component on Button, then check children)
                Image opImg = opBtn.GetComponent<Image>();
                if (opImg == null) opImg = opBtn.GetComponentInChildren<Image>(true); // Fallback to child

                if (opImg != null)
                {
                    opImg.sprite = catSprite;
                    opImg.preserveAspect = true;
                }
                else
                {
                    Debug.LogWarning($"Op Button {i} has no Image component!");
                }

                // 2. Set Text (Scans ALL children, ignores hierarchy structure)
                RTLTextMeshPro opRtl = opBtn.GetComponentInChildren<RTLTextMeshPro>(true);
                if (opRtl != null)
                {
                    opRtl.text = catName;
                }
                else
                {
                    Debug.LogWarning($"Op Button {i} has no RTLTextMeshPro component in children!");
                }

                // 3. Setup Button Interaction
                int index = i;
                opBtn.onClick.RemoveAllListeners();
                opBtn.onClick.AddListener(() => OnClick_SelectCategory(index));
                
                opBtn.gameObject.SetActive(true);
            }
        }
    }

    // 5. WAIT & FADE
    yield return new WaitUntil(() => !bgSystem.IsAnimating);
    yield return new WaitForSeconds(revealStartDelay);

    uiManager.opSetupPanel.SetActive(false);
    uiManager.opSelectionPanel.SetActive(true);

    for (int i = 0; i < loadedCategories.Count; i++)
    {
        // Fade Wall
        if (i < uiManager.wallCategoryImages.Length)
        {
            uiManager.wallCategoryImages[i].gameObject.SetActive(true);
            CanvasGroup wallCG = uiManager.wallCategoryImages[i].GetComponent<CanvasGroup>();
            StartCoroutine(uiManager.FadeCanvasGroup(wallCG, 0, 1, fadeSpeed));
        }

        // Fade Operator
        if (i < uiManager.opCategoryButtons.Length)
        {
            CanvasGroup opCG = uiManager.opCategoryButtons[i].GetComponent<CanvasGroup>();
            if (opCG == null) opCG = uiManager.opCategoryButtons[i].gameObject.AddComponent<CanvasGroup>();
            
            // Force Alpha to 0 initially so we see the fade
            opCG.alpha = 0; 
            StartCoroutine(uiManager.FadeCanvasGroup(opCG, 0, 1, fadeSpeed));
        }

        yield return new WaitForSeconds(0.2f);
    }

    isBusy = false;
}

    // --- SELECT CATEGORY ---
    public void OnClick_SelectCategory(int index)
    {
        if (isBusy) return;

        currentCategory = loadedCategories[index];
        currentQIndex = 0;
        score = 0;

        StartCoroutine(EnterGameModeRoutine(index));
    }

    IEnumerator EnterGameModeRoutine(int selectedIndex)
    {
        isBusy = true;

        List<Coroutine> fadeRoutines = new List<Coroutine>();

        // 1. Fade out UNSELECTED (Both Wall and Op)
        for (int i = 0; i < loadedCategories.Count; i++)
        {
            if (i == selectedIndex) continue;

            // Wall Fade Out
            CanvasGroup wCG = uiManager.wallCategoryImages[i].GetComponent<CanvasGroup>();
            if (wCG != null) fadeRoutines.Add(StartCoroutine(uiManager.FadeCanvasGroup(wCG, wCG.alpha, 0, fadeSpeed * 3f)));

            // Op Fade Out
            CanvasGroup oCG = uiManager.opCategoryButtons[i].GetComponent<CanvasGroup>();
            if (oCG != null) fadeRoutines.Add(StartCoroutine(uiManager.FadeCanvasGroup(oCG, oCG.alpha, 0, fadeSpeed * 3f)));
        }

        foreach (var r in fadeRoutines) yield return r;

        for (int i = 0; i < loadedCategories.Count; i++)
        {
            if (i != selectedIndex) 
            {
                uiManager.wallCategoryImages[i].gameObject.SetActive(false);
                uiManager.opCategoryButtons[i].gameObject.SetActive(false);
            }
        }

        // 2. Pause
        yield return new WaitForSeconds(1.0f);

        // 3. Fade out SELECTED
        CanvasGroup selWallCG = uiManager.wallCategoryImages[selectedIndex].GetComponent<CanvasGroup>();
        yield return StartCoroutine(uiManager.FadeCanvasGroup(selWallCG, 1, 0, fadeSpeed * 2f));
        uiManager.wallCategoryImages[selectedIndex].gameObject.SetActive(false);
        
        // Hide Op Button too
        uiManager.opCategoryButtons[selectedIndex].gameObject.SetActive(false);

        // 4. Switch Panels
        uiManager.opSelectionPanel.SetActive(false);
        uiManager.opGamePanel.SetActive(true);
        uiManager.wallSelectionPanel.SetActive(false);
        uiManager.ResetProgressBars();

        // 5. Back Animation
        if (bgSystem != null) yield return bgSystem.TriggerBackAll();

        // 6. Show Game Panel
        uiManager.wallGamePanel.SetActive(true);
        yield return StartCoroutine(uiManager.FadeCanvasGroup(uiManager.wallGamePanelCG, 0, 1, fadeSpeed));

        ShowQuestion();
        isBusy = false;
    }

    // --- GAME LOOP ---
    void ShowQuestion()
    {
        if (currentQIndex < currentCategory.questions.Count)
        {
            string qText = currentCategory.questions[currentQIndex].text;
            uiManager.UpdateScoreUI(qText, score);
        }
        else
        {
            EndGame();
        }
    }

    public void OnClick_Answer(bool isYes)
    {
        if (isBusy) return;

        bool correctAnswer = currentCategory.questions[currentQIndex].isYes;
        bool isUserCorrect = (isYes == correctAnswer);

        uiManager.SetProgressBar(currentQIndex, isUserCorrect);

        if (isUserCorrect) score++;
        currentQIndex++;
        ShowQuestion();
    }

    void EndGame()
    {
        uiManager.opQuestionText.text = "Game Over. Total Score: " + score;
        uiManager.wallQuestionText.text = "FINISHED";
        uiManager.wallScoreText.text = score.ToString();
    }

    public void OnClick_RestartGame()
    {
        StopAllCoroutines();
        isBusy = false;
        if (bgSystem != null) bgSystem.TriggerBackAll();
        uiManager.ResetVisibility();
        loadedCategories.Clear();
    }
}