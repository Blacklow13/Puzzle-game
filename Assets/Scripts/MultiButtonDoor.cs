using UnityEngine;

public class MultiButtonDoor : MonoBehaviour
{
    [Header("Класс объекта")]
    public ObjectClass.ClassType objectClass = ObjectClass.ClassType.Eternal;

    [Header("Ссылки")]
    public Transform doorSprite;

    [Header("Кнопки")]
    public Button[] requiredButtons;

    [Header("Настройки")]
    public float moveDuration = 1f;

    private bool isOpening = false;
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

        if (requiredButtons != null)
        {
            foreach (var button in requiredButtons)
            {
                if (button != null)
                {
                    button.OnPressed += OnButtonChanged;
                    button.OnReleased += OnButtonChanged;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (requiredButtons != null)
        {
            foreach (var button in requiredButtons)
            {
                if (button != null)
                {
                    button.OnPressed -= OnButtonChanged;
                    button.OnReleased -= OnButtonChanged;
                }
            }
        }
    }

    void OnButtonChanged()
    {
        CheckAllPressed();
    }

    void CheckAllPressed()
    {
        if (isOpen || isOpening) return;

        if (requiredButtons == null || requiredButtons.Length == 0) return;

        foreach (var button in requiredButtons)
        {
            if (button == null || !button.IsPressed)
                return;
        }

        StartOpening();
    }

    void StartOpening()
    {
        if (isOpening || isOpen) return;

        isOpening = true;
        targetProgress = 1f;
    }

    void Update()
    {
        if (!isOpening) return;
        if (isOpen) return;

        currentProgress = Mathf.MoveTowards(
            currentProgress,
            targetProgress,
            Time.deltaTime / moveDuration
        );

        if (doorSprite != null)
        {
            float newScaleY = originalSpriteScale.y * (1f - currentProgress);
            float offsetY = (doorHeight * currentProgress) / 2f;

            doorSprite.localScale = new Vector3(
                originalSpriteScale.x,
                newScaleY,
                originalSpriteScale.z
            );

            doorSprite.localPosition = originalSpritePosition + new Vector3(0f, offsetY, 0f);
        }

        if (Mathf.Abs(currentProgress - 1f) < 0.001f)
        {
            currentProgress = 1f;
            isOpen = true;
            isOpening = false;

            if (doorCollider != null)
                doorCollider.enabled = false;
        }
    }
}