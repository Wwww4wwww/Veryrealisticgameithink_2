using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;

    [Header("Crosshair Settings")]
    public Image crosshairImage;
    public float defaultSize = 30f;
    public float enlargedSize = 50f;
    public float smoothSpeed = 8f;

    [Header("Colors")]
    public Color defaultColor = Color.white;
    public Color interactColor = Color.yellow;

    [Header("Layers")]
    public LayerMask interactableLayers; // Слои, на которые реагирует прицел

    [Header("Raycast Settings")]
    public float rayDistance = 5f;
    public Camera playerCamera;

    private float currentSize;
    private Color currentColor;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (crosshairImage == null)
            crosshairImage = GetComponent<Image>();
        
        rectTransform = crosshairImage.GetComponent<RectTransform>();
        currentSize = defaultSize;
        currentColor = defaultColor;
        
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Изначально скрываем курсор
        Cursor.visible = false;
    }

    private void Update()
    {
        CheckInteractable();
        UpdateCrosshairVisual();
    }

    private void CheckInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayers))
        {
            // Проверяем, есть ли на объекте интерактивный компонент
            bool isInteractable = false;
            
            if (hit.collider.GetComponent<PickupItem>() != null)
                isInteractable = true;
            
            if (hit.collider.GetComponent<SpaceShipController>() != null)
                isInteractable = true;
            
            // Можно добавить другие интерактивные объекты
            if (hit.collider.GetComponent<InteractableObject>() != null)
                isInteractable = true;

            if (isInteractable)
            {
                currentSize = Mathf.Lerp(currentSize, enlargedSize, smoothSpeed * Time.deltaTime);
                currentColor = Color.Lerp(currentColor, interactColor, smoothSpeed * Time.deltaTime);
                return;
            }
        }
        
        // Нет интерактивного объекта
        currentSize = Mathf.Lerp(currentSize, defaultSize, smoothSpeed * Time.deltaTime);
        currentColor = Color.Lerp(currentColor, defaultColor, smoothSpeed * Time.deltaTime);
    }

    private void UpdateCrosshairVisual()
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(currentSize, currentSize);
        }
        
        if (crosshairImage != null)
        {
            crosshairImage.color = currentColor;
        }
    }

    // Показать/скрыть прицел (например, при открытии инвентаря)
    public void SetVisible(bool visible)
    {
        crosshairImage.enabled = visible;
        Cursor.visible = !visible;
    }
}