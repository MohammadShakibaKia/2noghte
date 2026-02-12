using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using RTLTMPro;
using System.Collections;

public class KashkoolController : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Kashkool";
    public float revealStartDelay = 2.0f;
    public float fadeSpeed = 1.5f;

    [Header("--- ASSETS ---")]
    public Sprite iconCorrect;
    public Sprite iconWrong;
    public Sprite iconDefault;

    [Header("--- OPERATOR PANEL ---")]
    public GameObject opSetupPanel;
    public GameObject opSelectionPanel;
    public GameObject opGamePanel;
    public RTLTextMeshPro opQuestionText;
    public RTLTextMeshPro opScoreText;
    public Button[] opCategoryButtons;

    [Header("--- WALL PANEL ---")]
    public GameObject wallSelectionPanel;
    public GameObject wallGamePanel;
    public CanvasGroup wallGamePanelCG;
    public Image[] wallCategoryImages;
    public RTLTextMeshPro wallQuestionText;
    public RTLTextMeshPro wallScoreText;
    public Image[] wallProgressBars;

    public StackableAnimation bgSystem;

    // Safety state to prevent fast-click issues
    private bool isBusy = false;
    private List<CategoryData> loadedCategories = new List<CategoryData>();
    private CategoryData currentCategory;
    private int currentQIndex;
    private int score;

    void Start()
    {
        ResetUIVisibility();
    }

    void ResetUIVisibility()
    {
        opSetupPanel.SetActive(true);
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(false);
        wallSelectionPanel.SetActive(true);
        wallGamePanel.SetActive(false);
        if (wallGamePanelCG) wallGamePanelCG.alpha = 0;

        foreach (var img in wallCategoryImages)
        {
            CanvasGroup cg = img.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0;
            img.gameObject.SetActive(false);
        }
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

        // 1. Start Background Animation
        bgSystem.TriggerForward();

        // 2. Load Data (While animation plays)
        string path = Path.Combine(Application.streamingAssetsPath, folderName);
        if (!Directory.Exists(path)) { Debug.LogError("Missing Folder"); isBusy = false; yield break; }

        var files = Directory.GetFiles(path, "*.json").Take(6).ToArray();
        loadedCategories.Clear();

        for (int i = 0; i < files.Length; i++)
        {
            string jsonContent = File.ReadAllText(files[i]);
            CategoryData catData = JsonUtility.FromJson<CategoryData>(jsonContent);
            loadedCategories.Add(catData);

            LoadImageFromFile(Path.Combine(path, catData.iconFileName), wallCategoryImages[i]);

            // Set RTL Text
            Transform titleT = wallCategoryImages[i].transform.Find("Title");
            if (titleT != null)
            {
                Transform rtlT = titleT.Find("Text - RTLTMP");
                if (rtlT != null) rtlT.GetComponent<RTLTextMeshPro>().text = catData.categoryName;
            }

            // Set Buttons
            opCategoryButtons[i].gameObject.SetActive(true);
            int index = i;
            opCategoryButtons[i].onClick.RemoveAllListeners();
            opCategoryButtons[i].onClick.AddListener(() => OnClick_SelectCategory(index));
        }

        // 3. Wait for Background to reach "Revealed" state
        while (bgSystem.IsAnimating) yield return null;
        yield return new WaitForSeconds(revealStartDelay);

        // 4. Fade In Categories
        opSetupPanel.SetActive(false);
        opSelectionPanel.SetActive(true);

        for (int i = 0; i < loadedCategories.Count; i++)
        {
            wallCategoryImages[i].gameObject.SetActive(true);
            CanvasGroup cg = wallCategoryImages[i].GetComponent<CanvasGroup>();
            while (cg.alpha < 1.0f) { cg.alpha += Time.deltaTime * fadeSpeed; yield return null; }
            yield return new WaitForSeconds(0.2f); // Staggered reveal
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

        // Pass the index to handle specific icon logic
        StartCoroutine(EnterGameModeRoutine(index));
    }

    IEnumerator EnterGameModeRoutine(int selectedIndex)
    {
        isBusy = true;

        // 1. Fade out ALL UNSELECTED categories SIMULTANEOUSLY
        bool anyStillFading = true;
        while (anyStillFading)
        {
            anyStillFading = false;
            for (int i = 0; i < loadedCategories.Count; i++)
            {
                if (i == selectedIndex) continue; // Leave the selected one alone

                CanvasGroup cg = wallCategoryImages[i].GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha > 0)
                {
                    cg.alpha -= Time.deltaTime * fadeSpeed * 3f; // Fast simultaneous fade
                    if (cg.alpha > 0) anyStillFading = true;
                }
            }
            yield return null;
        }

        // Ensure all unselected are fully hidden
        for (int i = 0; i < loadedCategories.Count; i++)
        {
            if (i != selectedIndex) wallCategoryImages[i].gameObject.SetActive(false);
        }

        // 2. PAUSE: Keep the selected category visible for 1 second
        yield return new WaitForSeconds(1.0f);

        // 3. Fade out the SELECTED category
        CanvasGroup selectedCG = wallCategoryImages[selectedIndex].GetComponent<CanvasGroup>();
        if (selectedCG != null)
        {
            while (selectedCG.alpha > 0)
            {
                selectedCG.alpha -= Time.deltaTime * fadeSpeed * 2f;
                yield return null;
            }
            wallCategoryImages[selectedIndex].gameObject.SetActive(false);
        }

        // 4. Switch UI and start background move
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(true);
        wallSelectionPanel.SetActive(false);

        // Reset Bars
        foreach (var bar in wallProgressBars) { bar.sprite = iconDefault; bar.color = Color.white; }

        // 5. Wait for Background Animation to reverse back to Intro 2
        if (bgSystem != null)
        {
            yield return bgSystem.TriggerBackAll();
        }

        // 6. Show Game Panel
        wallGamePanel.SetActive(true);
        if (wallGamePanelCG)
        {
            while (wallGamePanelCG.alpha < 1.0f)
            {
                wallGamePanelCG.alpha += Time.deltaTime * fadeSpeed;
                yield return null;
            }
        }

        ShowQuestion();
        isBusy = false;
    }

    // --- GAME LOOP ---
    void ShowQuestion()
    {
        if (currentQIndex < currentCategory.questions.Count)
        {
            string qText = currentCategory.questions[currentQIndex].text;
            opQuestionText.text = qText;
            opScoreText.text = "Score: " + score;
            wallQuestionText.text = qText;
            wallScoreText.text = score.ToString();
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

        if (currentQIndex < wallProgressBars.Length)
        {
            wallProgressBars[currentQIndex].sprite = isUserCorrect ? iconCorrect : iconWrong;
            wallProgressBars[currentQIndex].color = Color.white;
        }

        if (isUserCorrect) score++;
        currentQIndex++;
        ShowQuestion();
    }

    void EndGame()
    {
        opQuestionText.text = "Game Over. Total Score: " + score;
        wallQuestionText.text = "FINISHED";
        wallScoreText.text = score.ToString();
    }

    public void OnClick_RestartGame()
    {
        StopAllCoroutines();
        isBusy = false;

        bgSystem.TriggerBackAll();
        ResetUIVisibility();
        loadedCategories.Clear();
    }

    void LoadImageFromFile(string fullPath, Image targetImage)
    {
        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                targetImage.sprite = newSprite;
                targetImage.preserveAspect = true;
            }
        }
    }
}