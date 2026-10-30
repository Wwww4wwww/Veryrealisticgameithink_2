using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerControllerOldInput : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    // Компоненты
    private CharacterController controller;
    private Animator animator;

    // Ввод
    private Vector2 moveInput;
    private bool isRunning;
    private bool jumpPressed;

    // Физика
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    // Поворот
    private float rotationVelocity;
    private float targetRotation;

    // Хеши аниматора
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Блокируем курсор (опционально)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleInput(); // Считываем клавиши
        GroundCheck();
        ApplyGravity();
        Move();
        Jump();
        UpdateAnimator();
    }

    private void HandleInput()
    {
        // Получаем оси горизонтали и вертикали (A/D, стрелки ←→ и W/S, стрелки ↑↓)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized; // Нормализуем, чтобы по диагонали скорость не была выше

        // Бег — зажат Left Shift
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Прыжок — нажата клавиша Space (сработает один раз)
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // небольшое отрицательное значение для прилипания к земле
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Move()
    {
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (moveInput != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(JumpHash);
        }
        jumpPressed = false;
    }

    private void UpdateAnimator()
    {
        float animationSpeed = moveInput.magnitude * (isRunning ? 1f : 0.5f);
        animator.SetFloat(SpeedHash, animationSpeed, 0.1f, Time.deltaTime);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsRunningHash, isRunning);
    }
}