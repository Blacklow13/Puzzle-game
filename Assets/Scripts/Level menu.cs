using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenu : MonoBehaviour
{
    [Header("Кнопки")]
    public UnityEngine.UI.Button accountButton;
    public UnityEngine.UI.Button levelsMenuButton;
    public UnityEngine.UI.Button soundButton;
    public UnityEngine.UI.Button saveButton;
    public UnityEngine.UI.Button exitButton;

    [Header("Панели")]
    public GameObject accountPanel;
    public GameObject soundPanel;
    public GameObject finishPanel;

    [Header("Сцены")]
    public string levelsMenuSceneName = "levelsMenu";

    [Header("Уровни")]
    public int maxLevel = 7;

    [Header("Ссылка на уровень")]
    public LevelManager levelManager;

    private int currentLevelNumber = 1;

    void Start()
    {


        if (accountPanel != null)
            accountPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(false);

        
    }

    public void OnAccountClicked()
    {
        if (accountPanel != null)
        {
            accountPanel.SetActive(true);

            if (soundPanel != null)
                soundPanel.SetActive(false);
        }
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelsMenuSceneName);
    }

    public void OnSoundClicked()
    {
        if (soundPanel != null)
        {
            soundPanel.SetActive(true);

            if (accountPanel != null)
                accountPanel.SetActive(false);
        }
    }

    public void OnSaveClicked()
    {
        SaveProgress();
    }

    public void OnExitClicked()
    {
        SaveProgress();

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToNextLevel()
    {   
        currentLevelNumber = GetLevelNumber(SceneManager.GetActiveScene().name);
        int nextLevel = currentLevelNumber + 1;

        if (nextLevel > maxLevel)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelsMenuSceneName);
            return;
        }

        SaveProgress();

        Time.timeScale = 1f;

        SceneManager.LoadScene("Level " + nextLevel);
    }

    public void OpenFinishScreen()
    {
        SaveProgress();

        Time.timeScale = 0f;

        if (accountPanel != null)
            accountPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        if (finishPanel != null)
            finishPanel.SetActive(true);
    }

    int GetLevelNumber(string sceneName)
    {
        string[] parts = sceneName.Split(' ');

        if (parts.Length >= 2)
        {
            int number;
            if (int.TryParse(parts[1], out number))
                return number;
        }

        return 1;
    }

    void SaveProgress()
    {
        Debug.Log("[SAVE] Прогресс сохранён");
    }
}