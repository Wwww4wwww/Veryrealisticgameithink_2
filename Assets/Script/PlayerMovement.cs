using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private StartCutscene cutsceneScript;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaRegenDelay = 1f;
    private bool canRun = true;

    [Header("Head Bobbing")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float runBobSpeed = 14f;
    [SerializeField] private float runBobAmount = 0.08f;
    [SerializeField] private float sideBobMultiplier = 0.5f;
    [SerializeField] private float bobSmoothing = 8f;

    [Header("Movement Vignette")]
    [SerializeField] private bool enableVignette = true;
    [SerializeField] private float walkVignetteIntensity = 0.25f;
    [SerializeField] private float runVignetteIntensity = 0.7f;
    [SerializeField] private float vignetteSmoothness = 5f;
    [SerializeField] private Color vignetteColor = Color.black;

    [Header("Stamina UI")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFillImage;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 80f;

    [Header("FOV Zoom")]
    [SerializeField] private bool enableFOVZoom = true;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomedFOV = 40f;
    [SerializeField] private float zoomFOVSpeed = 5f;
    [SerializeField] private KeyCode zoomKey = KeyCode.Q;

    // Параметры анимации
    private readonly string ANIM_SPEED = "Speed";
    private readonly string ANIM_GROUNDED = "Grounded";
    private readonly string ANIM_JUMP = "Jump";
    private readonly string ANIM_MOVE_X = "MoveX";
    private readonly string ANIM_MOVE_Z = "MoveZ";

    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 moveDirection;
    private Vector2 moveInput;

    // Система стамины
    private float currentStamina;
    private bool isRunning = false;
    private float staminaRegenTimer = 0f;

    // Система боббинга
    private float bobTimer = 0f;
    private Vector3 originalCameraPosition;
    private float currentBobSpeed;
    private float currentBobAmount;

    // Система виньетки
    private Image vignetteImage;
    private float targetVignetteIntensity = 0f;
    private float currentVignetteIntensity = 0f;

    // FOV Zoom
    private bool isZooming = false;
    private float targetFOV;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
        if (animator == null)
            animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        
        currentStamina = maxStamina;
        canRun = true;
        
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.localPosition;
            targetFOV = normalFOV;
        }
        
        if (enableVignette)
            CreateVignette();
        
        UpdateStaminaUI();
        
        if (cutsceneScript == null)
            cutsceneScript = FindObjectOfType<StartCutscene>();
    }

    private void CreateVignette()
    {
        GameObject canvasObj = new GameObject("VignetteCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        GameObject vignetteObj = new GameObject("VignetteImage");
        vignetteObj.transform.SetParent(canvas.transform, false);
        vignetteImage = vignetteObj.AddComponent<Image>();
        
        RectTransform rect = vignetteImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Texture2D vignetteTexture = CreateVignetteTexture();
        vignetteImage.sprite = Sprite.Create(vignetteTexture, new Rect(0, 0, vignetteTexture.width, vignetteTexture.height), new Vector2(0.5f, 0.5f));
        vignetteImage.type = Image.Type.Simple;
        vignetteImage.color = new Color(vignetteColor.r, vignetteColor.g, vignetteColor.b, 0f);
        
        DontDestroyOnLoad(canvasObj);
    }

    private Texture2D CreateVignetteTexture()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01((distance / radius) * 1.5f - 0.5f);
                alpha = Mathf.Pow(alpha, 1.5f);
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovementInput();
        HandleStamina();
        HandleJumpAndGravity();
        HandleHeadBob();
        HandleVignette();
        HandleFOVZoom();
        UpdateAnimations();
    }

    private void HandleMouseLook()
    {
        // Проверяем, открыт ли инвентарь
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsInventoryOpen())
            return;
        
        // Проверяем, активна ли катсцена
        if (cutsceneScript != null && cutsceneScript.IsCutsceneActive)
            return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveInput = new Vector2(horizontal, vertical);
        moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = moveDirection.magnitude > 0.1f;
        
        if (shiftPressed && canRun && currentStamina > 0 && isMoving && isGrounded)
        {
            currentSpeed = runSpeed;
            isRunning = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            isRunning = false;
        }
    }

    private void HandleStamina()
    {
        if (isRunning && isGrounded && moveDirection.magnitude > 0.1f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            staminaRegenTimer = staminaRegenDelay;
            
            if (currentStamina <= 0)
            {
                canRun = false;
                isRunning = false;
                currentSpeed = walkSpeed;
            }
            else
            {
                canRun = true;
            }
        }
        else
        {
            if (staminaRegenTimer > 0)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
                
                if (currentStamina > 0 && !canRun)
                {
                    canRun = true;
                }
            }
        }
        
        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
            
            if (currentStamina < maxStamina * 0.3f)
                staminaFillImage.color = Color.red;
            else if (currentStamina < maxStamina * 0.6f)
                staminaFillImage.color = Color.yellow;
            else
                staminaFillImage.color = Color.green;
        }
    }

    private void HandleJumpAndGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator != null)
                animator.SetTrigger(ANIM_JUMP);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;
        
        bool isMoving = moveDirection.magnitude > 0.1f && isGrounded;
        
        if (isMoving)
        {
            if (isRunning)
            {
                currentBobSpeed = runBobSpeed;
                currentBobAmount = runBobAmount;
            }
            else
            {
                currentBobSpeed = walkBobSpeed;
                currentBobAmount = walkBobAmount;
            }
            
            bobTimer += Time.deltaTime * currentBobSpeed;
            
            float yOffset = Mathf.Sin(bobTimer) * currentBobAmount;
            float xOffset = Mathf.Cos(bobTimer * 2f) * currentBobAmount * sideBobMultiplier;
            
            Vector3 newPosition = originalCameraPosition;
            newPosition.y += yOffset;
            newPosition.x += xOffset;
            
            playerCamera.transform.localPosition = newPosition;
        }
        else
        {
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition, 
                originalCameraPosition, 
                Time.deltaTime * bobSmoothing
            );
        }
    }

    private void HandleVignette()
    {
        if (!enableVignette || vignetteImage == null) return;
        
        bool isMoving = moveDirection.magnitude > 0.1f && isGrounded;
        
        if (isMoving)
        {
            if (isRunning)
            {
                targetVignetteIntensity = runVignetteIntensity;
            }
            else
            {
                targetVignetteIntensity = walkVignetteIntensity;
            }
        }
        else
        {
            targetVignetteIntensity = 0f;
        }
        
        currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, vignetteSmoothness * Time.deltaTime);
        
        Color c = vignetteImage.color;
        c.a = currentVignetteIntensity;
        vignetteImage.color = c;
    }

    private void HandleFOVZoom()
    {
        if (!enableFOVZoom || playerCamera == null) return;
        
        // Проверяем, активна ли катсцена
        if (cutsceneScript != null && cutsceneScript.IsCutsceneActive)
            return;
        
        // Удержание Q для зума
        if (Input.GetKey(zoomKey))
        {
            isZooming = true;
            targetFOV = zoomedFOV;
        }
        else
        {
            isZooming = false;
            targetFOV = normalFOV;
        }
        
        // Плавное изменение FOV
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, zoomFOVSpeed * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float targetSpeed = moveInput.magnitude * (currentSpeed / runSpeed);
        
        if (!isGrounded)
            targetSpeed = 0f;

        animator.SetFloat(ANIM_SPEED, targetSpeed, 0.1f, Time.deltaTime);
        
        if (moveInput.magnitude > 0.1f)
        {
            animator.SetFloat(ANIM_MOVE_X, moveInput.x, 0.1f, Time.deltaTime);
            animator.SetFloat(ANIM_MOVE_Z, moveInput.y, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat(ANIM_MOVE_X, 0f, 0.1f, Time.deltaTime);
            animator.SetFloat(ANIM_MOVE_Z, 0f, 0.1f, Time.deltaTime);
        }
        
        animator.SetBool(ANIM_GROUNDED, isGrounded);
    }

    private void FixedUpdate()
    {
        Vector3 movement = moveDirection * currentSpeed * Time.fixedDeltaTime;
        controller.Move(movement);
        controller.Move(velocity * Time.fixedDeltaTime);
    }
    
    public float GetCurrentStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
    public float GetStaminaPercent() => currentStamina / maxStamina;
    public bool CanRun() => canRun && currentStamina > 0;
}