using UnityEngine;
using System.Collections.Generic;

public class ClonePlayer : MonoBehaviour
{
    [Header("Ссылка на игрока")]
    public PlayerController player;

    private float speed;
    private float climbSpeed;
    private float centerOffsetY;
    private float bottomOffsetY;
    private float centerCheckRadius;
    private float bottomCheckRadius;
    private float alignSpeed;

    private LayerMask platformsLayer;
    private LayerMask stairsTopLayer;

    private Rigidbody2D rb;

    private readonly HashSet<Collider2D> platformsInRange = new HashSet<Collider2D>();

    public bool IsOnPlatform  = false;
    public bool IsOnStairs    = false;
    public bool IsOnStairsTop = false;

    private List<LevelManager.ActionData> records;
    private float startTime = 0f;
    private int index = 0;

    private Vector2 currentMove = Vector2.zero;

    private bool hasAligned     = false;
    private bool lockHorizontal = false;
    private bool wasOnlyStairs  = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player != null)
        {
            speed             = player.speed;
            climbSpeed        = player.climbSpeed;
            centerOffsetY     = player.centerOffsetY;
            bottomOffsetY     = player.bottomOffsetY;
            centerCheckRadius = player.centerCheckRadius;
            bottomCheckRadius = player.bottomCheckRadius;
            alignSpeed        = player.alignSpeed;
            platformsLayer    = player.platformsLayer;
            stairsTopLayer    = player.stairsTopLayer;
        }
        else
        {
            Debug.LogError("PlayerController не назначен!");
        }
    }

    public void SetRecords(List<LevelManager.ActionData> newRecords)
    {
        records = new List<LevelManager.ActionData>(newRecords);
        startTime = Time.time;
        index = 0;
        currentMove = Vector2.zero;
        hasAligned = false;
        lockHorizontal = false;
        wasOnlyStairs = false;
    }

    void PlayAction(LevelManager.ActionData data)
    {
        switch (data.buttonName)
        {
            case "LeftStart":  currentMove.x = -1f; break;
            case "LeftStop":   currentMove.x =  0f; break;
            case "RightStart": currentMove.x =  1f; break;
            case "RightStop":  currentMove.x =  0f; break;
            case "UpStart":    currentMove.y =  1f; break;
            case "UpStop":     currentMove.y =  0f; break;
            case "DownStart":  currentMove.y = -1f; break;
            case "DownStop":   currentMove.y =  0f; break;
        }
    }

    Vector2 GetCenterPoint()
    {
        return (Vector2)transform.position + new Vector2(0f, centerOffsetY);
    }

    Vector2 GetBottomPoint()
    {
        return (Vector2)transform.position + new Vector2(0f, bottomOffsetY);
    }

    Collider2D GetStairsCollider(Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, bottomCheckRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Stairs"))
                return hit;
        }
        return null;
    }

    bool IsPointInTagWithRadius(Vector2 point, string tag, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag(tag))
                return true;
        }
        return false;
    }

    bool IsPointInLayer(Vector2 point, LayerMask mask, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius, mask);
        return hits.Length > 0;
    }

    void FixedUpdate()
    {
        // === ВОСПРОИЗВЕДЕНИЕ ===
        if (records != null && records.Count > 0)
        {
            float timer = Time.time - startTime;

            // Проигрываем все записи, чьё время уже наступило
            while (index < records.Count && records[index].time <= timer)
            {
                PlayAction(records[index]);
                index++;
            }

            // НЕ сбрасываем currentMove — пусть последняя запись сама решит
        }

        // === СОСТОЯНИЯ ===
        bool centerInStairs = IsPointInTagWithRadius(GetCenterPoint(), "Stairs", centerCheckRadius);

        bool bottomOnPlatformLayer  = IsPointInLayer(GetBottomPoint(), platformsLayer, bottomCheckRadius);
        bool bottomOnStairsTopLayer = IsPointInLayer(GetBottomPoint(), stairsTopLayer, bottomCheckRadius);

        IsOnStairs    = centerInStairs;
        IsOnPlatform  = platformsInRange.Count > 0 || bottomOnPlatformLayer;
        IsOnStairsTop = bottomOnStairsTopLayer;

        bool onlyOnStairs = IsOnStairs && !IsOnPlatform && !IsOnStairsTop;

        // === ВЫРАВНИВАНИЕ И БЛОКИРОВКА ===
        if (onlyOnStairs && !wasOnlyStairs)
        {
            hasAligned = false;
            lockHorizontal = true;
        }

        if (IsOnPlatform || IsOnStairsTop)
            lockHorizontal = false;

        wasOnlyStairs = onlyOnStairs;

        // === ФИЗИКА ===
        bool shouldBeKinematic = onlyOnStairs || (IsOnStairs && IsOnStairsTop);

        if (shouldBeKinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;
        else
            rb.bodyType = RigidbodyType2D.Dynamic;

        // === ВВОД ===
        Vector2 move = currentMove;

        if (lockHorizontal)
            move.x = 0f;

        // === СЧИТАЕМ ЦЕЛЕВУЮ ПОЗИЦИЮ ===
        Vector2 targetPos = rb.position;

        // Спуск с верха
        if (IsOnStairsTop && !IsOnStairs && move.y < 0f)
        {
            rb.gravityScale = 0f;
            targetPos += new Vector2(move.x, move.y) * climbSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
            return;
        }

        // Обычное движение
        if (IsOnStairs)
        {
            rb.gravityScale = 0f;
            targetPos += new Vector2(move.x, move.y) * climbSpeed * Time.fixedDeltaTime;
        }
        else if (IsOnStairsTop || IsOnPlatform)
        {
            rb.gravityScale = 1f;
            targetPos += new Vector2(move.x, 0f) * speed * Time.fixedDeltaTime;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        // === ВЫРАВНИВАНИЕ ===
        if (onlyOnStairs && !hasAligned)
        {
            Collider2D stairsCol = GetStairsCollider(GetBottomPoint());

            if (stairsCol != null)
            {
                float stairsCenterX = stairsCol.bounds.center.x;
                float newX = Mathf.MoveTowards(targetPos.x, stairsCenterX, alignSpeed * Time.fixedDeltaTime);
                targetPos.x = newX;

                if (Mathf.Abs(newX - stairsCenterX) < 0.01f)
                    hasAligned = true;
            }
        }

        rb.MovePosition(targetPos);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            platformsInRange.Add(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            platformsInRange.Add(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            platformsInRange.Remove(collision.collider);
    }

    void OnDrawGizmos()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(0f, centerOffsetY);
        Vector2 bottom = (Vector2)transform.position + new Vector2(0f, bottomOffsetY);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, centerCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(bottom, bottomCheckRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, bottom);
    }
}