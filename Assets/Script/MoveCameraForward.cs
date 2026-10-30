using UnityEngine;

public class MoveCameraForward : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 1.5f;                // Скорость движения (ед/сек)
    [SerializeField] private bool moveOnStart = true;           // Начать движение сразу при старте
    [SerializeField] private bool useSmoothing = false;         // Использовать плавное ускорение/замедление
    [SerializeField] private float accelerationTime = 1f;       // Время достижения полной скорости

    [Header("Limits")]
    [SerializeField] private bool limitDistance = false;        // Ограничить расстояние от старта
    [SerializeField] private float maxDistance = 50f;           // Максимальное расстояние

    private Vector3 startPosition;
    private float currentSpeed;
    private bool isMoving;
    private float distanceTraveled;

    private void Start()
    {
        startPosition = transform.position;
        isMoving = moveOnStart;
        currentSpeed = moveOnStart ? speed : 0f;
    }

    private void Update()
    {
        if (!isMoving) return;

        // Плавное изменение скорости
        if (useSmoothing)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, (speed / accelerationTime) * Time.deltaTime);
        }
        else
        {
            currentSpeed = speed;
        }

        // Перемещение вперёд относительно камеры
        Vector3 movement = transform.forward * currentSpeed * Time.deltaTime;
        transform.position += movement;

        // Проверка ограничения расстояния
        if (limitDistance)
        {
            distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                isMoving = false;
                // Опционально: вернуть камеру на максимальную дистанцию
                Vector3 direction = (transform.position - startPosition).normalized;
                transform.position = startPosition + direction * maxDistance;
            }
        }
    }

    // Публичные методы управления
    public void StartMoving()
    {
        isMoving = true;
        if (!useSmoothing) currentSpeed = speed;
    }

    public void StopMoving()
    {
        isMoving = false;
        currentSpeed = 0f;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        distanceTraveled = 0f;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }

    // Визуализация максимальной дистанции
    private void OnDrawGizmosSelected()
    {
        if (limitDistance)
        {
            Gizmos.color = Color.green;
            Vector3 center = Application.isPlaying ? startPosition : transform.position;
            Gizmos.DrawWireSphere(center, maxDistance);
        }
    }
}