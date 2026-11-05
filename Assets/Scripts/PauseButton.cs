using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [Header("Настройки уровня")]
    public GameObject levelMenu;

    private bool isPaused = false;

    void Start()
    {
        if (levelMenu != null) 
            levelMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (levelMenu != null)
            levelMenu.SetActive(isPaused);
    }

    public void OnResumeClicked()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (levelMenu != null)
            levelMenu.SetActive(false);
    }
}