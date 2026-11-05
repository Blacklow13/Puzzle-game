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
            {
                // ИСПРАВЛЕНИЕ: Игнорируем лестницу, если клон не может с ней взаимодействовать
                ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
                if (objClass != null && !objClass.CanCloneInteract()) continue;

                return hit;
            }
        }
        return null;
    }

    bool IsPointInStairs(Vector2 point, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Stairs"))
            {
                // ИСПРАВЛЕНИЕ: Проверка класса
                ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
                if (objClass != null && !objClass.CanCloneInteract()) continue;

                return true;
            }
        }
        return false;
    }

    bool IsPointInLayer(Vector2 point, LayerMask mask, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius, mask);

        foreach (var hit in hits)
        {
            // ИСПРАВЛЕНИЕ: Проверка класса
            ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
            if (objClass != null && !objClass.CanCloneInteract()) continue;

            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        if (records != null && records.Count > 0)
        {
            float timer = Time.time - startTime;
            while (index < records.Count && records[index].time <= timer)
            {
                PlayAction(records[index]);
                index++;
            }
        }

        bool centerInStairs = IsPointInStairs(GetCenterPoint(), centerCheckRadius);

        bool bottomOnPlatformLayer  = IsPointInLayer(GetBottomPoint(), platformsLayer, bottomCheckRadius);
        bool bottomOnStairsTopLayer = IsPointInLayer(GetBottomPoint(), stairsTopLayer, bottomCheckRadius);

        IsOnStairs    = centerInStairs;
        IsOnPlatform  = platformsInRange.Count > 0 || bottomOnPlatformLayer;
        IsOnStairsTop = bottomOnStairsTopLayer;

        bool onlyOnStairs = IsOnStairs && !IsOnPlatform && !IsOnStairsTop;

        if (onlyOnStairs && !wasOnlyStairs)
        {
            hasAligned = false;
            lockHorizontal = true;
        }

        if (IsOnPlatform || IsOnStairsTop)
            lockHorizontal = false;

        wasOnlyStairs = onlyOnStairs;

        bool shouldBeKinematic = onlyOnStairs || (IsOnStairs && IsOnStairsTop);

        if (shouldBeKinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;
        else
            rb.bodyType = RigidbodyType2D.Dynamic;

        Vector2 move = currentMove;

        if (lockHorizontal)
            move.x = 0f;

        Vector2 targetPos = transform.position;

        if (IsOnStairsTop && !IsOnStairs && move.y < 0f)
        {
            rb.gravityScale = 0f;
            targetPos += new Vector2(move.x, move.y) * climbSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
            return;
        }

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

        // Если клон на лестнице/платформе — двигаем напрямую. 
        // В воздухе — даем физике тянуть его вниз.
        if (IsOnStairs || IsOnStairsTop || IsOnPlatform)
        {
            rb.MovePosition(targetPos);
        }
        else
        {
            rb.linearVelocity = new Vector2(move.x * speed, rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            // ИСПРАВЛЕНИЕ: не добавляем платформу, если клон сквозь нее проваливается
            ObjectClass objClass = collision.gameObject.GetComponentInParent<ObjectClass>();
            if (objClass == null || objClass.CanCloneInteract())
                platformsInRange.Add(collision.collider);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            ObjectClass objClass = collision.gameObject.GetComponentInParent<ObjectClass>();
            if (objClass == null || objClass.CanCloneInteract())
                platformsInRange.Add(collision.collider);
            else
                platformsInRange.Remove(collision.collider); // Удаляем, если платформа стала недоступной
        }
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

    public bool CheckIfCloneIsInAir()
    {
        if (!IsOnStairs && !IsOnPlatform && !IsOnStairsTop)
        {
            if (rb.linearVelocity.y < -0.1f)
                return true;
        }
        return false;
    }

    public void Die()
    {
        Debug.Log("[CLONE] Клон упал и умер!");
    }
}