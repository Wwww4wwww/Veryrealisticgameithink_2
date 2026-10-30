using UnityEngine;

public class LightZone : MonoBehaviour
{
    [Header("Zone Light Settings")]
    public Color zoneColor = Color.red;
    public float zoneIntensity = 1.2f;
    public float transitionDuration = -1f; // -1 = использовать глобальную

    private bool isInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isInside)
        {
            isInside = true;
            if (LightController.Instance != null)
            {
                LightController.Instance.SetLight(zoneColor, zoneIntensity, transitionDuration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isInside)
        {
            isInside = false;
            if (LightController.Instance != null)
            {
                LightController.Instance.ResetToDefault(transitionDuration);
            }
        }
    }
}