using UnityEngine;

public class Finish : MonoBehaviour
{
    [Header("Настройки")]
    public string playerTag = "Player";

    public LevelMenu levelMenu;

    private bool isFinished = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isFinished) return;
        if (!other.CompareTag(playerTag)) return;

        isFinished = true;
        FinishLevel();
    }

    void FinishLevel()
    {
        Debug.Log("Финиш");
        levelMenu.OpenFinishScreen();
    }
}