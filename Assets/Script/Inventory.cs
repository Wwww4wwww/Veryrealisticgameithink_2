using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Settings")]
    public int inventorySize = 20;
    public List<InventorySlot> slots;

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        slots = new List<InventorySlot>();
        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        // Сначала пытаемся добавить в существующие стеки
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].CanAdd(item, amount))
            {
                slots[i].AddItem(item, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        // Затем ищем пустые слоты
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].AddItem(item, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Inventory full! Cannot add {item.itemName}");
        return false;
    }

    public bool RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        int remainingToRemove = amount;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].item == item)
            {
                int removeAmount = Mathf.Min(remainingToRemove, slots[i].amount);
                slots[i].RemoveItem(removeAmount);
                remainingToRemove -= removeAmount;

                if (remainingToRemove <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasItem(Item item, int amount = 1)
    {
        int totalAmount = GetItemAmount(item);
        return totalAmount >= amount;
    }

    public int GetItemAmount(Item item)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.item == item)
                total += slot.amount;
        }
        return total;
    }

    public List<InventorySlot> GetNonEmptySlots()
    {
        List<InventorySlot> nonEmpty = new List<InventorySlot>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty())
                nonEmpty.Add(slot);
        }
        return nonEmpty;
    }

    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        OnInventoryChanged?.Invoke();
    }
}