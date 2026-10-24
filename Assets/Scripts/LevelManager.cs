using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public ClonePlayer Clone;
    public PlayerController Player;

    [Header("UI")]
    public TMP_Text timerText;
    public GameObject levelMenu;

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

    void Start()
    {
        if (Clone != null)
            Clone.gameObject.SetActive(false);

        if (levelMenu != null)
            levelMenu.SetActive(false);

        UpdateTimerUI();
    }

    void AddRecord(string button, float time)
    {
        if (recordingStopped) return;

        ActionData data = new ActionData();
        data.time = time;
        data.buttonName = button;
        records.Add(data);
    }

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

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

        // Проверка на смерть 
        if (Player.CheckIfPlayerIsInAir() || CheckPlayerInCloneCollision())
        {
            Debug.Log("[GAMEOVER] Смерть");
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

        if (timerText != null)
            timerText.text = "Клон!";

        if (Clone == null)
        {
            Debug.LogError("Clone не назначен!");
            return;
        }

        Debug.Log($"[SPAWN] Записано {records.Count} действий");

        Clone.SetRecords(records);
        Clone.gameObject.SetActive(true);
    }

    void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Player.Die();
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

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (levelMenu != null)
            levelMenu.SetActive(isPaused);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}