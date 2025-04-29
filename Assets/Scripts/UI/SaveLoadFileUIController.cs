using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveLoadFileUIController : MonoBehaviour
{

    [SerializeField] private GameObject loadBtnPrefab;
    [SerializeField] private Transform content;

    [SerializeField] private TMP_InputField saveFileInputField;
    

    private void OnEnable()
    {
        GetLoadFiles();
    }

    private void OnDisable()
    {
        ClearChilds();
    }

    private void GetLoadFiles()
    {
        List<string> files = SaveSystem.ListAllSaveFiles(false);
        if(files == null) return;
        if(files.Count == 0) return;
        
        foreach (var loadedFile in files)
        {
            GameObject loadBtn = Instantiate(loadBtnPrefab);
            loadBtn.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileNameWithoutExtension(loadedFile);
            loadBtn.GetComponent<Button>().onClick.AddListener(() =>
            {
                SaveSystem.LoadGame(null, loadedFile);
            });
            loadBtn.transform.parent = content;
        }
    }

    private void ClearChilds()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void NewSaveGame()
    {
        string saveFileName = saveFileInputField.text;
        if(String.IsNullOrEmpty(saveFileName)) return;
        SaveSystem.SaveGame(null,saveFileName);
        ClearChilds();
        GetLoadFiles();
    }
    
}
