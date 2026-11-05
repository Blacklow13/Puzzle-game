using System;
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
    public GameObject gameEndingPanel;

    [Header("Сцены")]
    public string levelsMenuSceneName = "Menu";

    [Header("Настройки уровня")]
    public int levelNumber = 1;
    public int maxLevel = 7;
    public bool levelFinished = false;

    [Header("Ссылка на уровень")]
    public LevelManager levelManager;

    [Header("Ссылка на Тутор")]
    public TutorialManager tutorialManager;

    void Start()
    {
        if (accountPanel != null) accountPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(false);
        if (gameEndingPanel != null) gameEndingPanel.SetActive(false);
    }

    public void OnAccountClicked()
    {
        if (accountPanel != null)
        {
            accountPanel.SetActive(true);
            gameObject.SetActive(false);
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
            if (accountPanel != null) accountPanel.SetActive(false);
        }
    }

    public void OnSaveClicked()
    {
        SaveProgress();
    }

    public void OnExitClicked()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToNextLevel()
    {
        int nextLevel = levelNumber + 1;

        if (nextLevel > maxLevel)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelsMenuSceneName);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Level " + nextLevel);
    }

    public void OpenFinishScreen()
    {
        levelFinished = true;
        SaveProgress();
        Time.timeScale = 0f;

        if (accountPanel != null) accountPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(true);
    }

    public void OpenGameEndingScreen()
    {
        levelFinished = true;
        Time.timeScale = 0f;

        if (accountPanel != null) accountPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (finishPanel != null) finishPanel.SetActive(false);
        if (gameEndingPanel != null) gameEndingPanel.SetActive(true);
    }

    public bool IsFinishScreenOpen()
    {
        return finishPanel != null && finishPanel.activeSelf;
    }

    void SaveProgress()
    {
        ApiManager.Instance.SaveProgress(levelNumber);
        SaveLevelCompleted(levelNumber);
        Debug.Log($"[SAVE] Прогресс для уровня {levelNumber} сохранен локально.");
    }

    public void SaveLevelCompleted(int level)
    {
        PlayerPrefs.SetInt("Level_" + level + "_Completed", 1);

        int nextLevel = level + 1;
        if (nextLevel <= maxLevel)
        {
            PlayerPrefs.SetInt("Level_" + nextLevel + "_Unlocked", 1);
        }

        PlayerPrefs.Save();
        Debug.Log($"[PLAYERPREFS] Уровень {level} пройден. Разблокирован уровень {nextLevel}.");
    }
}