using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class KashkoolDataManager : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Kashkool";

    public List<CategoryData> LoadCategoriesFromDisk()
    {
        List<CategoryData> categories = new List<CategoryData>();
        string path = Path.Combine(Application.streamingAssetsPath, folderName);

        if (!Directory.Exists(path))
        {
            Debug.LogError($"[Kashkool] Missing Folder at: {path}");
            return categories;
        }

        // Load first 6 JSON files
        var files = Directory.GetFiles(path, "*.json").Take(6).ToArray();

        foreach (var file in files)
        {
            try
            {
                string jsonContent = File.ReadAllText(file);
                CategoryData catData = JsonUtility.FromJson<CategoryData>(jsonContent);
                catData.iconFileName = Path.Combine(path, catData.iconFileName); // Store path for later
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
}