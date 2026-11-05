using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginMenu : MonoBehaviour
{
    [Header("Кнопки")]
    public UnityEngine.UI.Button topButton;
    public UnityEngine.UI.Button bottomButton;
    public UnityEngine.UI.Button backButton;
    public UnityEngine.UI.Button sendButton;

    [Header("Тексты кнопок")]
    public TMP_Text topButtonText;
    public TMP_Text bottomButtonText;

    [Header("Поля ввода")]
    public TMP_InputField topInputField;
    public TMP_InputField bottomInputField;

    [Header("Ошибки")]
    public TMP_Text errorText;

    [Header("Сцена уровня")]
    public string levelSceneName = "Level1";

    private readonly string[] takenLogins = { "admin", "test", "user" };

    private bool isRegisterMode = false;

    void Start()
    {
        topInputField.gameObject.SetActive(false);
        bottomInputField.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        topButton.gameObject.SetActive(true);
        bottomButton.gameObject.SetActive(true);

        topButtonText.text = "Войти";
        bottomButtonText.text = "Зарегистрироваться";

        topButton.onClick.AddListener(OnLoginClicked);
        bottomButton.onClick.AddListener(OnRegisterClicked);
        backButton.onClick.AddListener(OnBackClicked);
        sendButton.onClick.AddListener(OnSendClicked);
    }

    void OnLoginClicked()
    {
        isRegisterMode = false;

        topButton.gameObject.SetActive(false);
        bottomButton.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        topInputField.gameObject.SetActive(true);
        bottomInputField.gameObject.SetActive(true);

        topInputField.text = "";
        bottomInputField.text = "";

        topInputField.contentType = TMP_InputField.ContentType.Standard;
        bottomInputField.contentType = TMP_InputField.ContentType.Password;

        topInputField.placeholder.GetComponent<TMP_Text>().text = "Логин";
        bottomInputField.placeholder.GetComponent<TMP_Text>().text = "Пароль";

        topInputField.ActivateInputField();
        HideError();

        topInputField.onSubmit.RemoveAllListeners();
        bottomInputField.onSubmit.RemoveAllListeners();

        topInputField.onSubmit.AddListener(OnTopSubmit);
        bottomInputField.onSubmit.AddListener(OnBottomSubmit);
    }

    void OnRegisterClicked()
    {
        isRegisterMode = true;

        topButton.gameObject.SetActive(false);
        bottomButton.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        topInputField.gameObject.SetActive(true);
        bottomInputField.gameObject.SetActive(true);

        topInputField.text = "";
        bottomInputField.text = "";

        topInputField.contentType = TMP_InputField.ContentType.Standard;
        bottomInputField.contentType = TMP_InputField.ContentType.Password;

        topInputField.placeholder.GetComponent<TMP_Text>().text = "Логин";
        bottomInputField.placeholder.GetComponent<TMP_Text>().text = "Пароль";

        topInputField.ActivateInputField();
        HideError();

        topInputField.onSubmit.RemoveAllListeners();
        bottomInputField.onSubmit.RemoveAllListeners();

        topInputField.onSubmit.AddListener(OnTopSubmit);
        bottomInputField.onSubmit.AddListener(OnBottomSubmit);
    }

    void OnTopSubmit(string value)
    {
        bottomInputField.ActivateInputField();
    }

    void OnBottomSubmit(string value)
    {
        string topValue = topInputField.text.Trim();
        string bottomValue = bottomInputField.text.Trim();

        if (!string.IsNullOrEmpty(bottomValue) && string.IsNullOrEmpty(topValue))
        {
            ShowError("Введите логин");
            topInputField.ActivateInputField();
            return;
        }

        if (string.IsNullOrEmpty(topValue) || string.IsNullOrEmpty(bottomValue))
        {
            ShowError("Заполните все поля");
            return;
        }

        if (!isRegisterMode)
        {
            ApiManager.Instance.Login(topValue, bottomValue,
                onSuccess: (msg) => {
                    Debug.Log("Успешный вход!");
                    SceneManager.LoadScene(levelSceneName);
                },
                onError: (err) => {
                    ShowError("Неверный логин или пароль");
                });
            return;
        }

        string passwordError = GetPasswordError(bottomValue);
        if (passwordError != null)
        {
            ShowError(passwordError);
            bottomInputField.ActivateInputField();
            return;
        }

        ApiManager.Instance.Register(topValue, bottomValue,
            onSuccess: (msg) =>
            {
                Debug.Log("Аккаунт создан! Выполняем вход...");

                ApiManager.Instance.Login(topValue, bottomValue,
                    onSuccess: (loginMsg) => { SceneManager.LoadScene(levelSceneName); },
                    onError: (loginErr) => { ShowError("Ошибка входа после регистрации"); });
            },
            onError: (err) =>
            {
                ShowError("Ошибка: логин уже занят или данные неверны");
            });
    }

    void OnSendClicked()
    {
        OnBottomSubmit(bottomInputField.text);
    }

    string GetPasswordError(string password)
    {
        if (password.Length < 6)
            return "Пароль должен быть не менее 6 символов";

        if (password.Contains(" "))
            return "Пароль не должен содержать пробелы";

        bool hasLetter = false;
        bool hasDigit = false;

        foreach (char c in password)
        {
            if (char.IsLetter(c)) hasLetter = true;
            if (char.IsDigit(c)) hasDigit = true;
        }

        if (!hasLetter)
            return "Пароль должен содержать хотя бы одну букву";

        if (!hasDigit)
            return "Пароль должен содержать хотя бы одну цифру";

        return null;
    }

    bool IsLoginTaken(string login)
    {
        foreach (var taken in takenLogins)
        {
            if (string.Equals(taken, login, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }

        Debug.Log($"[ERROR] {message}");
    }

    void HideError()
    {
        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
    }

    void OnBackClicked()
    {
        topInputField.gameObject.SetActive(false);
        bottomInputField.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);

        topButton.gameObject.SetActive(true);
        bottomButton.gameObject.SetActive(true);

        topButtonText.text = "Войти";
        bottomButtonText.text = "Зарегистрироваться";

        topInputField.onSubmit.RemoveAllListeners();
        bottomInputField.onSubmit.RemoveAllListeners();

        isRegisterMode = false;
        HideError();
    }
}