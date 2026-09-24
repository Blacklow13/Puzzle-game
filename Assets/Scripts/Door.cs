using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Класс объекта (время)")]
    public ObjectClass.ClassType objectClass = ObjectClass.ClassType.Eternal;

    [Header("Настройки")]
    public float moveDuration = 0.3f;

    private bool isOpen = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private float doorHeight;          // высота двери в мировых координатах

    private float currentProgress = 0f;  // 0 = закрыто, 1 = открыто
    private float targetProgress = 0f;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;

        // Высота двери = размер коллайдера * scale
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
            doorHeight = box.size.y * originalScale.y;
        else
            doorHeight = originalScale.y;
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

        // Новая высота: 1 -> 0
        float newScaleY = originalScale.y * (1f - currentProgress);

        // Сдвиг ВВЕРХ: на половину "исчезнувшей" высоты
        float offsetY = (doorHeight * currentProgress) / 2f;

        transform.localScale = new Vector3(originalScale.x, newScaleY, originalScale.z);
        transform.position = originalPosition + new Vector3(0f, offsetY, 0f);
    }
}