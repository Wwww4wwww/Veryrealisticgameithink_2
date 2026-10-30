using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerControllerInputSystem : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraTransform;      // Ссылка на камеру (дочерний объект)
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float verticalLookLimit = 80f;  // Ограничение взгляда вверх/вниз

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    // Компоненты
    private CharacterController controller;
    private Animator animator;

    // Ввод (значения будут обновляться через события)
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool jumpPressed;

    // Физика
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    // Поворот камеры
    private float xRotation = 0f;

    // Хеши аниматора
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        GroundCheck();
        ApplyGravity();
        Move();
        Jump();
        UpdateAnimator();
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
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

        // Направление движения относительно камеры
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * moveInput.x + forward * moveInput.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            // Плавный поворот персонажа в направлении движения
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
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

    // ========== Обработчики Input System ==========

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>() * mouseSensitivity * 0.01f; // Приводим к разумной чувствительности

        // Горизонтальный поворот персонажа (влево-вправо)
        transform.Rotate(Vector3.up, lookInput.x);

        // Вертикальный поворот камеры (вверх-вниз) с ограничением
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    public void OnRun(InputValue value)
    {
        isRunning = value.isPressed;
    }
}