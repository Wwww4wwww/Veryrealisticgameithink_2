using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Global Music")]
    public AudioClip globalMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public float crossfadeDuration = 1.5f;

    private AudioSource audioSource;
    private AudioClip currentClip;
    private Coroutine fadeRoutine;

    private void Awake()
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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.volume = 0f;
        audioSource.playOnAwake = false;

        if (globalMusic != null)
            PlayMusic(globalMusic, true);
    }

    public void PlayMusic(AudioClip clip, bool fadeIn = true, float duration = -1f)
    {
        if (clip == null) return;
        if (clip == currentClip && audioSource.isPlaying) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        float fadeTime = duration > 0 ? duration : crossfadeDuration;
        fadeRoutine = StartCoroutine(Crossfade(clip, fadeTime));
    }

    private IEnumerator Crossfade(AudioClip newClip, float duration)
    {
        // Если ничего не играет, просто начинаем с нового
        if (!audioSource.isPlaying || currentClip == null)
        {
            audioSource.clip = newClip;
            audioSource.volume = 0f;
            audioSource.Play();
            yield return StartCoroutine(FadeVolume(0f, musicVolume, duration));
            currentClip = newClip;
            fadeRoutine = null;
            yield break;
        }

        // Плавное затухание текущей
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        audioSource.Stop();

        // Начинаем новый клип
        audioSource.clip = newClip;
        audioSource.volume = 0f;
        audioSource.Play();

        // Плавное увеличение громкости
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, musicVolume, timer / duration);
            yield return null;
        }
        audioSource.volume = musicVolume;
        currentClip = newClip;
        fadeRoutine = null;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }
        audioSource.volume = to;
    }

    public void ReturnToGlobalMusic()
    {
        if (globalMusic != null)
            PlayMusic(globalMusic);
    }
public void StopMusicWithFade(float fadeDuration = -1f)
{
    if (fadeRoutine != null)
        StopCoroutine(fadeRoutine);
    
    float duration = fadeDuration > 0 ? fadeDuration : crossfadeDuration;
    fadeRoutine = StartCoroutine(FadeOutAndStop(duration));
}

private IEnumerator FadeOutAndStop(float duration)
{
    if (!audioSource.isPlaying) yield break;
    
    float startVolume = audioSource.volume;
    float timer = 0f;
    
    while (timer < duration)
    {
        timer += Time.deltaTime;
        float t = timer / duration;
        audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
        yield return null;
    }
    
    audioSource.Stop();
    audioSource.volume = startVolume; // сброс для будущего воспроизведения
    currentClip = null;
    fadeRoutine = null;
    
    Debug.Log("[MusicManager] Music stopped with fade");
}
}
