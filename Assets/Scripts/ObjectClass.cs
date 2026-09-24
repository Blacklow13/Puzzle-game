using UnityEngine;

public class ObjectClass : MonoBehaviour
{
    public enum ClassType
    {
        Eternal,   // серый — и игрок, и клон
        Past,      // синий — до появления клона (игрок), после — клон
        Present    // жёлтый — только игрок
    }

    public ClassType type = ClassType.Eternal;

    void Start()
    {
        ApplyColor();
    }

    void ApplyColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (type)
        {
            case ClassType.Eternal:
                sr.color = Color.gray;
                break;

            case ClassType.Past:
                sr.color = new Color(0.3f, 0.5f, 1f);  // синий
                break;

            case ClassType.Present:
                sr.color = new Color(1f, 0.9f, 0.2f);  // жёлтый
                break;
        }
    }
}