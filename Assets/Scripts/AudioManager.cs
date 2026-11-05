using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Источники звука")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource tickSource; // <--- НОВЫЙ ИСТОЧНИК ДЛЯ ТИКАНЬЯ

    [Header("Фоновые звуки")]
    public AudioClip backgroundMusic;
    public AudioClip clockTickLoop; // <--- ЗАЦИКЛЕННЫЙ ЗВУК ЧАСОВ

    [Header("Эффекты")]
    public AudioClip buttonClick;
    public AudioClip deathSound;
    public AudioClip doorOpen;
    public AudioClip footstepSound;
    public AudioClip changePhase;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (audioMixer != null)
        {
            float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

            AudioListener.volume = masterVol;
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20);
        }

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    // === НОВЫЕ МЕТОДЫ ДЛЯ ЧАСОВ ===
    public void StartClockTick()
    {
        if (tickSource != null && clockTickLoop != null)
        {
            tickSource.clip = clockTickLoop;
            tickSource.loop = true; // Зацикливаем
            tickSource.Play();
        }
    }

    public void StopClockTick()
    {
        if (tickSource != null)
        {
            tickSource.Stop();
        }
    }

    public void PauseClockTick()
    {
        if (tickSource != null && tickSource.isPlaying)
            tickSource.Pause();
    }

    public void ResumeClockTick()
    {
        if (tickSource != null && !tickSource.isPlaying)
            tickSource.UnPause();
    }
}