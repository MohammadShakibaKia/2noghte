using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class KashkoolDataManager : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Kashkool";
    private const string SAVE_KEY = "KashkoolSaveData";

    public List<CategoryData> LoadCategoriesFromDisk()
    {
        List<CategoryData> categories = new List<CategoryData>();
        string path = Path.Combine(Application.streamingAssetsPath, folderName);

        if (!Directory.Exists(path))
        {
            Debug.LogError($"[Kashkool] Missing Folder at: {path}");
            return categories;
        }

        var files = Directory.GetFiles(path, "*.json").Take(6).ToArray();

        foreach (var file in files)
        {
            try
            {
                string jsonContent = File.ReadAllText(file);
                CategoryData catData = JsonUtility.FromJson<CategoryData>(jsonContent);
                catData.iconFileName = Path.Combine(path, catData.iconFileName); 
                categories.Add(catData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading JSON {file}: {e.Message}");
            }
        }

        return categories;
    }

    public Sprite LoadSpriteFromFile(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
        return null;
    }

    // --- SAVE SYSTEM ---

    public void SaveGame(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game Saved.");
    }

    public GameSaveData LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return null;
        
        string json = PlayerPrefs.GetString(SAVE_KEY);
        try 
        {
            return JsonUtility.FromJson<GameSaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}

// --- DATA STRUCTURES ---

[System.Serializable]
public class GameSaveData
{
    public int categoryIndex;
    public int questionIndex;
    public int score;
    public float timerRemaining;
    public List<bool> answerHistory;
}

[System.Serializable]
public class Question
{
    public string text;
    public bool isYes;
}

[System.Serializable]
public class CategoryData
{
    public string categoryName;
    public string iconFileName; 
    public List<Question> questions;
}