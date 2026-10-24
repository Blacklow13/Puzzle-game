using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Класс объекта (время)")]
    public ObjectClass.ClassType objectClass = ObjectClass.ClassType.Eternal;

    [Header("Ссылки")]
    public Transform doorSprite;

    [Header("Настройки")]
    public float moveDuration = 0.3f;

    private bool isOpen = false;

    private Vector3 originalSpriteScale;
    private Vector3 originalSpritePosition;
    private float doorHeight;

    private float currentProgress = 0f;
    private float targetProgress = 0f;

    private BoxCollider2D doorCollider;

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();

        if (doorSprite != null)
        {
            originalSpriteScale = doorSprite.localScale;
            originalSpritePosition = doorSprite.localPosition;
        }

        if (doorCollider != null)
            doorHeight = doorCollider.size.y;
        else
            doorHeight = 1f;
    }

    public void Open()
    {
        if (isOpen) return;

        isOpen = true;
        targetProgress = 1f;
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        targetProgress = 0f;
    }

    void Update()
    {
        if (Mathf.Abs(currentProgress - targetProgress) < 0.001f)
            return;

        currentProgress = Mathf.MoveTowards(
            currentProgress,
            targetProgress,
            Time.deltaTime / moveDuration
        );

        if (doorSprite != null)
        {
            float newScaleY = originalSpriteScale.y * (1f - currentProgress);
            float offsetY = (doorHeight * currentProgress) / 2f;

            doorSprite.localScale = new Vector3(originalSpriteScale.x, newScaleY, originalSpriteScale.z);
            doorSprite.localPosition = originalSpritePosition + new Vector3(0f, offsetY, 0f);
        }

        // === Управление коллайдером ===
        if (doorCollider != null)
        {
            if (Mathf.Abs(currentProgress - 1f) < 0.001f)
                doorCollider.enabled = false;
            else
                doorCollider.enabled = true;
        }
    }
}