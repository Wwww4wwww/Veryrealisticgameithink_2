using UnityEngine;

public class SimpleCutsceneCleanup : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;              // Аниматор персонажа
    [SerializeField] private Camera playerCamera;            // Основная камера игрока
    [SerializeField] private Camera cutsceneCamera;          // Камера катсцены (будет удалена)
    [SerializeField] private GameObject cameraParent;        // Родитель камеры (будет удалён)

    [Header("Settings")]
    [SerializeField] private string animationStateName = "StartCutscene";
    [SerializeField] private bool playOnlyOnce = true;

    private bool cutscenePlayed = false;
    private bool cutsceneActive = false;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    private void Awake()
    {
        playerStartPosition = transform.position;
        playerStartRotation = transform.rotation;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        // Выключаем камеру катсцены по умолчанию
        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(false);
    }

    private void Start()
    {
        PlayCutscene();
    }

    private void Update()
    {
        if (cutsceneActive)
        {
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

        // Фиксируем персонажа
        transform.position = playerStartPosition;
        transform.rotation = playerStartRotation;

        // Переключаем камеры
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);

        if (cutsceneCamera != null)
            cutsceneCamera.gameObject.SetActive(true);

        // Запускаем анимацию
        if (animator != null)
            animator.Play(animationStateName, 0, 0f);
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

        // Включаем камеру игрока
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);

        // УДАЛЯЕМ камеру катсцены
        if (cutsceneCamera != null)
            Destroy(cutsceneCamera.gameObject);

        // УДАЛЯЕМ родительский объект
        if (cameraParent != null)
            Destroy(cameraParent);

        Debug.Log("Катсцена завершена. Камера катсцены и родитель удалены.");
    }

    public void ForceEndCutscene()
    {
        EndCutscene();
    }
}