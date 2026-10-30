using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public Item item;
    public int amount;

    public InventorySlot(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }

    public void Clear()
    {
        item = null;
        amount = 0;
    }

    public bool CanAdd(Item itemToAdd, int amountToAdd)
    {
        if (item == null) return true;
        if (item != itemToAdd) return false;
        return amount + amountToAdd <= item.maxStackSize;
    }

    public void AddItem(Item itemToAdd, int amountToAdd)
    {
        if (item == null)
        {
            item = itemToAdd;
            amount = amountToAdd;
        }
        else if (item == itemToAdd)
        {
            amount += amountToAdd;
        }
    }

    public void RemoveItem(int amountToRemove)
    {
        amount -= amountToRemove;
        if (amount <= 0)
        {
            Clear();
        }
    }
}