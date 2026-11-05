using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Обучения")]
    public GameObject tutorialPanel;
    public GameObject parentImage;
    public GameObject childImage;

    private int currentPage = 0;
    private int totalPages = 2;

    public int levelNumber = 7;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (levelNumber == 1)
            {
                OpenTutorial();
            }
        } 
        else Destroy(gameObject);
    }

    void LateUpdate()
    {
        // Ловим Esc именно в LateUpdate, чтобы LevelManager не успел открыть паузу
        if (tutorialPanel != null && tutorialPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTutorial();
        }
    }

    public void OpenTutorial()
    {
        currentPage = 0;

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialPanel.transform.SetAsLastSibling(); 
        }

        ShowPage();
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void NextPage()
    {
        currentPage++;

        if (currentPage < totalPages)
        {
            ShowPage();
        }
        else
        {
            CloseTutorial();
        }
    }

    void ShowPage()
    {
        if (parentImage != null) parentImage.SetActive(true);
        if (childImage != null) childImage.SetActive(currentPage >= 1);
    }
}