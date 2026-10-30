using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUISlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public Button slotButton;

    private InventorySlot slot;
    private int slotIndex;

    public void Initialize(InventorySlot slot, int index)
    {
        this.slot = slot;
        this.slotIndex = index;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (slot != null && !slot.IsEmpty())
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = true;
            amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";
            amountText.enabled = slot.amount > 1;
        }
        else
        {
            iconImage.enabled = false;
            amountText.enabled = false;
        }
    }

    public void OnSlotClick()
    {
        if (slot != null && !slot.IsEmpty())
        {
            // Использование предмета
            UseItem(slot.item);
        }
    }

    private void UseItem(Item item)
    {
        if (item.isConsumable)
        {
            // Применяем эффекты
            if (item.healthRestore > 0)
            {
                // Добавьте логику восстановления здоровья
                Debug.Log($"Restored {item.healthRestore} health");
            }
            
            if (item.staminaRestore > 0)
            {
                // Добавьте логику восстановления стамины
                Debug.Log($"Restored {item.staminaRestore} stamina");
            }
            
            // Удаляем один предмет
            Inventory.Instance.RemoveItem(item, 1);
        }
    }
}