using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeferredLoader : MonoBehaviour
{
    private int? slot;
    private string saveName;
    private string targetScene;

    public void DefferedLoad(int? slot, string saveName, string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Sahne adı boş olamaz!");
            return;
        }
        this.slot = slot;
        this.saveName = saveName;
        this.targetScene = sceneName;
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(targetScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(WaitOneFrameThenLoad());
    }

    private IEnumerator WaitOneFrameThenLoad()
    {
        yield return null;
        SaveSystem.LoadGame(slot, saveName); // şimdi doğru dosya yüklenir
        Destroy(gameObject);
    }

}
