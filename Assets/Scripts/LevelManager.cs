using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public ClonePlayer Clone;
    public PlayerController Player;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject levelMenu;

    [Header("Состояние (Прошлое/Настоящее)")]
    public GameObject pastIcon;
    public GameObject presentIcon;
    public bool isPresentState = false;

    [System.Serializable]
    public struct ActionData
    {
        public float time;
        public string buttonName;
    }

    public List<ActionData> records = new List<ActionData>();

    [Header("Настройки уровня")]
    public float levelTime = 20f;

    [Header("Клавиша перезапуска")]
    public KeyCode restartKey = KeyCode.X;

    [Header("Столкновение игрока и клона")]
    public float collisionCheckDelay = 0.5f;

    private float startTime = 0f;
    private float cloneSpawnTime = 0f;
    private bool isGameOver = false;

    private bool playerStartMoving = false;
    private bool cloneSpawned = false;
    private bool recordingStopped = false;

    private bool isPaused = false;
    
    public bool isTyping = false; 
    public bool isTutorialOpen = false; // <--- ДОБАВЛЕНО

    void Start()
    {
        if (Clone != null)
            Clone.gameObject.SetActive(false);

        if (levelMenu != null)
            levelMenu.SetActive(false);

        
        UpdateStateUI();
        UpdateTimerUI();
    }

    void Awake()
    {
        Instance = this;
    }

    void AddRecord(string button, float time)
    {
        if (recordingStopped) return;

        ActionData data = new ActionData();
        data.time = time;
        data.buttonName = button;
        records.Add(data);
    }

    void UpdateStateUI()
    {
        if (pastIcon != null) pastIcon.SetActive(!isPresentState);
        if (presentIcon != null) presentIcon.SetActive(isPresentState);
    }

    void Update()
    {
         if (Input.GetKeyDown(KeyCode.Escape))
    {
        // Если тутор открыт — просто выходим. Esc закроет сам тутор в LateUpdate.
        if (TutorialManager.Instance != null && TutorialManager.Instance.tutorialPanel.activeSelf)
        {
            return;
        }

        TogglePause();
        return;
    }

    if (isPaused)
    {
        if (!isTyping && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        return;
    }

        if (isGameOver) return;

        if (Input.GetKeyDown(restartKey))
        {
            RestartLevel();
            return;
        }

        if (!cloneSpawned)
        {
            if (records.Count > 0 && !playerStartMoving)
            {
                playerStartMoving = true;
                startTime = Time.time;

                if (AudioManager.Instance != null)
                    AudioManager.Instance.StartClockTick();
            }

            float t = 0f;
            if (playerStartMoving)
                t = Time.time - startTime;

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.A)) AddRecord("LeftStop", t);
                AddRecord("RightStart", t);
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.D)) AddRecord("RightStop", t);
                AddRecord("LeftStart", t);
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                if (!Input.GetKey(KeyCode.A)) AddRecord("RightStop", t);
            }

            if (Input.GetKeyUp(KeyCode.A))
            {
                if (!Input.GetKey(KeyCode.D)) AddRecord("LeftStop", t);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.S)) AddRecord("DownStop", t);
                AddRecord("UpStart", t);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.W)) AddRecord("UpStop", t);
                AddRecord("DownStart", t);
            }

            if (Input.GetKeyUp(KeyCode.W))
            {
                if (!Input.GetKey(KeyCode.S)) AddRecord("UpStop", t);
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                if (!Input.GetKey(KeyCode.W)) AddRecord("DownStop", t);
            }

            UpdateTimerUI();

            if (t >= levelTime)
            {
                SpawnClone();
            }
        }

        if (Player.CheckIfPlayerIsInAir() || CheckPlayerInCloneCollision())
        {
            GameOver();
            return;
        }

        if (cloneSpawned && Clone.CheckIfCloneIsInAir())
        {
            GameOver();
            return;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        if (cloneSpawned) return;

        float t = 0f;
        if (playerStartMoving)
            t = Time.time - startTime;

        float remaining = Mathf.Max(0f, levelTime - t);
        timerText.text = $"Время: {remaining:F1}";
    }

    void SpawnClone()
    {
        float t = levelTime;

        if (Input.GetKey(KeyCode.W)) AddRecord("UpStop", t);
        if (Input.GetKey(KeyCode.S)) AddRecord("DownStop", t);
        if (Input.GetKey(KeyCode.A)) AddRecord("LeftStop", t);
        if (Input.GetKey(KeyCode.D)) AddRecord("RightStop", t);

        cloneSpawned = true;
        recordingStopped = true;
        cloneSpawnTime = Time.time;
        
        isPresentState = true;
        UpdateStateUI();

        if (timerText != null)
            timerText.text = "Время вышло";

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopClockTick();
            AudioManager.Instance.PlaySFX(AudioManager.Instance.changePhase);

        if (Clone == null)
        {
            return;
        }

        Clone.SetRecords(records);
        Clone.gameObject.SetActive(true);
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (Player != null) Player.Die();
        if (Clone != null && cloneSpawned) Clone.Die();

        RestartLevel();
    }

    bool CheckPlayerInCloneCollision()
    {
        if (Player == null || Clone == null) return false;
        if (!cloneSpawned) return false;
        if (Time.time - cloneSpawnTime < collisionCheckDelay) return false;

        Collider2D playerCol = Player.GetComponentInChildren<Collider2D>();
        Collider2D cloneCol = Clone.GetComponentInChildren<Collider2D>();

        if (playerCol == null || cloneCol == null) return false;

        ColliderDistance2D dist = Physics2D.Distance(playerCol, cloneCol);
        
        if (dist.isOverlapped || dist.distance < 0.05f)
        {
            return true;
        }

        return false;
    }

    public void TogglePauseAndStopSFX()
    {
        if (levelMenu != null)
        {
            LevelMenu menuScript = levelMenu.GetComponent<LevelMenu>();

            if (menuScript != null && !menuScript.levelFinished && !menuScript.IsFinishScreenOpen())
            {
                isPaused = !isPaused;
                Time.timeScale = isPaused ? 0f : 1f;

                if (AudioManager.Instance != null)
                {
                    if (isPaused)
                    {
                        AudioManager.Instance.PauseClockTick();
                        
                        if (AudioManager.Instance.sfxSource != null)
                            AudioManager.Instance.sfxSource.Stop();
                    }
                    else
                    {
                        AudioManager.Instance.ResumeClockTick();
                    }
                }
            }
        }
    }

    public void TogglePause()
    {
        if (levelMenu != null)
        {
            LevelMenu menuScript = levelMenu.GetComponent<LevelMenu>();

            if (menuScript != null && !menuScript.levelFinished && !menuScript.IsFinishScreenOpen())
            {
                TogglePauseAndStopSFX();
                levelMenu.SetActive(isPaused);
            }
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}