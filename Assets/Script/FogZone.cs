using UnityEngine;
using System.Collections;
public class FogZone : MonoBehaviour
{
    [Header("Fog Settings")]
    public Color fogColor = Color.gray;
    public float fogDensity = 0.05f;      // Для Exponential режима
    public float fogStart = 10f;           // Для Linear режима
    public float fogEnd = 100f;            // Для Linear режима
    public float transitionDuration = 1.5f; // Длительность плавного перехода

    [Header("Fog Mode")]
    public FogMode fogMode = FogMode.Exponential;

    private bool isInside = false;
    private Color originalColor;
    private float originalDensity;
    private float originalStart;
    private float originalEnd;
    private FogMode originalMode;

    private void Start()
    {
        // Сохраняем исходные настройки тумана
        originalColor = RenderSettings.fogColor;
        originalDensity = RenderSettings.fogDensity;
        originalStart = RenderSettings.fogStartDistance;
        originalEnd = RenderSettings.fogEndDistance;
        originalMode = RenderSettings.fogMode;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInside)
        {
            isInside = true;
            StopAllCoroutines();
            StartCoroutine(TransitionToZoneFog());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInside)
        {
            isInside = false;
            StopAllCoroutines();
            StartCoroutine(TransitionToOriginalFog());
        }
    }

    private IEnumerator TransitionToZoneFog()
    {
        // Включаем туман, если он выключен
        RenderSettings.fog = true;
        
        float timer = 0f;
        
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startStart = RenderSettings.fogStartDistance;
        float startEnd = RenderSettings.fogEndDistance;
        FogMode startMode = RenderSettings.fogMode;

        // Меняем режим тумана мгновенно
        RenderSettings.fogMode = fogMode;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            
            // Плавно меняем цвет
            RenderSettings.fogColor = Color.Lerp(startColor, fogColor, t);
            
            // Меняем параметры в зависимости от режима
            if (fogMode == FogMode.Exponential || fogMode == FogMode.ExponentialSquared)
            {
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, fogDensity, t);
            }
            else if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(startStart, fogStart, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(startEnd, fogEnd, t);
            }
            
            yield return null;
        }
        
        // Фиксируем конечные значения
        RenderSettings.fogColor = fogColor;
        if (fogMode == FogMode.Exponential || fogMode == FogMode.ExponentialSquared)
            RenderSettings.fogDensity = fogDensity;
        else if (fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }
    }

    private IEnumerator TransitionToOriginalFog()
    {
        float timer = 0f;
        
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startStart = RenderSettings.fogStartDistance;
        float startEnd = RenderSettings.fogEndDistance;
        FogMode startMode = RenderSettings.fogMode;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            
            RenderSettings.fogColor = Color.Lerp(startColor, originalColor, t);
            
            if (originalMode == FogMode.Exponential || originalMode == FogMode.ExponentialSquared)
            {
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, originalDensity, t);
            }
            else if (originalMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(startStart, originalStart, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(startEnd, originalEnd, t);
            }
            
            yield return null;
        }
        
        // Восстанавливаем исходные настройки
        RenderSettings.fogColor = originalColor;
        RenderSettings.fogDensity = originalDensity;
        RenderSettings.fogStartDistance = originalStart;
        RenderSettings.fogEndDistance = originalEnd;
        RenderSettings.fogMode = originalMode;
        
        // Если исходный туман был выключен, выключаем
        if (!RenderSettings.fog)
            RenderSettings.fog = false;
    }
}