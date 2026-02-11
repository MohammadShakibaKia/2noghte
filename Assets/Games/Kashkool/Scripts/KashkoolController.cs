using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq; // Needed to limit files to 6
using TMPro;

public class KashkoolController : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Kashkool"; // Folder inside StreamingAssets

    [Header("--- OPERATOR PANEL (Display 1) ---")]
    public GameObject opSetupPanel;       // The "Start" screen
    public GameObject opSelectionPanel;   // The screen with 6 Buttons
    public GameObject opGamePanel;        // The screen with Question & Yes/No
    
    public TMPro.TextMeshPro opQuestionText;
    public TMPro.TextMeshPro opScoreText;
    public Button[] opCategoryButtons;    // Drag your 6 Buttons here

    [Header("--- WALL PANEL (Display 2) ---")]
    public GameObject wallSelectionPanel; // The screen with 6 Images
    public GameObject wallGamePanel;      // The screen with Question & Progress
    
    public Image[] wallCategoryImages;    // Drag your 6 UI Images here
    public TMPro.TextMeshPro wallQuestionText;
    public TMPro.TextMeshPro wallScoreText;
    public Image[] wallProgressBars;      // Drag your 10 Progress Squares here

    // -- Internal Data --
    private List<CategoryData> loadedCategories = new List<CategoryData>();
    private CategoryData currentCategory;
    private int currentQIndex;
    private int score;

    void Start()
    {
        // 1. Activate the Projector/Wall Screen
        if (Display.displays.Length > 1) 
          //  Display.displays[1].Activate();

        // 2. Set Initial State
        opSetupPanel.SetActive(true);
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(false);
        
        wallSelectionPanel.SetActive(true);
        wallGamePanel.SetActive(false);

        // 3. Clear Wall Images (Optional)
        foreach(var img in wallCategoryImages) img.color = Color.clear; 
    }

    // -----------------------------------------------------------
    // PART 1: LOADING DATA & IMAGES (The "Reveal" Button)
    // -----------------------------------------------------------
    public void OnClick_RevealCategories()
    {
        Debug.Log("Reveal Categories Clicked!");
        loadedCategories.Clear();
        
        string path = Path.Combine(Application.streamingAssetsPath, folderName);
        
        if (!Directory.Exists(path)) 
        {
            Debug.LogError("Folder not found: " + path);
            opQuestionText.text = "Error: Folder Missing!";
            return;
        }

        // Get the first 6 JSON files
        var files = Directory.GetFiles(path, "*.json").Take(6).ToArray();

        // Switch UI
        opSetupPanel.SetActive(false);
        opSelectionPanel.SetActive(true);

        for (int i = 0; i < 6; i++)
        {
            if (i < files.Length)
            {
                // A. Read JSON
                string jsonContent = File.ReadAllText(files[i]);
                CategoryData catData = JsonUtility.FromJson<CategoryData>(jsonContent);
                loadedCategories.Add(catData);

                // B. Load Image for Wall
                // This combines the folder path + the filename from JSON
                string imagePath = Path.Combine(path, catData.iconFileName);
                LoadImageFromFile(imagePath, wallCategoryImages[i]);

                // C. Setup Operator Button
                opCategoryButtons[i].gameObject.SetActive(true);
                // This looks for the text inside the button regardless of if it's Text or TMP
                //opCategoryButtons[i].GetComponentInChildren<TMPro.TextMeshPro>().text = catData.categoryName;
                
                // D. Link Button Click
                int index = i; // Create local copy for the lambda
                opCategoryButtons[i].onClick.RemoveAllListeners();
                opCategoryButtons[i].onClick.AddListener(() => OnClick_SelectCategory(index));
            }
            else
            {
                // Hide unused slots
                opCategoryButtons[i].gameObject.SetActive(false);
                wallCategoryImages[i].gameObject.SetActive(false);
            }
        }
    }

    // --- THE IMAGE LOADING FUNCTION ---
    void LoadImageFromFile(string fullPath, Image targetImage)
    {
        if (File.Exists(fullPath))
        {
            // 1. Read Bytes
            byte[] fileData = File.ReadAllBytes(fullPath);
            
            // 2. Create Texture
            Texture2D tex = new Texture2D(2, 2);
            // LoadImage auto-resizes the texture dimensions
            if (tex.LoadImage(fileData)) 
            {
                // 3. Create Sprite
                // Rect(0,0,w,h) uses the whole image. Vector2(0.5,0.5) centers it.
                Sprite newSprite = Sprite.Create(tex, 
                    new Rect(0, 0, tex.width, tex.height), 
                    new Vector2(0.5f, 0.5f));

                // 4. Assign to UI
                targetImage.sprite = newSprite;
                targetImage.color = Color.white; // Make visible
                targetImage.preserveAspect = true; // Don't stretch!
                targetImage.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Image file missing: " + fullPath);
            targetImage.color = Color.gray; // Placeholder
        }
    }

    // -----------------------------------------------------------
    // PART 2: STARTING THE GAME
    // -----------------------------------------------------------
    void OnClick_SelectCategory(int index)
    {
        currentCategory = loadedCategories[index];
        
        // Reset Logic
        currentQIndex = 0;
        score = 0;

        // Switch Panels
        opSelectionPanel.SetActive(false);
        opGamePanel.SetActive(true);
        wallSelectionPanel.SetActive(false);
        wallGamePanel.SetActive(true);

        // Reset Progress Bars to White
        foreach (var bar in wallProgressBars) bar.color = Color.white;

        ShowQuestion();
    }

    // -----------------------------------------------------------
    // PART 3: GAME LOOP
    // -----------------------------------------------------------
    void ShowQuestion()
    {
        if (currentQIndex < currentCategory.questions.Count)
        {
            string qText = currentCategory.questions[currentQIndex].text;
            
            // Update both screens
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
        bool correctAnswer = currentCategory.questions[currentQIndex].isYes;
        bool isUserCorrect = (isYes == correctAnswer);

        // Color the specific bar square
        if (currentQIndex < wallProgressBars.Length)
        {
            wallProgressBars[currentQIndex].color = isUserCorrect ? Color.green : Color.red;
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
}