using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class KashkoolController : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Kashkool";

    [Header("--- OPERATOR PANEL ---")]
    public GameObject opSetupPanel;      // Panel with "Reveal Categories" button
    public GameObject opSelectionPanel;  // Panel with 6 buttons to choose category
    public GameObject opGamePanel;       // Panel with Yes/No buttons
    public Text opQuestionText;
    public Button[] opCategoryButtons;   // Drag your 6 operator buttons here

    [Header("--- WALL PANEL ---")]
    public GameObject wallSelectionPanel; // Shows the 6 Squares
    public GameObject wallGamePanel;      // Shows the Question & Progress
    public Image[] wallCategoryImages;    // Drag the 6 Big Squares (Images) here
    public Text wallQuestionText;
    public Image[] wallProgressBars;      // Drag the 10 small squares here
    public Text wallScoreText;

    // Internal Data
    private List<CategoryData> loadedCategories = new List<CategoryData>();
    private List<Sprite> loadedIcons = new List<Sprite>();
    
    private CategoryData currentCategory;
    private int currentQIndex;
    private int score;

    void Start()
    {
        // Activate Wall Display
        if (Display.displays.Length > 1) Display.displays[1].Activate();

        // 1. Initial State
        opSetupPanel.SetActive(true);
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(false);
        
        wallSelectionPanel.SetActive(true);
        wallGamePanel.SetActive(false);

        // Clear Wall Images initially (optional)
        foreach (var img in wallCategoryImages) img.color = Color.gray; 
    }

    // --- STEP 1: LOAD AND REVEAL ---

    public void OnClick_RevealCategories()
    {
        LoadData();
        
        opSetupPanel.SetActive(false);
        opSelectionPanel.SetActive(true);

        // Update Wall Images and Operator Buttons
        for (int i = 0; i < 6; i++)
        {
            if (i < loadedCategories.Count)
            {
                // Set Wall Image
                wallCategoryImages[i].sprite = loadedIcons[i];
                wallCategoryImages[i].color = Color.white;

                // Set Operator Button Text & Action
                opCategoryButtons[i].gameObject.SetActive(true);
                opCategoryButtons[i].GetComponentInChildren<Text>().text = loadedCategories[i].categoryName;
                
                // Magic to assign button click to this specific category
                int index = i; 
                opCategoryButtons[i].onClick.RemoveAllListeners();
                opCategoryButtons[i].onClick.AddListener(() => OnClick_SelectCategory(index));
            }
            else
            {
                // Hide unused slots if less than 6 files found
                wallCategoryImages[i].gameObject.SetActive(false);
                opCategoryButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void LoadData()
    {
        loadedCategories.Clear();
        loadedIcons.Clear();

        string path = Path.Combine(Application.streamingAssetsPath, folderName);
        if (!Directory.Exists(path)) { Debug.LogError("No Folder!"); return; }

        var files = Directory.GetFiles(path, "*.json").Take(6);

        foreach (var file in files)
        {
            // 1. Load JSON
            string json = File.ReadAllText(file);
            CategoryData cat = JsonUtility.FromJson<CategoryData>(json);
            loadedCategories.Add(cat);

            // 2. Load Image
            string imagePath = Path.Combine(path, cat.iconFileName);
            Sprite icon = null;
            if (File.Exists(imagePath))
            {
                byte[] bytes = File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(bytes);
                icon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            loadedIcons.Add(icon);
        }
    }

    // --- STEP 2: SELECT CATEGORY & START GAME ---

    void OnClick_SelectCategory(int index)
    {
        currentCategory = loadedCategories[index];
        StartGame();
    }

    void StartGame()
    {
        // Switch UI to Game Mode
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(true);
        
        wallSelectionPanel.SetActive(false);
        wallGamePanel.SetActive(true);

        // Reset Game Variables
        currentQIndex = 0;
        score = 0;
        
        // Reset Progress Bar colors to white/gray
        foreach(var img in wallProgressBars) img.color = Color.white;

        ShowQuestion();
    }

    // --- STEP 3: GAMEPLAY LOOP ---

    void ShowQuestion()
    {
        if (currentQIndex < currentCategory.questions.Count)
        {
            string q = currentCategory.questions[currentQIndex].text;
            opQuestionText.text = q;
            wallQuestionText.text = q;
            wallScoreText.text = score.ToString();
        }
        else
        {
            EndGame();
        }
    }

    public void OnClick_Answer(bool operatorSaidYes)
    {
        bool correctAnswer = currentCategory.questions[currentQIndex].isYes;
        bool isCorrect = (operatorSaidYes == correctAnswer);

        // Update Progress Bar (Green for Correct, Red for Wrong)
        if (currentQIndex < wallProgressBars.Length)
        {
            wallProgressBars[currentQIndex].color = isCorrect ? Color.green : Color.red;
        }

        if (isCorrect) score++;

        currentQIndex++;
        ShowQuestion();
    }

    void EndGame()
    {
        wallQuestionText.text = "FINISHED";
        opQuestionText.text = "Game Over. Total Score: " + score;
        wallScoreText.text = score.ToString();
        
        // Optional: Disable Yes/No buttons here
    }
}