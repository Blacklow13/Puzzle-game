using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance;
    public static event System.Action OnProgressUpdated;

    [Header("Сервер")]
    [SerializeField] private string baseUrl = "http://127.0.0.1:5000/api";
    [SerializeField] private int timeoutSeconds = 10;
    [SerializeField] private int maxLevel = 7;

    // ---------- DTO ----------

    [System.Serializable]
    private class AuthRequest
    {
        public string login;
        public string password;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string token;
        public string access_token;
    }

    [System.Serializable]
    private class SaveProgressRequest
    {
        public int level_id;
    }

    [System.Serializable]
    private class ProgressResponse
    {
        public List<int> completed_levels = new List<int>();
    }

    [System.Serializable]
    private class ChangePasswordRequest
    {
        public string old_password;
        public string new_password;
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string msg;
        public string message;
        public string error;
    }

    [System.Serializable]
    private class SettingsDto
    {
        public float master = 1f;
        public float music = 1f;
        public float sfx = 1f;
    }

    // ---------- Жизненный цикл ----------

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[API] Токена нет — LoadProgress пропущен до логина.");
            return;
        }

        LoadProgress(
            onSuccess: (levels) =>
            {
                ApplyLevelsToPrefs(levels);
                Debug.Log($"[API] Прогресс синхронизирован. Уровней: {levels.Count}");
                OnProgressUpdated?.Invoke();
            },
            onError: (err) => Debug.Log("[API] LoadProgress при старте не удался: " + err)
        );

        LoadSettings(
            onSuccess: (m, mu, s) => Debug.Log($"[API] Настройки синхронизированы: {m:F2}/{mu:F2}/{s:F2}"),
            onError: (err) => Debug.Log("[API] LoadSettings при старте не удался: " + err)
        );
    }

    // ---------- Публичное API ----------

    public void Register(string login, string password,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        if (!ValidateCredentials(login, password, onError)) return;
        StartCoroutine(RegisterRoutine(login.Trim(), password, onSuccess, onError));
    }

    public void Login(string login, string password,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        if (!ValidateCredentials(login, password, onError)) return;
        StartCoroutine(LoginRoutine(login.Trim(), password, onSuccess, onError));
    }

    public void SaveProgress(int levelId)
    {
        if (levelId < 1 || levelId > maxLevel)
        {
            Debug.LogWarning($"[API] SaveProgress: levelId={levelId} вне 1..{maxLevel}, пропускаем.");
            return;
        }

        PlayerPrefs.SetInt("Level_" + levelId + "_Completed", 1);
        if (levelId + 1 <= maxLevel)
            PlayerPrefs.SetInt("Level_" + (levelId + 1) + "_Unlocked", 1);
        PlayerPrefs.Save();
        Debug.Log($"[PLAYERPREFS] Уровень {levelId} пройден локально.");
        OnProgressUpdated?.Invoke();

        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[API] SaveProgress: нет токена, на сервер не отправляем.");
            return;
        }
        StartCoroutine(SaveProgressRoutine(levelId, token));
    }

    public void LoadProgress(System.Action<List<int>> onSuccess, System.Action<string> onError)
    {
        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[API] LoadProgress: токена нет, пропускаем.");
            return;
        }
        StartCoroutine(LoadProgressRoutine(token, onSuccess, onError));
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("JWT_Token");
        PlayerPrefs.Save();
        Debug.Log("[API] Токен удалён (logout).");
    }

    // ---------- Сброс ----------

    public void ResetProgress(System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        ClearProgressInPrefs();
        ClearSettingsInPrefs();
        OnProgressUpdated?.Invoke();
        Debug.Log("[API] Локальный прогресс и настройки очищены.");

        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[API] ResetProgress: нет токена, сервер не трогаем.");
            onSuccess?.Invoke("Локальный прогресс очищен");
            return;
        }

        StartCoroutine(ResetProgressRoutine(token, onSuccess, onError));
    }

    public void ClearLocalProgress()
    {
        ClearProgressInPrefs();
        OnProgressUpdated?.Invoke();
        Debug.Log("[API] Локальный прогресс очищен (без сервера).");
    }

    // ---------- Настройки ----------

    public void SaveSettings(float master, float music, float sfx,
        System.Action<string> onSuccess = null, System.Action<string> onError = null)
    {
        WriteSettingsToPrefs(master, music, sfx);
        Debug.Log($"[SETTINGS] Локально сохранено: master={master:F2}, music={music:F2}, sfx={sfx:F2}");

        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[SETTINGS] SaveSettings: нет токена, на сервер не отправляем.");
            onSuccess?.Invoke("Настройки сохранены локально");
            return;
        }

        StartCoroutine(SaveSettingsRoutine(master, music, sfx, token, onSuccess, onError));
    }

    public void LoadSettings(System.Action<float, float, float> onSuccess,
        System.Action<string> onError = null)
    {
        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            Debug.Log("[SETTINGS] LoadSettings: токена нет, пропускаем.");
            onError?.Invoke("Нет токена");
            return;
        }
        StartCoroutine(LoadSettingsRoutine(token, onSuccess, onError));
    }

    public void GetLocalSettings(out float master, out float music, out float sfx)
    {
        master = PlayerPrefs.GetFloat("Audio_Master", 1f);
        music  = PlayerPrefs.GetFloat("Audio_Music",  1f);
        sfx    = PlayerPrefs.GetFloat("Audio_SFX",    1f);
    }

    // ---------- Смена пароля ----------

    public void ChangePassword(string oldPassword, string newPassword,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            onError?.Invoke("Введите текущий пароль");
            return;
        }
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            onError?.Invoke("Новый пароль не может быть пустым");
            return;
        }
        if (newPassword.Length < 4)
        {
            onError?.Invoke("Новый пароль должен быть не короче 4 символов");
            return;
        }
        if (oldPassword == newPassword)
        {
            onError?.Invoke("Новый пароль совпадает со старым");
            return;
        }

        string token = PlayerPrefs.GetString("JWT_Token", "");
        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Нужно войти в аккаунт");
            return;
        }

        StartCoroutine(ChangePasswordRoutine(oldPassword, newPassword, token, onSuccess, onError));
    }

    // ---------- Валидация ----------

    private bool ValidateCredentials(string login, string password, System.Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            onError?.Invoke("Логин не может быть пустым");
            return false;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            onError?.Invoke("Пароль не может быть пустым");
            return false;
        }
        if (login.Trim().Length < 3)
        {
            onError?.Invoke("Логин должен быть не короче 3 символов");
            return false;
        }
        if (password.Length < 4)
        {
            onError?.Invoke("Пароль должен быть не короче 4 символов");
            return false;
        }
        return true;
    }

    // ---------- Префы ----------

    private void ApplyLevelsToPrefs(List<int> levels)
    {
        if (levels == null) return;

        foreach (int lvl in levels)
        {
            if (lvl < 1 || lvl > maxLevel) continue;

            PlayerPrefs.SetInt("Level_" + lvl + "_Completed", 1);
            if (lvl + 1 <= maxLevel)
                PlayerPrefs.SetInt("Level_" + (lvl + 1) + "_Unlocked", 1);
        }
        PlayerPrefs.Save();
    }

    private void ClearProgressInPrefs()
    {
        for (int i = 1; i <= maxLevel; i++)
        {
            PlayerPrefs.DeleteKey("Level_" + i + "_Completed");
            PlayerPrefs.DeleteKey("Level_" + i + "_Unlocked");
        }
        PlayerPrefs.Save();
    }

    private void WriteSettingsToPrefs(float master, float music, float sfx)
    {
        PlayerPrefs.SetFloat("Audio_Master", Mathf.Clamp01(master));
        PlayerPrefs.SetFloat("Audio_Music",  Mathf.Clamp01(music));
        PlayerPrefs.SetFloat("Audio_SFX",    Mathf.Clamp01(sfx));
        PlayerPrefs.Save();
    }

    private void ClearSettingsInPrefs()
    {
        PlayerPrefs.DeleteKey("Audio_Master");
        PlayerPrefs.DeleteKey("Audio_Music");
        PlayerPrefs.DeleteKey("Audio_SFX");
        PlayerPrefs.Save();
    }

    // ---------- Корутины ----------

    private IEnumerator RegisterRoutine(string login, string password,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonUtility.ToJson(new AuthRequest { login = login, password = password });

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/auth/register", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke("Аккаунт создан");
            }
            else
            {
                string body = SafeBody(req);
                Debug.Log($"[REGISTER] code={req.responseCode} error={req.error} body={body}");
                onError?.Invoke(BuildErrorMessage(body, req));
            }
        }
    }

    private IEnumerator LoginRoutine(string login, string password,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonUtility.ToJson(new AuthRequest { login = login, password = password });

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/auth/login", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string body = SafeBody(req);

                if (req.responseCode == 401 || req.responseCode == 400)
                {
                    Debug.Log($"[LOGIN] Отклонено сервером (code={req.responseCode}).");
                    onError?.Invoke("Неверный логин или пароль");
                }
                else
                {
                    Debug.LogError($"[LOGIN] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
                yield break;
            }

            string response = SafeBody(req);
            Debug.Log("[LOGIN RAW] " + response);

            LoginResponse data = TryParse<LoginResponse>(response, "[LOGIN]");
            if (data == null)
            {
                onError?.Invoke("Сервер вернул некорректный ответ");
                yield break;
            }

            string token = !string.IsNullOrEmpty(data.token) ? data.token : data.access_token;

            if (string.IsNullOrEmpty(token) || token == "null")
            {
                Debug.LogError($"[LOGIN] Сервер не вернул токен. response={response}");
                onError?.Invoke("Сервер не вернул токен");
                yield break;
            }

            if (token.StartsWith("Bearer "))
                token = token.Substring(7);

            if (token.Split('.').Length != 3)
            {
                Debug.LogError($"[LOGIN] Некорректный токен. token='{token}' response={response}");
                onError?.Invoke("Сервер вернул некорректный токен");
                yield break;
            }

            PlayerPrefs.SetString("JWT_Token", token);

            ClearProgressInPrefs();
            ClearSettingsInPrefs();

            PlayerPrefs.Save();
            OnProgressUpdated?.Invoke();
            onSuccess?.Invoke("Вход выполнен");

            LoadProgress(
                onSuccess: (levels) =>
                {
                    ApplyLevelsToPrefs(levels);
                    Debug.Log($"[API] Прогресс после логина синхронизирован. Уровней: {levels.Count}");
                    OnProgressUpdated?.Invoke();
                },
                onError: (err) => Debug.Log("[API] LoadProgress после логина не удался: " + err)
            );

            LoadSettings(
                onSuccess: (m, mu, s) => Debug.Log($"[API] Настройки после логина: {m:F2}/{mu:F2}/{s:F2}"),
                onError: (err) => Debug.Log("[API] LoadSettings после логина не удался: " + err)
            );
        }
    }

    private IEnumerator SaveProgressRoutine(int levelId, string token)
    {
        string json = JsonUtility.ToJson(new SaveProgressRequest { level_id = levelId });

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/progress/", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[SAVE] Уровень {levelId} сохранён на сервере.");
            }
            else
            {
                string body = SafeBody(req);

                if (req.responseCode == 401)
                {
                    Debug.Log("[SAVE] Токен протух — удаляем JWT_Token.");
                    PlayerPrefs.DeleteKey("JWT_Token");
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.LogError($"[SAVE] result={req.result} code={req.responseCode} error={req.error} body={body}");
                }
            }
        }
    }

    private IEnumerator LoadProgressRoutine(string token,
        System.Action<List<int>> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/progress/", "GET"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string body = SafeBody(req);

                if (req.responseCode == 401)
                {
                    Debug.Log("[LOAD] Токен протух — удаляем JWT_Token.");
                    PlayerPrefs.DeleteKey("JWT_Token");
                    PlayerPrefs.Save();
                    onError?.Invoke("Токен протух");
                }
                else
                {
                    Debug.LogError($"[LOAD] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
                yield break;
            }

            string response = SafeBody(req);
            Debug.Log("[LOAD RAW] " + response);

            ProgressResponse data = TryParse<ProgressResponse>(response, "[LOAD]");
            if (data == null)
            {
                onError?.Invoke("Сервер вернул некорректный ответ");
                yield break;
            }

            List<int> levels = data.completed_levels ?? new List<int>();
            onSuccess?.Invoke(levels);
        }
    }

    private IEnumerator ResetProgressRoutine(string token,
        System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/progress/", "DELETE"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[RESET] Прогресс удалён на сервере.");
                onSuccess?.Invoke("Прогресс удалён");
            }
            else
            {
                string body = SafeBody(req);

                if (req.responseCode == 401)
                {
                    Debug.Log("[RESET] Токен протух — удаляем JWT_Token.");
                    PlayerPrefs.DeleteKey("JWT_Token");
                    PlayerPrefs.Save();
                    onError?.Invoke("Токен протух, войдите заново");
                }
                else
                {
                    Debug.LogError($"[RESET] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
            }
        }
    }

    private IEnumerator ChangePasswordRoutine(string oldPassword, string newPassword,
        string token, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonUtility.ToJson(new ChangePasswordRequest
        {
            old_password = oldPassword,
            new_password = newPassword
        });

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/auth/change-password", "PUT"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[PASSWORD] Пароль изменён.");
                onSuccess?.Invoke("Пароль изменён");
            }
            else
            {
                string body = SafeBody(req);
                var err = TryParse<ErrorResponse>(body, "[PASSWORD]");
                string msg = err?.error ?? err?.message ?? err?.msg;

                if (req.responseCode == 401)
                {
                    if (!string.IsNullOrEmpty(msg))
                    {
                        Debug.Log($"[PASSWORD] Отклонено: {msg}");
                        onError?.Invoke(msg);
                    }
                    else
                    {
                        Debug.Log("[PASSWORD] Токен протух или неверный старый пароль.");
                        onError?.Invoke("Неверный текущий пароль или токен протух");
                    }
                }
                else if (req.responseCode == 400)
                {
                    onError?.Invoke(!string.IsNullOrEmpty(msg) ? msg : "Некорректные данные");
                }
                else
                {
                    Debug.LogError($"[PASSWORD] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
            }
        }
    }

    private IEnumerator SaveSettingsRoutine(float master, float music, float sfx,
        string token, System.Action<string> onSuccess, System.Action<string> onError)
    {
        string json = JsonUtility.ToJson(new SettingsDto
        {
            master = Mathf.Clamp01(master),
            music  = Mathf.Clamp01(music),
            sfx    = Mathf.Clamp01(sfx)
        });

        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/settings/", "PUT"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[SETTINGS] Настройки сохранены на сервере.");
                onSuccess?.Invoke("Настройки сохранены");
            }
            else
            {
                string body = SafeBody(req);

                if (req.responseCode == 401)
                {
                    Debug.Log("[SETTINGS] Токен протух — удаляем JWT_Token.");
                    PlayerPrefs.DeleteKey("JWT_Token");
                    PlayerPrefs.Save();
                    onError?.Invoke("Токен протух, войдите заново");
                }
                else
                {
                    Debug.LogError($"[SETTINGS] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
            }
        }
    }

    private IEnumerator LoadSettingsRoutine(string token,
        System.Action<float, float, float> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest req = new UnityWebRequest($"{baseUrl}/settings/", "GET"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", "Bearer " + token);
            req.timeout = timeoutSeconds;

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string body = SafeBody(req);

                if (req.responseCode == 401)
                {
                    Debug.Log("[SETTINGS] Токен протух — удаляем JWT_Token.");
                    PlayerPrefs.DeleteKey("JWT_Token");
                    PlayerPrefs.Save();
                    onError?.Invoke("Токен протух");
                }
                else
                {
                    Debug.LogError($"[SETTINGS] result={req.result} code={req.responseCode} error={req.error} body={body}");
                    onError?.Invoke(BuildErrorMessage(body, req));
                }
                yield break;
            }

            string response = SafeBody(req);
            Debug.Log("[SETTINGS RAW] " + response);

            SettingsDto data = TryParse<SettingsDto>(response, "[SETTINGS]");
            if (data == null)
            {
                onError?.Invoke("Сервер вернул некорректный ответ");
                yield break;
            }

            float master = Mathf.Clamp01(data.master);
            float music  = Mathf.Clamp01(data.music);
            float sfx    = Mathf.Clamp01(data.sfx);

            WriteSettingsToPrefs(master, music, sfx);
            Debug.Log($"[SETTINGS] Загружено с сервера: master={master:F2}, music={music:F2}, sfx={sfx:F2}");
            onSuccess?.Invoke(master, music, sfx);
        }
    }

    // ---------- Хелперы ----------

    private string SafeBody(UnityWebRequest req)
    {
        if (req == null || req.downloadHandler == null) return "";
        try { return req.downloadHandler.text ?? ""; }
        catch { return ""; }
    }

    private T TryParse<T>(string json, string logTag) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"{logTag} Не удалось распарсить JSON: {e.Message}. Тело: {json}");
            return null;
        }
    }

    private string BuildErrorMessage(string body, UnityWebRequest req)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            var err = TryParse<ErrorResponse>(body, "[ERR]");
            if (err != null)
            {
                if (!string.IsNullOrEmpty(err.msg)) return err.msg;
                if (!string.IsNullOrEmpty(err.message)) return err.message;
                if (!string.IsNullOrEmpty(err.error)) return err.error;
            }
            return body.Length > 200 ? body.Substring(0, 200) + "..." : body;
        }

        if (req != null && !string.IsNullOrEmpty(req.error))
            return req.error;

        return "Неизвестная ошибка сети";
    }
}