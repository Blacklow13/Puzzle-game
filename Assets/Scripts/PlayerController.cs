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
        bool centerInStairs = IsPointInTagWithRadius(GetCenterPoint(), "Stairs", centerCheckRadius);
        bool bottomInStairs = IsPointInTagWithRadius(GetBottomPoint(), "Stairs", bottomCheckRadius);

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

        if (Application.isPlaying)
        {
            Collider2D stairsCol = GetStairsCollider(bottom);
            if (stairsCol != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(stairsCol.bounds.center, new Vector3(0.1f, 0.5f, 0f));
            }
        }
    }
}