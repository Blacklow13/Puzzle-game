using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class AccountPanel : MonoBehaviour
{
    [Header("Поле ввода")]
    public TMP_InputField oldPasswordInput;
    public TMP_InputField newPasswordInput;

    [Header("Кнопка и её текст")]
    public TMP_Text changePasswordButtonText;

    [Header("Навигация")]
    public GameObject levelMenu;

    [Header("Сцена меню уровней")]
    public string levelsMenuSceneName = "Menu";

    private string defaultButtonText = "Сменить пароль";
    private Color defaultButtonColor = Color.white;

    private Coroutine textCoroutine;
    private bool isBusy = false;

    void Start()
    {
        if (changePasswordButtonText != null)
        {
            defaultButtonText = changePasswordButtonText.text;
            defaultButtonColor = changePasswordButtonText.color;
        }
    }

    void OnEnable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.isTyping = true;
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.isTyping = false;
    }

    // ---------- Смена пароля ----------

    public void OnChangePasswordClicked()
    {
        if (isBusy) return;

        string oldPass = oldPasswordInput != null ? oldPasswordInput.text : "";
        string newPass = newPasswordInput != null ? newPasswordInput.text : "";

        if (string.IsNullOrEmpty(oldPass))
        {
            ShowButtonMessage("Введите текущий пароль", Color.red, 2f);
            return;
        }
        if (string.IsNullOrEmpty(newPass))
        {
            ShowButtonMessage("Введите новый пароль", Color.red, 2f);
            return;
        }
        if (newPass.Length < 4)
        {
            ShowButtonMessage("Пароль < 4 символов", Color.red, 2f);
            return;
        }
        if (oldPass == newPass)
        {
            ShowButtonMessage("Пароли совпадают", Color.red, 2f);
            return;
        }

        if (ApiManager.Instance == null)
        {
            ShowButtonMessage("Нет соединения", Color.red, 2f);
            return;
        }

        isBusy = true;
        ShowButtonMessage("Отправка...", Color.yellow, 5f);

        ApiManager.Instance.ChangePassword(
            oldPass,
            newPass,
            onSuccess: (msg) =>
            {
                isBusy = false;
                ShowButtonMessage("Пароль изменён!", Color.green, 2f);
                if (oldPasswordInput != null) oldPasswordInput.text = "";
                if (newPasswordInput != null) newPasswordInput.text = "";
                Debug.Log("[ACCOUNT] " + msg);
            },
            onError: (err) =>
            {
                isBusy = false;
                ShowButtonMessage(err, Color.red, 3f);
                Debug.LogWarning("[ACCOUNT] ChangePassword error: " + err);
            }
        );
    }

    // ---------- Сброс прогресса ----------

    public void OnResetGameClicked()
    {
        if (isBusy) return;

        if (ApiManager.Instance == null)
        {
            ShowButtonMessage("Нет соединения", Color.red, 2f);
            return;
        }

        isBusy = true;
        ShowButtonMessage("Сброс...", Color.yellow, 5f);

        ApiManager.Instance.ResetProgress(
            onSuccess: (msg) =>
            {
                isBusy = false;
                Debug.Log("[ACCOUNT] " + msg);
                ShowButtonMessage("Игра сброшена", Color.yellow, 2f);
                Invoke(nameof(ReturnToLevelsMenu), 2f);
            },
            onError: (err) =>
            {
                isBusy = false;
                ShowButtonMessage("Сброшено локально", Color.yellow, 2f);
                Debug.LogWarning("[ACCOUNT] ResetProgress error: " + err);
                Invoke(nameof(ReturnToLevelsMenu), 2f);
            }
        );
    }

    // ---------- Навигация ----------

    public void OnBackClicked()
    {
        Debug.Log("[UI] Кнопка Назад нажата!");

        gameObject.SetActive(false);

        if (levelMenu != null)
            levelMenu.SetActive(true);
    }

    // ---------- Вспомогательное ----------

    private void ShowButtonMessage(string message, Color color, float duration)
    {
        if (textCoroutine != null)
            StopCoroutine(textCoroutine);

        textCoroutine = StartCoroutine(ChangeTextRoutine(message, color, duration));
    }

    private IEnumerator ChangeTextRoutine(string message, Color color, float duration)
    {
        if (changePasswordButtonText == null) yield break;

        changePasswordButtonText.text = message;
        changePasswordButtonText.color = color;

        yield return new WaitForSecondsRealtime(duration);

        changePasswordButtonText.text = defaultButtonText;
        changePasswordButtonText.color = defaultButtonColor;
    }

    void ReturnToLevelsMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelsMenuSceneName);
    }
}