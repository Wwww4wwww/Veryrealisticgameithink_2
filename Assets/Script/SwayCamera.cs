using UnityEngine;

public class SwayCamera : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float swayAngle = 15f;        // Максимальный угол отклонения в градусах
    [SerializeField] private float swaySpeed = 1.5f;       // Скорость полного цикла (вперёд-назад)
    [SerializeField] private bool useSmoothDamping = true; // Использовать сглаживание
    [SerializeField] private float smoothTime = 0.3f;      // Время сглаживания

    [Header("Axis")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y; // Ось вращения

    private enum RotationAxis { X, Y, Z }

    private float currentAngle;
    private float targetAngle;
    private float angleVelocity; // Для SmoothDamp

    private void Start()
    {
        // Запоминаем начальный угол по выбранной оси
        currentAngle = GetCurrentAxisAngle();
        targetAngle = currentAngle;
    }

    private void Update()
    {
        // Вычисляем целевой угол с помощью синуса времени
        float sineValue = Mathf.Sin(Time.time * swaySpeed);
        float baseTargetAngle = GetInitialAxisAngle() + sineValue * swayAngle;

        if (useSmoothDamping)
        {
            targetAngle = baseTargetAngle;
            currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref angleVelocity, smoothTime);
        }
        else
        {
            currentAngle = baseTargetAngle;
        }

        ApplyRotation();
    }

    // Получает угол поворота объекта по выбранной оси в момент старта
    private float GetInitialAxisAngle()
    {
        return rotationAxis switch
        {
            RotationAxis.X => transform.eulerAngles.x,
            RotationAxis.Y => transform.eulerAngles.y,
            RotationAxis.Z => transform.eulerAngles.z,
            _ => 0f
        };
    }

    // Получает текущий угол по выбранной оси (используется для SmoothDamp)
    private float GetCurrentAxisAngle()
    {
        return rotationAxis switch
        {
            RotationAxis.X => transform.eulerAngles.x,
            RotationAxis.Y => transform.eulerAngles.y,
            RotationAxis.Z => transform.eulerAngles.z,
            _ => 0f
        };
    }

    // Применяет поворот
    private void ApplyRotation()
    {
        Vector3 euler = transform.eulerAngles;
        switch (rotationAxis)
        {
            case RotationAxis.X:
                euler.x = currentAngle;
                break;
            case RotationAxis.Y:
                euler.y = currentAngle;
                break;
            case RotationAxis.Z:
                euler.z = currentAngle;
                break;
        }
        transform.eulerAngles = euler;
    }
}