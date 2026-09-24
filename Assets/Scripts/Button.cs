using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("Класс объекта (время)")]
    public ObjectClass.ClassType objectClass = ObjectClass.ClassType.Eternal;

    [Header("Ссылка на дверь")]
    public Door targetDoor;

    [Header("Настройки")]
    public string activatorTag = "Player";

    private int pressedCount = 0;   // сколько объектов на кнопке

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanInteract(other)) return;

        pressedCount++;

        if (pressedCount == 1 && targetDoor != null)
            targetDoor.Open();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!CanInteract(other)) return;

        pressedCount--;

        if (pressedCount <= 0)
        {
            pressedCount = 0;

            if (targetDoor != null)
                targetDoor.Close();
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