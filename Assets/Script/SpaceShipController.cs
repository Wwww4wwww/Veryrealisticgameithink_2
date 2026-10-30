using UnityEngine;

public class SpaceShipController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 30f;
    public float strafeSpeed = 25f;
    public float verticalSpeed = 20f;

    [Header("Auto Rotation")]
    public float rotationSmoothing = 5f;
    public float minSpeedForRotation = 0.5f;

    [Header("Free Camera")]
    public Camera shipCamera;
    public float cameraOrbitSpeed = 2f;
    public float cameraDistance = 8f;
    public Vector2 cameraVerticalLimit = new Vector2(10f, 80f);

    [Header("Camera Zoom")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 3f;
    public float maxZoomDistance = 20f;
    public float zoomSmoothness = 5f;   // Плавность изменения дистанции (чем больше, тем быстрее)

    [Header("Entry/Exit")]
    public Transform entryPoint;
    public Transform exitPoint;

    private SpaceShipEntry playerEntry;
    private bool isControlled = false;

    private Vector3 currentVelocity = Vector3.zero;
    private float camYaw = 0f;
    private float camPitch = 20f;

    private float targetCameraDistance;   // Целевая дистанция (изменяется колёсиком)
    private float currentCameraDistance;  // Текущая дистанция (плавно следует за target)

    private void Start()
    {
        if (shipCamera != null)
        {
            camYaw = shipCamera.transform.eulerAngles.y;
            camPitch = shipCamera.transform.eulerAngles.x;
        }
        targetCameraDistance = cameraDistance;
        currentCameraDistance = cameraDistance;
    }

    private void Update()
    {
        if (!isControlled) return;

        // --- Движение ---
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        if (Input.GetKey(KeyCode.E)) move += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) move += Vector3.down;
        if (move.magnitude > 1f) move.Normalize();

        Vector3 desiredDirection = transform.TransformDirection(move);
        Vector3 velocity = desiredDirection;
        velocity.x *= strafeSpeed;
        velocity.z *= forwardSpeed;
        velocity.y *= verticalSpeed;

        transform.position += velocity * Time.deltaTime;
        currentVelocity = velocity;

        // --- Автоповорот ---
        if (currentVelocity.magnitude > minSpeedForRotation)
        {
            Vector3 direction = currentVelocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        }

        // --- Камера (орбита) ---
        if (Input.GetMouseButton(1))
        {
            camYaw += Input.GetAxis("Mouse X") * cameraOrbitSpeed;
            camPitch -= Input.GetAxis("Mouse Y") * cameraOrbitSpeed;
            camPitch = Mathf.Clamp(camPitch, cameraVerticalLimit.x, cameraVerticalLimit.y);
        }

        // --- Zoom колёсиком (изменяем целевую дистанцию) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetCameraDistance -= scroll * zoomSpeed;
            targetCameraDistance = Mathf.Clamp(targetCameraDistance, minZoomDistance, maxZoomDistance);
        }

        // --- Плавное приближение текущей дистанции к целевой ---
        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetCameraDistance, zoomSmoothness * Time.deltaTime);

        // --- Применяем позицию камеры с плавной дистанцией ---
        if (shipCamera != null)
        {
            Quaternion camRotation = Quaternion.Euler(camPitch, camYaw, 0);
            Vector3 desiredPosition = transform.position + camRotation * new Vector3(0, 0, -currentCameraDistance);
            shipCamera.transform.position = desiredPosition;
            shipCamera.transform.LookAt(transform);
        }
    }

    public void TakeControl(SpaceShipEntry entry)
    {
        playerEntry = entry;
        isControlled = true;

        if (shipCamera != null)
        {
            shipCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void ReleaseControl()
    {
    Debug.Log("[SpaceShipController] ReleaseControl called");
    isControlled = false;
    playerEntry = null;

    if (shipCamera != null)
    {
        shipCamera.gameObject.SetActive(false);
        // НЕ разблокируем курсор здесь
        // Cursor.lockState = CursorLockMode.None;
        // Cursor.visible = true;
    }
        }
            }
    