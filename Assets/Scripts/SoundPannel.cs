using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SoundPanel : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Ползунки")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Тексты значений")]
    public TMP_Text masterText;
    public TMP_Text musicText;
    public TMP_Text sfxText;

    [Header("Навигация")]
    public GameObject levelMenu;

    [Header("Сохранение")]
    public float saveDebounce = 0.5f;

    private float pendingSaveTime = -1f;
    private bool isApplyingFromServer = false;

    void Start()
    {
        float master, music, sfx;
        if (ApiManager.Instance != null)
        {
            ApiManager.Instance.GetLocalSettings(out master, out music, out sfx);
        }
        else
        {
            master = PlayerPrefs.GetFloat("Audio_Master", 1f);
            music  = PlayerPrefs.GetFloat("Audio_Music",  1f);
            sfx    = PlayerPrefs.GetFloat("Audio_SFX",    1f);
        }

        isApplyingFromServer = true;
        if (masterSlider != null) masterSlider.value = master;
        if (musicSlider  != null) musicSlider.value  = music;
        if (sfxSlider    != null) sfxSlider.value    = sfx;
        isApplyingFromServer = false;

        ApplyToMixer(master, music, sfx);
        UpdateTexts(master, music, sfx);

        if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
        if (musicSlider  != null) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider    != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);

        if (ApiManager.Instance != null)
        {
            ApiManager.Instance.LoadSettings(
                onSuccess: (m, mu, s) =>
                {
                    isApplyingFromServer = true;
                    if (masterSlider != null) masterSlider.value = m;
                    if (musicSlider  != null) musicSlider.value  = mu;
                    if (sfxSlider    != null) sfxSlider.value    = s;
                    isApplyingFromServer = false;

                    ApplyToMixer(m, mu, s);
                    UpdateTexts(m, mu, s);
                    Debug.Log($"[SOUND] Настройки загружены с сервера: {m:F2}/{mu:F2}/{s:F2}");
                },
                onError: (err) => Debug.Log("[SOUND] LoadSettings не удался: " + err)
            );
        }
    }

    void Update()
    {
        if (pendingSaveTime > 0f && Time.unscaledTime >= pendingSaveTime)
        {
            pendingSaveTime = -1f;
            SaveNow();
        }
    }

    // ---------- Обработчики слайдеров ----------

    void OnMasterChanged(float value)
    {
        AudioListener.volume = value;
        if (masterText != null) masterText.text = $"{(value * 100):F0}%";
        if (!isApplyingFromServer) ScheduleSave();
    }

    void OnMusicChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        if (musicText != null) musicText.text = $"{(value * 100):F0}%";
        if (!isApplyingFromServer) ScheduleSave();
    }

    void OnSFXChanged(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        if (sfxText != null) sfxText.text = $"{(value * 100):F0}%";
        if (!isApplyingFromServer) ScheduleSave();
    }

    // ---------- Сохранение ----------

    private void ScheduleSave()
    {
        pendingSaveTime = Time.unscaledTime + saveDebounce;
    }

    private void SaveNow()
    {
        float m  = masterSlider != null ? masterSlider.value : 1f;
        float mu = musicSlider  != null ? musicSlider.value  : 1f;
        float s  = sfxSlider    != null ? sfxSlider.value    : 1f;

        if (ApiManager.Instance != null)
        {
            ApiManager.Instance.SaveSettings(m, mu, s,
                onSuccess: (msg) => Debug.Log("[SOUND] " + msg),
                onError: (err)   => Debug.LogWarning("[SOUND] SaveSettings error: " + err)
            );
        }
        else
        {
            PlayerPrefs.SetFloat("Audio_Master", m);
            PlayerPrefs.SetFloat("Audio_Music",  mu);
            PlayerPrefs.SetFloat("Audio_SFX",    s);
            PlayerPrefs.Save();
        }
    }

    // ---------- Применение ----------

    private void ApplyToMixer(float master, float music, float sfx)
    {
        AudioListener.volume = master;
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(music, 0.0001f)) * 20);
            audioMixer.SetFloat("SFXVolume",   Mathf.Log10(Mathf.Max(sfx,   0.0001f)) * 20);
        }
    }

    private void UpdateTexts(float master, float music, float sfx)
    {
        if (masterText != null) masterText.text = $"{(master * 100):F0}%";
        if (musicText  != null) musicText.text  = $"{(music  * 100):F0}%";
        if (sfxText    != null) sfxText.text    = $"{(sfx    * 100):F0}%";
    }

    // ---------- Кнопки ----------

    public void OnBackClicked()
    {
        if (pendingSaveTime > 0f)
        {
            pendingSaveTime = -1f;
            SaveNow();
        }

        gameObject.SetActive(false);
        if (levelMenu != null) levelMenu.SetActive(true);
    }
}