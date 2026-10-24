using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishMenu : MonoBehaviour
{
    [Header("Кнопки")]
    public UnityEngine.UI.Button accountButton;
    public UnityEngine.UI.Button levelsMenuButton;
    public UnityEngine.UI.Button soundButton;
    public UnityEngine.UI.Button saveButton;
    public UnityEngine.UI.Button nextLevelButton;
    public UnityEngine.UI.Button exitButton;

    [Header("Панели")]
    public GameObject accountPanel;
    public GameObject soundPanel;

    [Header("Сцены")]
    public string levelsMenuSceneName = "levelsMenu";
    public string[] levelScenes = { "Level1", "Level2", "Level3", "Level4", "Level5", "Level6", "Level7" };

    [Header("Ссылки")]
    public LevelManager levelManager;

    private int currentLevelIndex = 0;

    void Start()
    {
        accountButton.onClick.AddListener(OnAccountClicked);
        levelsMenuButton.onClick.AddListener(OnMainMenuClicked);
        soundButton.onClick.AddListener(OnSoundClicked);
        saveButton.onClick.AddListener(OnSaveClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        if (accountPanel != null)
            accountPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        // Текущий уровень по названию сцены
        string currentScene = SceneManager.GetActiveScene().name;

        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == currentScene)
            {
                currentLevelIndex = i;
                break;
            }
        }
    }

    // === Аккаунт ===
    void OnAccountClicked()
    {
        if (accountPanel != null)
        {
            accountPanel.SetActive(true);

            if (soundPanel != null)
                soundPanel.SetActive(false);
        }
        // Заглушка
        Debug.Log("[FINISH MENU] Открыт раздел «Аккаунт»");
    }

    // === В меню выбора уровня ===
    void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        // Заглушка
        Debug.Log("[FINISH MENU] Переход в меню выбора уровня");

        SceneManager.LoadScene(levelsMenuSceneName);
    }

    // === Звук ===
    void OnSoundClicked()
    {
        if (soundPanel != null)
        {
            soundPanel.SetActive(true);

            if (accountPanel != null)
                accountPanel.SetActive(false);
        }
        // Заглушка
        Debug.Log("[FINISH MENU] Открыт раздел «Звук»");
    }

    // === Сохранение ===
    void OnSaveClicked()
    {
        SaveProgress();
    }

    // === Следующий уровень ===
    void OnNextLevelClicked()
    {
        int nextIndex = currentLevelIndex + 1;

        if (nextIndex >= levelScenes.Length)
        {
            // Заглушка
            Debug.Log("[FINISH MENU] Все уровни пройдены!");

            // Возврат в  меню выбора уровня
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelsMenuSceneName);
            return;
        }

        SaveProgress();

        Time.timeScale = 1f;

        string nextScene = levelScenes[nextIndex];
        Debug.Log($"[FINISH MENU] Переход на следующий уровень: {nextScene}");

        SceneManager.LoadScene(nextScene);
    }

    // === ВЫЙТИ ===
    void OnExitClicked()
    {
        SaveProgress();

        Time.timeScale = 1f;

        Debug.Log("[FINISH MENU] Сохранение + выход из игры");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // === СОХРАНЕНИЕ ===
    void SaveProgress()
    {
        // ЗАГЛУШКА — потом заменить на запрос к бекенду
        Debug.Log("[SAVE] Прогресс сохранён");

        // TODO: отправка на сервер
        // ApiClient.SaveProgress(...);
    }
}