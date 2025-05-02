using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json; //   -> com.unity.nuget.newtonsoft-json
using System.IO;
using UnityEngine.SceneManagement;

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
            return Application.persistentDataPath + "/checkpoint.dat";
        }
    }

    private static string GetMetaKey(int? slot = null, string saveName = null)
    {
        if (!string.IsNullOrEmpty(saveName))
            return saveName;
        else if (slot.HasValue)
            return $"save_slot{slot.Value}";
        else
            return "checkpoint";
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
        System.IO.File.WriteAllBytes(filePath, binaryData);
        
        string sceneName = SceneManager.GetActiveScene().name;
        string metaName = GetMetaKey(slot, saveName);
        SaveMetaDataManager.WriteMetaData(metaName,sceneName, 0);
    }    
    
    public static void LoadGame(int? slot = null, string saveName = null)
    {
        string filePath = GetFilePath(slot, saveName);
        if (!System.IO.File.Exists(filePath)) return;

        byte[] binData = System.IO.File.ReadAllBytes(filePath);
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
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (wrapper.sceneName != currentScene)
        {
            //Change Scene!
            GameObject loaderObject = new GameObject("DeferredLoader");
            DeferredLoader loader = loaderObject.AddComponent<DeferredLoader>();
            loader.DefferedLoad(slot, saveName, wrapper.sceneName);
            return;
        }
        
        //Sahne aynı ise alt kısım çalışır!
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
    
    [Obsolete("ListAllSaveFiles() artık kullanılmıyor, yerine ListAllSaveMetadatas() fonksiyonunu kullan.")]
    public static List<string> ListAllSaveFiles(bool fullPath = false)
    {
        string directoryPath = Application.persistentDataPath;
        if (!System.IO.Directory.Exists(directoryPath))
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
        public string sceneName;
        public int sceneIndex;
        public List<SaveEntry> jsonData = new List<SaveEntry>();

        public SerializationWrapper()
        {
        }
        
        public SerializationWrapper(Dictionary<string, object> dictionary)
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
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
    
    public static class SaveMetaDataManager
    {
        public static string GetMetadataPath(string saveName)
        {
            return Path.Combine(Application.persistentDataPath, saveName + ".meta");
        }

        public static void WriteMetaData(string saveName, string sceneName, float playTime)
        {
            MetaData metaData = new MetaData(saveName, sceneName, playTime);
            string json = JsonConvert.SerializeObject(metaData, Formatting.Indented);
            File.WriteAllText(GetMetadataPath(saveName), json);
        }

        public static MetaData ReadMetaData(string saveName)
        {
            string path = GetMetadataPath(saveName);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<MetaData>(json);
        }
        
        public static List<MetaData> ListAllSaveMetaDatas()
        {
            string d_path = Application.persistentDataPath;

            if (!Directory.Exists(d_path))
            {
                Debug.Log("MetaData klasörü bulunamadı!");
                return new List<MetaData>();
            }

            var metaFiles = Directory.GetFiles(d_path, "*.meta");

            List<MetaData> metaDatas = new List<MetaData>();

            foreach (string metaFilePath in metaFiles)
            {
                try
                {
                    string json = File.ReadAllText(metaFilePath);
                    MetaData metaData = JsonConvert.DeserializeObject<MetaData>(json);
                    if (metaData != null)
                    {
                        string datFilePath = Path.Combine(d_path, metaData.saveName + ".dat");
                        if(!File.Exists(datFilePath)) continue; // meta dosyası var ancak dat dosyası yok o zaman listeye ekleme! TODO : Belki developer'a bilgi verilmeli!
                        metaDatas.Add(metaData);                    
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Meta dosyası okunamadı! {metaFilePath} -> {ex.Message}");
                }
            }

            return metaDatas
                .OrderByDescending(meta => meta.saveTimeRaw)
                .ToList();
        }
    }
}

[System.Serializable]
public class MetaData
{
    public string saveName;
    public string sceneName;
    [JsonIgnore] public DateTime saveTimeRaw;
    public string saveTime;
    public float playTimeInSeconds;

    public MetaData(){}
        
    public MetaData(string saveName, string sceneName, float playTimeInSeconds)
    {
        this.saveName = saveName;
        this.sceneName = sceneName;
        this.playTimeInSeconds = playTimeInSeconds;
        saveTimeRaw = System.DateTime.Now;
        this.saveTime = saveTimeRaw.ToString("yyyy-MM-dd HH:mm:ss");
    }
        
}