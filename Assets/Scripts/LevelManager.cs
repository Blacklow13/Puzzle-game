using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public ClonePlayer Clone;
    public PlayerController Player;

    [Header("UI")]
    public TMP_Text timerText;

    [System.Serializable]
    public struct ActionData
    {
        public float time;
        public string buttonName;
    }

    public List<ActionData> records = new List<ActionData>();

    [Header("Настройки уровня")]
    public float levelTime = 20f;

    private float startTime = 0f;
    private bool cloneSpawned = false;
    private bool recordingStopped = false;

    void Start()
    {
        if (Clone != null)
            Clone.gameObject.SetActive(false);

        startTime = Time.time;
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
        if (cloneSpawned) return;

        float t = Time.time - startTime;

        // === ГОРИЗОНТАЛЬ ===
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
                AddRecord("LeftStop", t);

            AddRecord("RightStart", t);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.D))
                AddRecord("RightStop", t);

            AddRecord("LeftStart", t);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            if (!Input.GetKey(KeyCode.A))
                AddRecord("RightStop", t);
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            if (!Input.GetKey(KeyCode.D))
                AddRecord("LeftStop", t);
        }

        // === ВЕРТИКАЛЬ ===
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.S))
                AddRecord("DownStop", t);

            AddRecord("UpStart", t);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.W))
                AddRecord("UpStop", t);

            AddRecord("DownStart", t);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            if (!Input.GetKey(KeyCode.S))
                AddRecord("UpStop", t);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            if (!Input.GetKey(KeyCode.W))
                AddRecord("DownStop", t);
        }

        UpdateTimerUI();

        if (t >= levelTime)
            SpawnClone();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        float t = Time.time - startTime;
        float remaining = Mathf.Max(0f, levelTime - t);
        timerText.text = $"Время: {remaining:F1}";
    }

    void SpawnClone()
{
    float t = levelTime;

    // Добавляем Stop для ВСЕХ зажатых клавиш
    if (Input.GetKey(KeyCode.W))
        AddRecord("UpStop", t);

    if (Input.GetKey(KeyCode.S))
        AddRecord("DownStop", t);

    if (Input.GetKey(KeyCode.A))
        AddRecord("LeftStop", t);

    if (Input.GetKey(KeyCode.D))
        AddRecord("RightStop", t);

    cloneSpawned = true;
    recordingStopped = true;

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
}