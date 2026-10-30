using UnityEngine;

public class StartCutscene : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private MonoBehaviour cameraLook;
    [SerializeField] private Camera playerCamera;

    [Header("Cutscene Camera Positions")]
    [SerializeField] private Transform cutsceneCameraStart;
    [SerializeField] private Transform cutsceneCameraEnd;

    [Header("Field of View Settings")]
    [SerializeField] private float cutsceneFOV = 40f;
    [SerializeField] private float normalFOV = 60f;

    [Header("Cutscene Settings")]
    [SerializeField] private string animationStateName = "StartCutscene";
    [SerializeField] private bool disableCursor = true;
    [SerializeField] private bool playOnlyOnce = true;

    private bool cutscenePlayed = false;
    private bool cutsceneActive = false;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;
    
    // Кешируем ссылки для быстрого доступа
    private MonoBehaviour[] playerScripts;
    private MonoBehaviour[] cameraScripts;

    // Публичное свойство для проверки из других скриптов
    public bool IsCutsceneActive => cutsceneActive;

    private void Awake()
    {
        playerStartPosition = transform.position;
        playerStartRotation = transform.rotation;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        if (cameraLook == null && playerCamera != null)
            cameraLook = playerCamera.GetComponent<MonoBehaviour>();

        // Кешируем все скрипты для отключения
        playerScripts = GetComponents<MonoBehaviour>();
        if (playerCamera != null)
            cameraScripts = playerCamera.GetComponents<MonoBehaviour>();

        // Создаём точки, если не назначены
        if (cutsceneCameraStart == null)
        {
            GameObject go = new GameObject("CutsceneCameraStart");
            go.transform.position = playerCamera != null ? playerCamera.transform.position : transform.position;
            go.transform.rotation = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;
            cutsceneCameraStart = go.transform;
            Debug.LogWarning("[StartCutscene] CutsceneCameraStart создан автоматически.");
        }

        if (cutsceneCameraEnd == null)
        {
            GameObject go = new GameObject("CutsceneCameraEnd");
            go.transform.position = playerCamera != null ? playerCamera.transform.position : transform.position + Vector3.back * 3f;
            go.transform.rotation = playerCamera != null ? playerCamera.transform.rotation : Quaternion.identity;
            cutsceneCameraEnd = go.transform;
            Debug.LogWarning("[StartCutscene] CutsceneCameraEnd создан автоматически.");
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayCutscene();
    }

    private void Update()
    {
        if (cutsceneActive)
        {
            // Фиксируем камеру в начальной позиции КАЖДЫЙ КАДР
            if (playerCamera != null && cutsceneCameraStart != null)
            {
                playerCamera.transform.position = cutsceneCameraStart.position;
                playerCamera.transform.rotation = cutsceneCameraStart.rotation;
                playerCamera.fieldOfView = cutsceneFOV;
            }

            // Фиксируем персонажа
            transform.position = playerStartPosition;
            transform.rotation = playerStartRotation;

            CheckAnimationEnd();
        }
    }

    public void PlayCutscene()
    {
        if (playOnlyOnce && cutscenePlayed) return;

        cutsceneActive = true;
        cutscenePlayed = true;

        // Полностью отключаем управление
        SetScriptsEnabled(false);

        // Фиксируем персонажа
        transform.position = playerStartPosition;
        transform.rotation = playerStartRotation;

        // Перемещаем камеру в начальную точку
        if (playerCamera != null && cutsceneCameraStart != null)
        {
            playerCamera.transform.position = cutsceneCameraStart.position;
            playerCamera.transform.rotation = cutsceneCameraStart.rotation;
            playerCamera.fieldOfView = cutsceneFOV;
        }

        // Запускаем анимацию
        if (animator != null)
        {
            animator.Play(animationStateName, 0, 0f);
        }

        Debug.Log($"[StartCutscene] Катсцена началась. Камера: {playerCamera?.transform.position}, FOV: {playerCamera?.fieldOfView}");
    }

    private void CheckAnimationEnd()
    {
        if (animator == null)
        {
            EndCutscene();
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isCutsceneAnimation = stateInfo.IsName(animationStateName);

        if (!isCutsceneAnimation || (stateInfo.normalizedTime >= 0.99f && !animator.IsInTransition(0)))
        {
            EndCutscene();
        }
    }

    private void EndCutscene()
    {
        if (!cutsceneActive) return;

        cutsceneActive = false;

        // ПЕРЕМЕЩАЕМ КАМЕРУ В КОНЕЧНУЮ ТОЧКУ (пока скрипты ещё отключены)
        if (playerCamera != null && cutsceneCameraEnd != null)
        {
            playerCamera.transform.position = cutsceneCameraEnd.position;
            playerCamera.transform.rotation = cutsceneCameraEnd.rotation;
            playerCamera.fieldOfView = normalFOV;
        }

        // ВКЛЮЧАЕМ СКРИПТЫ ОБРАТНО
        SetScriptsEnabled(true);

        if (disableCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log($"[StartCutscene] Катсцена завершена. Камера: {playerCamera?.transform.position}, FOV: {playerCamera?.fieldOfView}");
    }

    private void SetScriptsEnabled(bool enabled)
    {
        // Отключаем скрипты на Player
        foreach (MonoBehaviour script in playerScripts)
        {
            if (script != null && script != this && script != animator)
            {
                script.enabled = enabled;
            }
        }

        // Отключаем скрипты на Камере
        if (cameraScripts != null)
        {
            foreach (MonoBehaviour script in cameraScripts)
            {
                if (script != null)
                {
                    script.enabled = enabled;
                }
            }
        }
    }

    public void ForceEndCutscene()
    {
        EndCutscene();
    }
}