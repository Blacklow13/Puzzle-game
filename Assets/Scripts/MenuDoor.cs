using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuDoor : MonoBehaviour
{
    [Header("Настройки уровня")]
    public string levelSceneName = "Level 1";
    public int levelNumber = 1;

    [Header("Ссылки")]
    public string playerTag = "Player";

    [Header("Визуал (Родитель и Ребенок)")]
    public SpriteRenderer parentOpenSprite;
    public GameObject childClosedObject;

    [Header("UI (опционально)")]
    public TMP_Text interactPrompt;
    public TMP_Text lockedPrompt;

    private bool isPlayerNear = false;
    private bool isUnlocked = false;

    void Start()
    {
        if (levelNumber == 1)
        {
            isUnlocked = true;
        }
        else
        {
            isUnlocked = PlayerPrefs.GetInt("Level_" + levelNumber + "_Unlocked", 0) == 1;
        }

        UpdateDoorVisual();

        if (interactPrompt != null) interactPrompt.gameObject.SetActive(false);
        if (lockedPrompt != null) lockedPrompt.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (isUnlocked)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(levelSceneName);
            }
            else
            {
                Debug.Log("Уровень заблокирован!");
            }
        }
    }

    void UpdateDoorVisual()
    {
        if (parentOpenSprite != null)
        {
            parentOpenSprite.enabled = isUnlocked;
        }

        if (childClosedObject != null)
        {
            childClosedObject.SetActive(!isUnlocked);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = true;

            if (isUnlocked && interactPrompt != null)
                interactPrompt.gameObject.SetActive(true);
            else if (!isUnlocked && lockedPrompt != null)
                lockedPrompt.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false;
            if (interactPrompt != null) interactPrompt.gameObject.SetActive(false);
            if (lockedPrompt != null) lockedPrompt.gameObject.SetActive(false);
        }
    }
}