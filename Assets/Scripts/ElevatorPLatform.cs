using UnityEngine;
using System.Collections.Generic;

public class ElevatorPlatform : MonoBehaviour
{
    [Header("Настройки движения")]
    public Vector2 moveOffset = new Vector2(0, 3f);
    public float moveSpeed = 2f;
    public bool stayUp = true;

    [Header("Ссылки")]
    public Button targetButton;

    private Vector2 startPos;
    private Vector2 targetPos;
    private bool isMoving = false;
    private Rigidbody2D rb;

    private List<Rigidbody2D> passengers = new List<Rigidbody2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        targetPos = startPos;

        if (targetButton != null)
        {
            targetButton.OnPressed += OnButtonPressed;
            targetButton.OnReleased += OnButtonReleased;
        }
    }

    void OnDestroy()
    {
        if (targetButton != null)
        {
            targetButton.OnPressed -= OnButtonPressed;
            targetButton.OnReleased -= OnButtonReleased;
        }
    }

    void OnButtonPressed()
    {
        targetPos = startPos + moveOffset;
        isMoving = true;

        // ДОБАВЛЕНО: Звук запуска лифта
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.doorOpen);
    }

    void OnButtonReleased()
    {
        if (!stayUp)
        {
            targetPos = startPos;
            isMoving = true;
        }
    }

    void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 previousPos = rb.position;

        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        Vector2 delta = newPos - previousPos;

        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i] == null)
            {
                passengers.RemoveAt(i);
                continue;
            }
            
            passengers[i].MovePosition(passengers[i].position + delta);
        }

        if (Vector2.Distance(newPos, targetPos) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Clone"))
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                Rigidbody2D passengerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (passengerRb != null && !passengers.Contains(passengerRb))
                {
                    passengers.Add(passengerRb);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Clone"))
        {
            Rigidbody2D passengerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (passengerRb != null)
            {
                passengers.Remove(passengerRb);
            }
        }
    }
}