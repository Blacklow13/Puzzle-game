using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("Класс объекта (время)")]
    public ObjectClass.ClassType objectClass = ObjectClass.ClassType.Eternal;

    [Header("Ссылка на дверь (для Door.cs)")]
    public Door targetDoor;

    [Header("Настройки")]
    public string activatorTag = "Player";

    public event System.Action OnPressed;
    public event System.Action OnReleased;

    public bool IsPressed { get; private set; } = false;

    private int pressedCount = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanInteract(other)) return;

        pressedCount++;

        if (pressedCount == 1)
        {
            IsPressed = true;

            if (targetDoor != null)
                targetDoor.Open();

            OnPressed?.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!CanInteract(other)) return;

        pressedCount--;

        if (pressedCount <= 0)
        {
            pressedCount = 0;

            if (IsPressed)
            {
                IsPressed = false;

                if (targetDoor != null)
                    targetDoor.Close();

                OnReleased?.Invoke();
            }
        }
    }

    bool CanInteract(Collider2D other)
    {
        if (!other.CompareTag(activatorTag) && !other.CompareTag("Clone"))
            return false;

        switch (objectClass)
        {
            case ObjectClass.ClassType.Eternal:
                return true;

            case ObjectClass.ClassType.Past:
                return other.CompareTag("Clone") || !IsCloneSpawned();

            case ObjectClass.ClassType.Present:
                return other.CompareTag("Player");

            default:
                return true;
        }
    }

    bool IsCloneSpawned()
    {
        LevelManager lm = FindObjectOfType<LevelManager>();
        return lm != null && lm.Clone != null && lm.Clone.gameObject.activeSelf;
    }
}