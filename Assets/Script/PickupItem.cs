using UnityEngine;
using TMPro;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    public Item item;
    public int amount = 1;

    [Header("Pickup Settings")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public GameObject pickupEffect;

    [Header("UI")]
    public GameObject pickupPrompt;
    public TextMeshProUGUI promptText;

    private Transform player;
    private bool isInRange = false;

    private void Start()
    {
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    private void Update()
    {
        if (isInRange && Input.GetKeyDown(pickupKey))
        {
            Pickup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isInRange = true;
            
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(true);
                if (promptText != null)
                    promptText.text = $"Press {pickupKey} to pick up {item.itemName} x{amount}";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            
            // Скрываем подсказку при выходе из зоны
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
        }
    }

    private void Pickup()
    {
        // Пытаемся добавить предмет в инвентарь
        if (Inventory.Instance.AddItem(item, amount))
        {
            Debug.Log($"Picked up {amount}x {item.itemName}");
            
            // Скрываем подсказку перед уничтожением
            if (pickupPrompt != null)
                pickupPrompt.SetActive(false);
            
            // Эффект подбора
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            
            // Уничтожаем предмет
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory is full!");
            // Можно показать временное сообщение "Inventory full"
            StartCoroutine(ShowFullInventoryMessage());
        }
    }
    
    private System.Collections.IEnumerator ShowFullInventoryMessage()
    {
        if (promptText != null)
        {
            string originalText = promptText.text;
            promptText.text = "Inventory is full!";
            yield return new WaitForSeconds(1f);
            promptText.text = originalText;
        }
    }
}