using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float climbSpeed = 3.0f;

    [Header("Точки проверки")]
    public float centerOffsetY = 0.3f;
    public float bottomOffsetY = -0.5f;

    [Header("Радиусы")]
    public float centerCheckRadius = 0.1f;
    public float bottomCheckRadius = 0.25f;

    [Header("Выравнивание по лестнице")]
    public float alignSpeed = 10f;

    [Header("Слои")]
    public LayerMask platformsLayer;
    public LayerMask stairsTopLayer;

    private Rigidbody2D rb;
    private readonly HashSet<Collider2D> platformsInRange = new HashSet<Collider2D>();

    public bool IsOnPlatform  = false;
    public bool IsOnStairs    = false;
    public bool IsOnStairsTop = false;

    private bool hasAligned     = false;
    private bool lockHorizontal = false;
    private bool wasOnlyStairs  = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
                ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
                if (objClass != null && !objClass.CanPlayerInteract())
                    continue;

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
                ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
                if (objClass != null && !objClass.CanPlayerInteract())
                    continue;

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
            ObjectClass objClass = hit.GetComponentInParent<ObjectClass>();
            // Если платформа неактивна для игрока, игнорируем её
            if (objClass != null && !objClass.CanPlayerInteract())
                continue;

            return true;
        }
        return false;
    }

    void FixedUpdate()
    {
        bool centerInStairs = IsPointInStairs(GetCenterPoint(), centerCheckRadius);
        bool bottomInStairs = IsPointInStairs(GetBottomPoint(), bottomCheckRadius);

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

        if (onlyOnStairs && !hasAligned)
        {
            Collider2D stairsCol = GetStairsCollider(GetBottomPoint());

            if (stairsCol != null)
            {
                float stairsCenterX = stairsCol.bounds.center.x;
                float currentX = rb.position.x;
                float newX = Mathf.MoveTowards(currentX, stairsCenterX, alignSpeed * Time.fixedDeltaTime);

                if (Mathf.Abs(newX - stairsCenterX) < 0.01f)
                    hasAligned = true;

                rb.position = new Vector2(newX, rb.position.y);
            }
        }

        wasOnlyStairs = onlyOnStairs;

        bool shouldBeKinematic = onlyOnStairs || (IsOnStairs && IsOnStairsTop);

        if (shouldBeKinematic)
            rb.bodyType = RigidbodyType2D.Kinematic;
        else
            rb.bodyType = RigidbodyType2D.Dynamic;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (lockHorizontal)
            h = 0f;

        Vector2 targetPos = rb.position;

        if (IsOnStairsTop && !IsOnStairs && v < 0f)
        {
            rb.gravityScale = 0f;
            targetPos += new Vector2(h, v) * climbSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPos);
            return;
        }

        if (IsOnStairs)
        {
            rb.gravityScale = 0f;
            targetPos += new Vector2(h, v) * climbSpeed * Time.fixedDeltaTime;
        }
        else if (IsOnStairsTop || IsOnPlatform)
        {
            rb.gravityScale = 1f;
            targetPos += new Vector2(h, 0f) * speed * Time.fixedDeltaTime;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        if (IsOnStairs || IsOnStairsTop || IsOnPlatform)
        {
            rb.MovePosition(targetPos);
        }
        else
        {
      
            rb.linearVelocity = new Vector2(h * speed, rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            platformsInRange.Add(collision.collider);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            ObjectClass objClass = collision.gameObject.GetComponentInParent<ObjectClass>();
            if (objClass != null && !objClass.CanPlayerInteract())
                platformsInRange.Remove(collision.collider); // Платформа стала призрачной — удаляем
            else
                platformsInRange.Add(collision.collider);
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

    public bool CheckIfPlayerIsInAir()
    {
        // Если мы на лестнице, мы не падаем
        if (IsOnStairs || IsOnStairsTop) 
            return false;

        // Если скорость по Y отрицательная (мы летим вниз), значит мы падаем
        if (rb.linearVelocity.y < -0.5f) 
            return true;

        // Если мы не на платформе
        if (!IsOnPlatform)
            return true;

        return false;
    }

    public void Die()
    {
        // Заглушка
    }
}