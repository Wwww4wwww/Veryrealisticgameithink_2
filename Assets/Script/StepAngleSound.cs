using UnityEngine;

public class StepAngleSound : MonoBehaviour
{
    [Header("Настройки")]
    public Space rotationSpace = Space.Self;   // Self = локальные углы, World = глобальные
    public Axis axis = Axis.Y;                 // Ось, по которой отслеживать поворот
    public float step = 180f;                  // Шаг в градусах (каждые 180°)
    public AudioClip sound;                    // Звук, который играть на каждом шаге
    public AudioSource audioSource;            // Если не указан, добавится автоматически

    public enum Axis { X, Y, Z }

    private float lastStepIndex;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Запоминаем текущий "квантованный" индекс шага
        float startAngle = GetCurrentAngle();
        lastStepIndex = Mathf.Round(startAngle / step);
    }

    void Update()
    {
        if (sound == null || step <= 0.001f) return;

        float currentAngle = GetCurrentAngle();
        float currentStepIndex = Mathf.Round(currentAngle / step);

        // Если индекс изменился – пересекли порог
        if (!Mathf.Approximately(currentStepIndex, lastStepIndex))
        {
            audioSource.PlayOneShot(sound);
            lastStepIndex = currentStepIndex;
        }
    }

    private float GetCurrentAngle()
    {
        Vector3 angles = rotationSpace == Space.Self ? transform.localEulerAngles : transform.eulerAngles;
        float value = axis switch
        {
            Axis.X => angles.x,
            Axis.Y => angles.y,
            Axis.Z => angles.z,
            _ => 0f
        };
        // Нормализуем угол в диапазон 0–360
        value %= 360f;
        if (value < 0) value += 360f;
        return value;
    }
}