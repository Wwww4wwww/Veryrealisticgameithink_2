using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public float stepInterval = 0.5f;      // Интервал между шагами (сек)
    public float walkStepInterval = 0.5f;  // Интервал при ходьбе
    public float runStepInterval = 0.3f;   // Интервал при беге
    public float volumeMultiplier = 1f;

    [Header("Layer Sounds")]
    public TerrainLayerSound[] terrainLayers; // Звуки для слоёв террейна
    public LayerSound[] physicsMaterials;     // Звуки для PhysicsMaterial (опционально)

    [Header("References")]
    public CharacterController controller;
    public Animator animator;
    public PlayerMovement playerMovement; // Ссылка на ваш скрипт движения

    private float stepTimer = 0f;
    private bool isGrounded = true;
    private float currentStepInterval;
    private string currentLayerName = "";

    [System.Serializable]
    public class TerrainLayerSound
    {
        public TerrainLayer terrainLayer;
        public AudioClip[] footstepSounds;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [System.Serializable]
    public class LayerSound
    {
        public LayerMask layer;
        public AudioClip[] footstepSounds;
        [Range(0f, 1f)] public float volume = 1f;
    }

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        currentStepInterval = stepInterval;
    }

    private void Update()
    {
        // Проверяем, стоит ли персонаж на земле и двигается ли
        bool isMoving = playerMovement != null && 
                        (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0);
        
        isGrounded = controller.isGrounded;

        // Определяем интервал шагов (ходьба или бег)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        currentStepInterval = isRunning ? runStepInterval : walkStepInterval;

        if (isGrounded && isMoving)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= currentStepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = GetFootstepClip();
        if (clip != null)
        {
            float volume = GetFootstepVolume();
            audioSource.pitch = Random.Range(0.9f, 1.1f); // Лёгкая вариация
            audioSource.PlayOneShot(clip, volume * volumeMultiplier);
        }
    }

    private AudioClip GetFootstepClip()
    {
        // 1. Сначала проверяем террейн
        if (Terrain.activeTerrain != null)
        {
            Vector3 playerPos = transform.position;
            TerrainLayer layer = GetTerrainLayerAtPosition(playerPos);
            foreach (var tls in terrainLayers)
            {
                if (tls.terrainLayer == layer && tls.footstepSounds.Length > 0)
                {
                    return tls.footstepSounds[Random.Range(0, tls.footstepSounds.Length)];
                }
            }
        }

        // 2. Проверяем PhysicsMaterial через Raycast
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            // Проверяем слои
            foreach (var ls in physicsMaterials)
            {
                if (ls.layer == (ls.layer.value & (1 << hit.collider.gameObject.layer)))
                {
                    if (ls.footstepSounds.Length > 0)
                        return ls.footstepSounds[Random.Range(0, ls.footstepSounds.Length)];
                }
            }

            // Проверяем тег
            if (hit.collider.CompareTag("Wood"))
            {
                // Можно добавить отдельную обработку по тегу
            }
        }

        // 3. Звук по умолчанию (если ничего не найдено)
        if (terrainLayers.Length > 0 && terrainLayers[0].footstepSounds.Length > 0)
            return terrainLayers[0].footstepSounds[Random.Range(0, terrainLayers[0].footstepSounds.Length)];

        return null;
    }

    private float GetFootstepVolume()
    {
        // Проверяем террейн
        if (Terrain.activeTerrain != null)
        {
            Vector3 playerPos = transform.position;
            TerrainLayer layer = GetTerrainLayerAtPosition(playerPos);
            foreach (var tls in terrainLayers)
            {
                if (tls.terrainLayer == layer)
                    return tls.volume;
            }
        }

        // Проверяем слои
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
        {
            foreach (var ls in physicsMaterials)
            {
                if (ls.layer == (ls.layer.value & (1 << hit.collider.gameObject.layer)))
                    return ls.volume;
            }
        }

        return 1f;
    }

    private TerrainLayer GetTerrainLayerAtPosition(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return null;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 normalizedPos = new Vector3(
            (position.x - terrainPos.x) / terrain.terrainData.size.x,
            (position.z - terrainPos.z) / terrain.terrainData.size.z,
            0
        );

        float[,,] map = terrain.terrainData.GetAlphamaps(
            (int)(normalizedPos.x * terrain.terrainData.alphamapWidth),
            (int)(normalizedPos.z * terrain.terrainData.alphamapHeight),
            1, 1
        );

        int maxIndex = 0;
        float maxValue = 0;
        for (int i = 0; i < terrain.terrainData.terrainLayers.Length; i++)
        {
            if (map[0, 0, i] > maxValue)
            {
                maxValue = map[0, 0, i];
                maxIndex = i;
            }
        }
        return terrain.terrainData.terrainLayers[maxIndex];
    }

    // Опционально: добавить визуальный дебаг
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.5f);
    }
}