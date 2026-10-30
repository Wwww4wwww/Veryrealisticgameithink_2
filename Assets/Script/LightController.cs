using System.Collections;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public static LightController Instance;

    [Header("Default Settings")]
    public Color defaultColor = Color.white;
    public float defaultIntensity = 1f;

    [Header("Transition")]
    public float transitionDuration = 1.5f;

    private Light directionalLight;
    private Coroutine transitionRoutine;

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

        directionalLight = GetComponent<Light>();
        if (directionalLight == null)
        {
            Debug.LogError("LightController: нет компонента Light!");
            return;
        }

        defaultColor = directionalLight.color;
        defaultIntensity = directionalLight.intensity;
    }

    public void SetLight(Color targetColor, float targetIntensity, float customDuration = -1f)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionLight(targetColor, targetIntensity, customDuration));
    }

    public void ResetToDefault(float customDuration = -1f)
    {
        SetLight(defaultColor, defaultIntensity, customDuration);
    }

    private IEnumerator TransitionLight(Color targetColor, float targetIntensity, float customDuration)
    {
        float duration = customDuration > 0 ? customDuration : transitionDuration;
        Color startColor = directionalLight.color;
        float startIntensity = directionalLight.intensity;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            directionalLight.color = Color.Lerp(startColor, targetColor, t);
            directionalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        directionalLight.color = targetColor;
        directionalLight.intensity = targetIntensity;
        transitionRoutine = null;
    }
}