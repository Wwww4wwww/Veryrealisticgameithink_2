using System.Collections;
using UnityEngine;

public class MusicZone : MonoBehaviour
{
    [Header("Zone Sounds (layered)")]
    public ZoneSound[] zoneSounds;

    [Header("Transition")]
    public float fadeDuration = 1.5f;
    public bool playOnEnter = true;
    public bool stopOnExit = true;

    [Header("Global Music Control")]
    public bool fadeGlobalMusic = true;
    public bool stopGlobalMusicInZone = true;   // Полностью останавливать глобальную музыку в зоне

    private bool isInside = false;
    private AudioSource[] audioSources;
    private AudioSource globalAudioSource;
    private float originalGlobalVolume;

    [System.Serializable]
    public class ZoneSound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
        public bool loop = true;
        public bool randomPitch = false;
        public float minPitch = 0.9f;
        public float maxPitch = 1.1f;
        public float randomDelayMin = 0f;
        public float randomDelayMax = 0f;
    }

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            globalAudioSource = MusicManager.Instance.GetComponent<AudioSource>();
            if (globalAudioSource != null)
            {
                originalGlobalVolume = globalAudioSource.volume;
                Debug.Log($"[MusicZone] Original global volume = {originalGlobalVolume}");
            }
        }

        // Создаём зональные аудиосорсы
        audioSources = new AudioSource[zoneSounds.Length];
        for (int i = 0; i < zoneSounds.Length; i++)
        {
            GameObject soundObj = new GameObject($"ZoneSound_{i}");
            soundObj.transform.SetParent(transform);
            audioSources[i] = soundObj.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].loop = zoneSounds[i].loop;
            audioSources[i].volume = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInside && playOnEnter)
        {
            isInside = true;
            StartCoroutine(FadeAllIn());
            if (fadeGlobalMusic)
                StartCoroutine(FadeOutGlobalMusic());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInside && stopOnExit)
        {
            isInside = false;
            StartCoroutine(FadeAllOut());
            if (fadeGlobalMusic)
                StartCoroutine(FadeInGlobalMusic());
        }
    }

    private IEnumerator FadeAllIn()
    {
        // Запуск зональных звуков
        for (int i = 0; i < zoneSounds.Length; i++)
        {
            if (zoneSounds[i].clip != null && audioSources[i] != null)
            {
                if (zoneSounds[i].randomPitch)
                    audioSources[i].pitch = Random.Range(zoneSounds[i].minPitch, zoneSounds[i].maxPitch);
                float delay = Random.Range(zoneSounds[i].randomDelayMin, zoneSounds[i].randomDelayMax);
                if (delay > 0) yield return new WaitForSeconds(delay);
                audioSources[i].clip = zoneSounds[i].clip;
                audioSources[i].Play();
            }
        }
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            for (int i = 0; i < zoneSounds.Length; i++)
                if (audioSources[i] != null)
                    audioSources[i].volume = Mathf.Lerp(0f, zoneSounds[i].volume, t);
            yield return null;
        }
        for (int i = 0; i < zoneSounds.Length; i++)
            if (audioSources[i] != null) audioSources[i].volume = zoneSounds[i].volume;
    }

    private IEnumerator FadeAllOut()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            for (int i = 0; i < zoneSounds.Length; i++)
                if (audioSources[i] != null && audioSources[i].isPlaying)
                    audioSources[i].volume = Mathf.Lerp(zoneSounds[i].volume, 0f, t);
            yield return null;
        }
        for (int i = 0; i < zoneSounds.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].Stop();
                audioSources[i].volume = 0f;
            }
        }
    }

    private IEnumerator FadeOutGlobalMusic()
    {
        if (globalAudioSource == null) yield break;
        if (stopGlobalMusicInZone)
        {
            float startVolume = globalAudioSource.volume;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                globalAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            globalAudioSource.Stop();
            globalAudioSource.volume = 0f; // сбрасываем, чтобы при выходе запустить с 0
        }
        else
        {
            // приглушаем до 0, но не останавливаем
            float startVolume = globalAudioSource.volume;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                globalAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }
            globalAudioSource.volume = 0f;
        }
    }

    private IEnumerator FadeInGlobalMusic()
    {
        if (globalAudioSource == null) yield break;

        // Определяем целевую громкость (приоритет: MusicManager.musicVolume, потом originalGlobalVolume)
        float targetVolume = originalGlobalVolume;
        if (MusicManager.Instance != null)
            targetVolume = MusicManager.Instance.musicVolume; // предполагаем, что есть public float musicVolume

        Debug.Log($"[MusicZone] FadeInGlobalMusic, target volume = {targetVolume}");

        if (stopGlobalMusicInZone)
        {
            // Если музыка не играет, запускаем её
            if (!globalAudioSource.isPlaying)
            {
                AudioClip globalClip = MusicManager.Instance != null ? MusicManager.Instance.globalMusic : null;
                if (globalClip != null)
                {
                    globalAudioSource.clip = globalClip;
                    globalAudioSource.loop = true;
                    globalAudioSource.volume = 0f;
                    globalAudioSource.Play();
                    Debug.Log("[MusicZone] Global music restarted");
                }
                else
                {
                    Debug.LogWarning("[MusicZone] No global music clip available!");
                }
            }

            // Плавно увеличиваем громкость до targetVolume
            float timer = 0f;
            float startVol = globalAudioSource.volume;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                globalAudioSource.volume = Mathf.Lerp(startVol, targetVolume, t);
                yield return null;
            }
            globalAudioSource.volume = targetVolume;
            Debug.Log($"[MusicZone] Global music volume set to {targetVolume}");
        }
        else
        {
            // Если не останавливали, просто возвращаем громкость
            float startVolume = globalAudioSource.volume;
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                globalAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
                yield return null;
            }
            globalAudioSource.volume = targetVolume;
        }
    }
    public void StopZoneSounds()
    {
        StopAllCoroutines();
        StartCoroutine(FadeAllOut());
    }
}