using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform doorSprite;

    [Header("Настройки")]
    public float moveDuration = 0.3f;
    public float moveDistance = 2f;

    private bool isOpen = false;
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private Vector3 originalSpritePosition;

    private BoxCollider2D doorCollider;

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorSprite != null)
            originalSpritePosition = doorSprite.localPosition;
    }

    public void Open()
    {
        if (isOpen) return;
        
        isOpen = true;
        targetProgress = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.doorOpen);
    }

    public void Close()
    {
        if (!isOpen) return;
        
        isOpen = false;
        targetProgress = 0f;
    }

    void Update()
    {
        if (Mathf.Abs(currentProgress - targetProgress) < 0.001f) return;

        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime / moveDuration);

        if (doorSprite != null)
        {
            doorSprite.localPosition = originalSpritePosition + new Vector3(0f, -moveDistance * currentProgress, 0f);
        }

        if (doorCollider != null)
        {
            doorCollider.enabled = (Mathf.Abs(currentProgress - 1f) > 0.001f);
        }
    }
}