using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public string description = "Item description";
    public Sprite icon = null;
    public int maxStackSize = 1;
    public bool isConsumable = false;
    public float weight = 0f;
    
    public int healthRestore = 0;
    public int staminaRestore = 0;
}