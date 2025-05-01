using UnityEngine;

public class UIController : MonoBehaviour
{
    public void StopGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGAme()
    {
        Time.timeScale = 1;
    }

    public void CheckPointSave()
    {
        SaveSystem.SaveGame();
    }

    public void CheckPointLoad()
    {
        SaveSystem.LoadGame();
    }
}
