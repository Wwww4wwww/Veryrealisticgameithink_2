using System.Collections;
using UnityEngine;

public class ShipInputManager : MonoBehaviour
{
    [Header("References")]
    public LayerMask shipLayer;
    public Camera playerCamera;
    public GameObject player;

    [Header("Ship Music")]
    public AudioClip shipMusic;
    public float shipMusicVolume = 0.5f;
    public float musicFadeDuration = 1.5f;

    [Header("Ship Fog")]
    public bool disableFogInShip = true;
    public float fogFadeDuration = 1.5f;

    [Header("UI - Enter Prompt")]
    public GameObject shipPrompt;
    public TMPro.TextMeshProUGUI shipPromptText;

    [Header("UI - No Key Prompt")]
    public GameObject noKeyPrompt;
    public TMPro.TextMeshProUGUI noKeyPromptText;

    [Header("Key Requirement")]
    public bool requireKey = true;
    public string requiredItemName = "ShipKey";

    private SpaceShipController currentShip;
    private bool isInShip = false;
    private AudioSource shipAudioSource;
    private bool isLookingAtShip = false;
    private bool hasRequiredKey = false;

    // Для остановки зональной музыки
    private MusicZone[] activeMusicZones;
    private FogZone[] activeFogZones;

    // Оригинальные настройки тумана
    private Color originalFogColor;
    private float originalFogDensity;
    private float originalFogStart;
    private float originalFogEnd;
    private FogMode originalFogMode;
    private bool originalFogEnabled;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        
        if (playerCamera == null && player != null)
            playerCamera = player.GetComponentInChildren<Camera>();

        shipAudioSource = gameObject.AddComponent<AudioSource>();
        shipAudioSource.loop = true;
        shipAudioSource.volume = 0f;
        shipAudioSource.playOnAwake = false;

        // Сохраняем настройки тумана
        originalFogEnabled = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogStart = RenderSettings.fogStartDistance;
        originalFogEnd = RenderSettings.fogEndDistance;
        originalFogMode = RenderSettings.fogMode;

        // Скрываем UI при старте
        if (shipPrompt != null)
            shipPrompt.SetActive(false);
        if (noKeyPrompt != null)
            noKeyPrompt.SetActive(false);
    }

    private void Update()
    {
        // Проверяем, есть ли ключ у игрока
        if (requireKey)
        {
            hasRequiredKey = CheckPlayerHasItem(requiredItemName);
        }
        
        // Проверяем взгляд на корабль
        CheckLookingAtShip();
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isInShip)
                ExitShip();
            else if (isLookingAtShip)
            {
                if (requireKey && !hasRequiredKey)
                {
                    ShowNoKeyMessage();
                    return;
                }
                TryEnterShip();
            }
        }
    }

    private bool CheckPlayerHasItem(string itemName)
    {
        if (Inventory.Instance == null) return false;
        
        foreach (var slot in Inventory.Instance.slots)
        {
            if (slot.item != null && slot.item.itemName == itemName && slot.amount > 0)
            {
                return true;
            }
        }
        return false;
    }

    private void ShowNoKeyMessage()
    {
        Debug.Log("[ShipInputManager] No key! Cannot enter the ship.");
        
        if (noKeyPrompt != null)
        {
            StartCoroutine(ShowNoKeyMessageCoroutine());
        }
    }

    private IEnumerator ShowNoKeyMessageCoroutine()
    {
        noKeyPrompt.SetActive(true);
        if (noKeyPromptText != null)
            noKeyPromptText.text = $"You need {requiredItemName} to enter the ship!";
        
        yield return new WaitForSeconds(2f);
        
        noKeyPrompt.SetActive(false);
    }

    private void CheckLookingAtShip()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 10f, shipLayer))
        {
            if (hit.collider.GetComponent<SpaceShipController>() != null)
            {
                if (!isLookingAtShip)
                {
                    isLookingAtShip = true;
                    
                    // Показываем разную подсказку в зависимости от наличия ключа
                    if (requireKey && !hasRequiredKey)
                    {
                        ShowShipPrompt(false); // Не показываем обычную подсказку
                    }
                    else
                    {
                        ShowShipPrompt(true);
                    }
                }
                return;
            }
        }
        
        if (isLookingAtShip)
        {
            isLookingAtShip = false;
            ShowShipPrompt(false);
        }
    }

    private void ShowShipPrompt(bool show)
    {
        if (shipPrompt != null)
        {
            shipPrompt.SetActive(show);
            if (show && shipPromptText != null)
            {
                shipPromptText.text = $"Press F to enter the ship";
            }
        }
    }

    public void TryEnterShip()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, shipLayer))
        {
            SpaceShipController ship = hit.collider.GetComponent<SpaceShipController>();
            if (ship != null)
            {
                EnterShip(ship);
            }
        }
    }

    private void EnterShip(SpaceShipController ship)
    {
        Debug.Log("[ShipInputManager] Entering ship");
        
        currentShip = ship;
        isInShip = true;

        // --- ОСТАНАВЛИВАЕМ ВСЕ ЗОНАЛЬНЫЕ МУЗЫКИ ---
        StopAllZoneMusic();

        // --- ОСТАНАВЛИВАЕМ ГЛОБАЛЬНУЮ МУЗЫКУ ---
        StartCoroutine(FadeOutGlobalMusic(musicFadeDuration));

        // --- ЗАПУСКАЕМ КОРАБЕЛЬНУЮ МУЗЫКУ ---
        if (shipMusic != null)
            StartCoroutine(FadeInShipMusic(musicFadeDuration));

        // --- УБИРАЕМ ТУМАН ---
        if (disableFogInShip)
        {
            StopAllZoneFog();
            StartCoroutine(FadeOutFog(fogFadeDuration));
        }

        // Отключаем игрока
        if (player != null)
            player.SetActive(false);

        ship.TakeControl(null);
        
        // Скрываем подсказки
        if (shipPrompt != null)
            shipPrompt.SetActive(false);
        if (noKeyPrompt != null)
            noKeyPrompt.SetActive(false);
    }

    private void ExitShip()
    {
        Debug.Log("[ShipInputManager] Exiting ship");
        
        if (currentShip == null) return;
        if (currentShip.exitPoint == null)
        {
            Debug.LogError("[ExitShip] exitPoint is null!");
            return;
        }

        // --- ОСТАНАВЛИВАЕМ КОРАБЕЛЬНУЮ МУЗЫКУ ---
        StartCoroutine(FadeOutShipMusic(musicFadeDuration));

        // --- ВОЗВРАЩАЕМ ГЛОБАЛЬНУЮ МУЗЫКУ ---
        StartCoroutine(FadeInGlobalMusic(musicFadeDuration));

        // --- ВОЗВРАЩАЕМ ТУМАН ---
        if (disableFogInShip)
            StartCoroutine(FadeInFog(fogFadeDuration));

        // Включаем игрока
        if (player != null)
        {
            player.transform.SetParent(null);
            player.transform.position = currentShip.exitPoint.position;
            player.transform.rotation = currentShip.exitPoint.rotation;
            player.transform.Translate(currentShip.transform.forward * -1.5f, Space.World);
            player.SetActive(true);
        }

        currentShip.ReleaseControl();
        currentShip = null;
        isInShip = false;
    }

    // --- ОСТАНОВКА ВСЕХ ЗОНАЛЬНЫХ МУЗЫК ---
    private void StopAllZoneMusic()
    {
        MusicZone[] zones = FindObjectsOfType<MusicZone>();
        foreach (var zone in zones)
        {
            zone.StopZoneSounds();
            Debug.Log($"[ShipInputManager] Stopped music zone: {zone.name}");
        }
    }

    // --- ОСТАНОВКА ВСЕХ ЗОНАЛЬНЫХ ТУМАНОВ ---
    private void StopAllZoneFog()
    {
        FogZone[] zones = FindObjectsOfType<FogZone>();
        foreach (var zone in zones)
        {
            Debug.Log($"[ShipInputManager] Disabled fog zone: {zone.name}");
        }
    }

    // --- УПРАВЛЕНИЕ ГЛОБАЛЬНОЙ МУЗЫКОЙ ---
    private IEnumerator FadeOutGlobalMusic(float duration)
    {
        if (MusicManager.Instance == null) yield break;
        AudioSource globalSource = MusicManager.Instance.GetComponent<AudioSource>();
        if (globalSource == null) yield break;
        
        float startVol = globalSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            globalSource.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }
        globalSource.Stop();
        globalSource.volume = startVol;
        Debug.Log("[ShipInputManager] Global music faded out");
    }

    private IEnumerator FadeInGlobalMusic(float duration)
    {
        if (MusicManager.Instance == null) yield break;
        AudioSource globalSource = MusicManager.Instance.GetComponent<AudioSource>();
        if (globalSource == null) yield break;
        
        AudioClip globalClip = MusicManager.Instance.globalMusic;
        if (globalClip != null)
            globalSource.clip = globalClip;
        
        globalSource.volume = 0f;
        globalSource.Play();
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            globalSource.volume = Mathf.Lerp(0f, 0.5f, t);
            yield return null;
        }
        globalSource.volume = 0.5f;
        Debug.Log("[ShipInputManager] Global music faded in");
    }

    // --- УПРАВЛЕНИЕ КОРАБЕЛЬНОЙ МУЗЫКОЙ ---
    private IEnumerator FadeInShipMusic(float duration)
    {
        if (shipMusic == null) yield break;
        shipAudioSource.clip = shipMusic;
        shipAudioSource.volume = 0f;
        shipAudioSource.Play();
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            shipAudioSource.volume = Mathf.Lerp(0f, shipMusicVolume, t);
            yield return null;
        }
        shipAudioSource.volume = shipMusicVolume;
        Debug.Log("[ShipInputManager] Ship music faded in");
    }

    private IEnumerator FadeOutShipMusic(float duration)
    {
        float startVol = shipAudioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            shipAudioSource.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }
        shipAudioSource.Stop();
        Debug.Log("[ShipInputManager] Ship music faded out");
    }

    // --- УПРАВЛЕНИЕ ТУМАНОМ ---
    private IEnumerator FadeOutFog(float duration)
    {
        float timer = 0f;
        Color startColor = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        float startStart = RenderSettings.fogStartDistance;
        float startEnd = RenderSettings.fogEndDistance;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            RenderSettings.fogColor = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);
            
            if (RenderSettings.fogMode == FogMode.Exponential || RenderSettings.fogMode == FogMode.ExponentialSquared)
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, 0f, t);
            else if (RenderSettings.fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(startStart, 1000f, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(startEnd, 1000f, t);
            }
            
            yield return null;
        }
        
        RenderSettings.fog = false;
        Debug.Log("[ShipInputManager] Fog faded out");
    }

    private IEnumerator FadeInFog(float duration)
    {
        RenderSettings.fog = true;
        
        float timer = 0f;
        Color targetColor = originalFogColor;
        float targetDensity = originalFogDensity;
        float targetStart = originalFogStart;
        float targetEnd = originalFogEnd;
        
        Color startColor = RenderSettings.fogColor;
        float startDensity = 0f;
        float startStart = 1000f;
        float startEnd = 1000f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            
            RenderSettings.fogColor = Color.Lerp(startColor, targetColor, t);
            
            if (originalFogMode == FogMode.Exponential || originalFogMode == FogMode.ExponentialSquared)
                RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t);
            else if (originalFogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = Mathf.Lerp(startStart, targetStart, t);
                RenderSettings.fogEndDistance = Mathf.Lerp(startEnd, targetEnd, t);
            }
            
            yield return null;
        }
        
        RenderSettings.fogColor = targetColor;
        RenderSettings.fogDensity = targetDensity;
        RenderSettings.fogStartDistance = targetStart;
        RenderSettings.fogEndDistance = targetEnd;
        RenderSettings.fogMode = originalFogMode;
        Debug.Log("[ShipInputManager] Fog faded in");
    }

    // --- ПУБЛИЧНЫЕ МЕТОДЫ ---
    public bool IsInShip()
    {
        return isInShip;
    }
}