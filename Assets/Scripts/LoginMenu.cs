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
    private readonly string[] takenEmails = { "test@mail.ru", "admin@mail.ru" };

    private bool isRegisterMode = false;
    private bool isEmailStep = false;
    private string savedEmail = "";

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

        HideError();
    }

    void OnLoginClicked()
    {
        isRegisterMode = false;
        isEmailStep = false;

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
        isEmailStep = true;
        savedEmail = "";

        topButton.gameObject.SetActive(false);
        bottomButton.gameObject.SetActive(false);
        sendButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(true);

        topInputField.gameObject.SetActive(true);
        bottomInputField.gameObject.SetActive(true);

        topInputField.text = "";
        bottomInputField.text = "";

        topInputField.contentType = TMP_InputField.ContentType.Standard;
        bottomInputField.contentType = TMP_InputField.ContentType.Standard;

        topInputField.placeholder.GetComponent<TMP_Text>().text = "Логин";
        bottomInputField.placeholder.GetComponent<TMP_Text>().text = "Почта";

        bottomInputField.ActivateInputField();
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

        if (!isRegisterMode)
        {
            if (string.IsNullOrEmpty(topValue) || string.IsNullOrEmpty(bottomValue))
            {
                ShowError("Заполните все поля");
                return;
            }
            // Заглушка
            Debug.Log($"[LOGIN] Логин: {topValue}, Пароль: {bottomValue}");
            SceneManager.LoadScene(levelSceneName);
            return;
        }

        if (isEmailStep)
        {
            if (string.IsNullOrEmpty(topValue))
            {
                ShowError("Введите логин");
                topInputField.ActivateInputField();
                return;
            }

            if (IsLoginTaken(topValue))
            {
                ShowError("Логин уже занят");
                topInputField.ActivateInputField();
                return;
            }

            string emailError = GetEmailError(bottomValue);
            if (emailError != null)
            {
                ShowError(emailError);
                bottomInputField.ActivateInputField();
                return;
            }

            if (IsEmailTaken(bottomValue))
            {
                ShowError("Почта уже зарегистрирована");
                bottomInputField.ActivateInputField();
                return;
            }

            savedEmail = bottomValue;
            // Заглушка
            Debug.Log($"[REGISTER] Логин: {topValue}, Почта: {savedEmail}");

            isEmailStep = false;

            bottomInputField.text = "";
            bottomInputField.contentType = TMP_InputField.ContentType.Password;
            bottomInputField.placeholder.GetComponent<TMP_Text>().text = "Пароль";

            bottomInputField.ActivateInputField();
            HideError();
            return;
        }

        if (string.IsNullOrEmpty(topValue) || string.IsNullOrEmpty(bottomValue))
        {
            ShowError("Заполните все поля");
            return;
        }

        string passwordError = GetPasswordError(bottomValue);
        if (passwordError != null)
        {
            ShowError(passwordError);
            bottomInputField.ActivateInputField();
            return;
        }
        // Заглушка
        Debug.Log($"[REGISTER] Логин: {topValue}, Почта: {savedEmail}, Пароль: {bottomValue}");
        SceneManager.LoadScene(levelSceneName);
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

    string GetEmailError(string email)
    {
        if (email.Length < 5)
            return "Почта слишком короткая";

        if (email.Contains(" "))
            return "Почта не должна содержать пробелы";

        int atIndex = email.IndexOf('@');
        if (atIndex < 1)
            return "Почта должна содержать символ @";

        if (email.IndexOf('@', atIndex + 1) != -1)
            return "Почта должна содержать только один символ @";

        string localPart = email.Substring(0, atIndex);
        if (localPart.Length < 1)
            return "Введите имя почты до символа @";

        string domainPart = email.Substring(atIndex + 1);
        if (domainPart.Length < 3)
            return "Введите домен после символа @";

        int dotIndex = domainPart.IndexOf('.');
        if (dotIndex < 1)
            return "Почта должна содержать точку в домене";

        string tld = domainPart.Substring(dotIndex + 1);
        if (tld.Length < 2)
            return "Домен должен быть не короче 2 символов";

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

    bool IsEmailTaken(string email)
    {
        foreach (var taken in takenEmails)
        {
            if (string.Equals(taken, email, System.StringComparison.OrdinalIgnoreCase))
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
        isEmailStep = false;
        savedEmail = "";

        HideError();
    }
}