using UnityEngine;

public class ObjectClass : MonoBehaviour
{
    public enum ClassType
    {
        Eternal, // Серый
        Past,    // Синий
        Present  // Жёлтый
    }

    [Header("Класс объекта")]
    public ClassType type = ClassType.Present;

    [Header("Ссылки")]
    public LevelManager levelManager;
    public Collider2D playerCollider;
    public Collider2D cloneCollider;

    [Header("Цвета")]
    public Color eternalColor = Color.gray;
    public Color pastColor = new Color(0.3f, 0.5f, 1f);
    public Color presentColor = new Color(1f, 0.9f, 0.2f);

    [Header("Прозрачность")]
    [Range(0f, 1f)]
    public float disabledAlpha = 0.25f;

    private SpriteRenderer spriteRenderer;
    private Collider2D objectCollider;
    private bool cloneSpawned = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectCollider = GetComponent<Collider2D>();

        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();

        if (playerCollider == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerCollider = player.GetComponentInChildren<Collider2D>();
        }

        if (cloneCollider == null && levelManager != null && levelManager.Clone != null)
        {
            cloneCollider = levelManager.Clone.GetComponentInChildren<Collider2D>();
        }

        UpdateState();
    }

    void Update()
    {
        if (levelManager == null) return;

        bool newCloneState = levelManager.Clone != null && levelManager.Clone.gameObject.activeSelf;

        if (newCloneState != cloneSpawned)
        {
            cloneSpawned = newCloneState;
            UpdateState();
        }
    }

    void UpdateState()
    {
        bool isActive = IsActiveNow();

        ApplyColor(isActive);
        UpdateCollisions();
    }

    bool IsActiveNow()
    {
        switch (type)
        {
            case ClassType.Eternal:
                return true; // Всегда активен
            case ClassType.Past:
                return !cloneSpawned; // Активен ТОЛЬКО до клона
            case ClassType.Present:
                return cloneSpawned; // Активен ТОЛЬКО после клона
            default:
                return true;
        }
    }

    void UpdateCollisions()
    {
        if (objectCollider == null) return;

        bool ignorePlayer = false;
        bool ignoreClone = false;

        // Определяем, кто сквозь что проходит
        switch (type)
        {
            case ClassType.Eternal:
                ignorePlayer = false; // Все всегда взаимодействуют 
                ignoreClone = false;
                break;

            case ClassType.Past:
                ignorePlayer = cloneSpawned;  // Игрок проходит после клона
                ignoreClone = false;          // Клон всегда упирается
                break;

            case ClassType.Present:
                ignorePlayer = !cloneSpawned; // Игрок проходит ДО клона
                ignoreClone = true;           // Клон всегда проходит
                break;
        }

        // Применяем игнорирование коллизий
        if (playerCollider != null)
            Physics2D.IgnoreCollision(objectCollider, playerCollider, ignorePlayer);

        if (cloneCollider != null)
            Physics2D.IgnoreCollision(objectCollider, cloneCollider, ignoreClone);
    }

    void ApplyColor(bool isActive)
    {
        if (spriteRenderer == null) return;

        Color baseColor;

        switch (type)
        {
            case ClassType.Eternal:
                baseColor = eternalColor;
                break;
            case ClassType.Past:
                baseColor = pastColor;
                break;
            case ClassType.Present:
                baseColor = presentColor;
                break;
            default:
                baseColor = Color.white;
                break;
        }

        baseColor.a = isActive ? 1f : disabledAlpha;
        spriteRenderer.color = baseColor;
    }

    public bool CanPlayerInteract()
    {
        switch (type)
        {
            case ClassType.Eternal:  return true;
            case ClassType.Past:     return !cloneSpawned;
            case ClassType.Present:  return cloneSpawned;
            default:                 return true;
        }
    }

    public bool CanCloneInteract()
    {
        switch (type)
        {
            case ClassType.Eternal:  return true;
            case ClassType.Past:     return true;
            case ClassType.Present:  return false;
            default:                 return true;
        }
    }

    public bool CanInteract(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return CanPlayerInteract();

        if (other.CompareTag("Clone"))
            return CanCloneInteract();

        return false;
    }
}