using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json; //   -> com.unity.nuget.newtonsoft-json
using System.IO;

[System.Serializable]
public class SaveEntry
{
    public string key;
    public object data;
}

public class SaveSystem : MonoBehaviour
{

    private static string GetFilePath(int? slot = null, string saveName = null)
    {
        if (!string.IsNullOrEmpty(saveName))
        {
            return Application.persistentDataPath + "/" + saveName + ".dat";
        }else if (slot.HasValue)
        {
            return Application.persistentDataPath + "/save_slot" + slot.Value + ".dat";
        }
        else
        {
            return Application.persistentDataPath + "/save.dat";
        }
    }
    
    public static void SaveGame(int? slot = null, string saveName = null)
    {
        var saveables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        Dictionary<string, object> stateDict = new Dictionary<string, object>();
        foreach (var saveable in saveables)
        {
            stateDict[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
        }
        
        SerializationWrapper wrapper = new SerializationWrapper(stateDict);
        
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented, settings);
        byte[] binaryData = System.Text.Encoding.UTF8.GetBytes(json);
        string filePath = GetFilePath(slot, saveName);
        UnityEngine.Windows.File.WriteAllBytes(filePath, binaryData);
    }    
    
    public static void LoadGame(int? slot = null, string saveName = null)
    {
        string filePath = GetFilePath(slot, saveName);

        if (!UnityEngine.Windows.File.Exists(filePath)) return;

        byte[] binData = UnityEngine.Windows.File.ReadAllBytes(filePath);
        string json = Encoding.UTF8.GetString(binData);

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        SerializationWrapper wrapper = JsonConvert.DeserializeObject<SerializationWrapper>(json, settings);
        
        if (wrapper == null || wrapper.jsonData == null)
        {
            Debug.LogError("Kayıtlı data yüklenemedi. Veri bozuk olabilir..");
            return;
        }
        var saveables = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            string id = saveable.GetUniqueIdentifier();
            var matchedEntry = wrapper.jsonData.FirstOrDefault(entry => entry.key == id);

            if (matchedEntry != null && matchedEntry.data != null)
            {
                saveable.RestoreState(matchedEntry.data);
            }
        }
    }

    public static List<string> ListAllSaveFiles(bool fullPath = false)
    {
        string directoryPath = Application.persistentDataPath;
        if (!UnityEngine.Windows.Directory.Exists(directoryPath))
        {
            Debug.Log("Kayıt klasörü bulunamadı!");
            return new List<string>();
        }

        var files = System.IO.Directory.GetFiles(directoryPath, "*.dat");

        if (!files.Any())
        {
            Debug.Log("Kayıt dosyası bulunamadı.");
            return new List<string>();
        }
        
        var sortedFiles = files.OrderByDescending(f => System.IO.File.GetLastWriteTime(f));
        
        List<string> saveFileNames = new List<string>();
        
        foreach (var file in sortedFiles)
        {
            if (fullPath)
            {
                saveFileNames.Add(file);
            }
            else
            {
                saveFileNames.Add(Path.GetFileNameWithoutExtension(file)); // sadece dosya adı
            }
        }
        
        return saveFileNames;
    }
    
    [System.Serializable]
    private class SerializationWrapper
    {

        public List<SaveEntry> jsonData = new List<SaveEntry>();

        public SerializationWrapper()
        {
        }
        
        public SerializationWrapper(Dictionary<string, object> dictionary)
        {
            foreach (var pair in dictionary)
            {
                SaveEntry entry = new SaveEntry
                {
                    key = pair.Key,
                    data = pair.Value
                };
                jsonData.Add(entry);
            }
        }
    }
    
}
