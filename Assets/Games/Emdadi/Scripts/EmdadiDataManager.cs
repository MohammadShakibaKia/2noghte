using UnityEngine;
using System.IO;

public class EmdadiDataManager : MonoBehaviour
{
    [Header("Configuration")]
    public string folderName = "Emdadi";
    public string jsonFileName = "EmdadiData.json";

    public EmdadiData LoadData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, folderName, jsonFileName);

        if (File.Exists(path))
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                return JsonUtility.FromJson<EmdadiData>(jsonContent);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Emdadi] JSON Error: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[Emdadi] File missing at: {path}");
        }

        return new EmdadiData();
    }
}