using UnityEngine;

public class RotationBasedMusic : MonoBehaviour
{
    [System.Serializable]
    public class MusicTrigger
    {
        public string name;                // Название триггера (для удобства)
        public Vector3 axis = Vector3.up; // Ось, по которой отслеживаем поворот (локальная или мировая)
        public float angleThreshold = 45f; // Градус, при котором переключаем музыку
        public bool useRange = false;      // Если true, срабатывает при попадании угла в диапазон
        public float angleMin = 0f;
        public float angleMax = 360f;
        public AudioClip musicClip;        // Музыка для этого состояния
        public float crossfadeDuration = 1f; // Длительность плавного перехода (0 = мгновенно)
    }

    [Header("Rotation Settings")]
    public Space rotationSpace = Space.Self; // World или Self
    public bool continuousCheck = true;      // Постоянно проверяем (иначе только при старте)

    [Header("Audio")]
    public AudioSource audioSource;          // Источник для музыки (лучше создать отдельный)
    public MusicTrigger[] musicTriggers;

    private MusicTrigger activeTrigger;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("RotationBasedMusic: нет AudioSource! Добавьте компонент.");
            return;
        }

        // Первоначальная проверка
        CheckRotationAndUpdateMusic();
    }

    private void Update()
    {
        if (continuousCheck)
            CheckRotationAndUpdateMusic();
    }

    private void CheckRotationAndUpdateMusic()
    {
        if (musicTriggers == null || musicTriggers.Length == 0) return;

        foreach (var trigger in musicTriggers)
        {
            if (IsRotationInTrigger(trigger))
            {
                if (activeTrigger != trigger)
                {
                    SwitchToMusic(trigger);
                    activeTrigger = trigger;
                }
                break; // сработал первый подходящий триггер
            }
        }
    }

    private bool IsRotationInTrigger(MusicTrigger trigger)
    {
        // Получаем текущий угол по указанной оси
        float angle;
        if (rotationSpace == Space.World)
        {
            Vector3 euler = transform.eulerAngles;
            angle = GetAngleOnAxis(euler, trigger.axis);
        }
        else
        {
            Vector3 localEuler = transform.localEulerAngles;
            angle = GetAngleOnAxis(localEuler, trigger.axis);
        }

        if (trigger.useRange)
        {
            // Нормализуем угол в диапазон 0-360, затем проверяем попадание в [min, max]
            float normAngle = NormalizeAngle(angle);
            return normAngle >= trigger.angleMin && normAngle <= trigger.angleMax;
        }
        else
        {
            // Проверяем равенство с погрешностью (так как float)
            return Mathf.Abs(Mathf.DeltaAngle(angle, trigger.angleThreshold)) < 0.1f;
        }
    }

    private float GetAngleOnAxis(Vector3 eulerAngles, Vector3 axis)
    {
        // Приводим ось к главным направлениям (упрощённо)
        if (axis == Vector3.right) return eulerAngles.x;
        if (axis == Vector3.up) return eulerAngles.y;
        if (axis == Vector3.forward) return eulerAngles.z;
        // Если ось не стандартная, можно брать скалярное произведение, но для простоты используем Y
        Debug.LogWarning("Используется нестандартная ось. Результат может быть неточным.");
        return eulerAngles.y;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    private void SwitchToMusic(MusicTrigger newTrigger)
    {
        if (newTrigger.musicClip == null)
        {
            Debug.LogWarning("Музыкальный клип не назначен для триггера " + newTrigger.name);
            return;
        }

        // Если просим плавный переход
        if (newTrigger.crossfadeDuration > 0 && audioSource.isPlaying)
        {
            StartCoroutine(CrossfadeMusic(newTrigger.musicClip, newTrigger.crossfadeDuration));
        }
        else
        {
            audioSource.clip = newTrigger.musicClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log($"Смена музыки: {activeTrigger?.name ?? "none"} -> {newTrigger.name}");
    }

    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        // Плавно убавляем текущую
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        // Плавно прибавляем новую
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, timer / duration);
            yield return null;
        }
        audioSource.volume = startVolume;
    }
}