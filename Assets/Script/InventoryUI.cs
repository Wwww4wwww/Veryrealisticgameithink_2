using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("References")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;

    private InventoryUISlot[] uiSlots;
    private bool isInventoryOpen = false;

    // Статическое событие для оповещения других скриптов
    public static event System.Action<bool> OnInventoryToggled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[InventoryUI] Inventory.Instance is null!");
            return;
        }
        
        if (slotPrefab == null)
        {
            Debug.LogError("[InventoryUI] slotPrefab is not assigned!");
            return;
        }
        
        if (slotsParent == null)
        {
            Debug.LogError("[InventoryUI] slotsParent is not assigned!");
            return;
        }
        
        CreateSlots();
        
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += UpdateUI;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void CreateSlots()
    {
        uiSlots = new InventoryUISlot[Inventory.Instance.inventorySize];
        
        for (int i = 0; i < Inventory.Instance.inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventoryUISlot uiSlot = slotObj.GetComponent<InventoryUISlot>();
            
            if (uiSlot == null)
            {
                Debug.LogError($"[InventoryUI] Slot prefab missing InventoryUISlot component on slot {i}");
                continue;
            }
            
            uiSlot.Initialize(Inventory.Instance.slots[i], i);
            uiSlots[i] = uiSlot;
        }
    }

    private void UpdateUI()
    {
        if (uiSlots == null) return;
        
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null)
                uiSlots[i].UpdateUI();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);
        
        // Вызываем событие для других скриптов
        OnInventoryToggled?.Invoke(isInventoryOpen);
        
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }
}