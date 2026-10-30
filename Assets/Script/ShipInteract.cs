using UnityEngine;
using TMPro;

public class ShipInteract : MonoBehaviour
{
    [Header("UI Settings")]
    public string shipName = "Spaceship";
    
    [Header("UI")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    private bool isLookingAtShip = false;
    private Camera playerCamera;
    private ShipInputManager shipInput;
    
    void Start()
    {
        playerCamera = Camera.main;
        shipInput = FindObjectOfType<ShipInputManager>();
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    void Update()
    {
        // Проверяем, смотрит ли игрок на корабль
        CheckIfLookingAtShip();
        
        // Показываем или скрываем подсказку в зависимости от того, смотрит ли игрок
        // и находится ли он вне корабля
        if (shipInput != null && !shipInput.IsInShip())
        {
            ShowPrompt(isLookingAtShip);
        }
        else if (shipInput != null && shipInput.IsInShip())
        {
            // Если внутри корабля - скрываем подсказку
            if (interactionPrompt != null && interactionPrompt.activeSelf)
                interactionPrompt.SetActive(false);
        }
    }
    
    void CheckIfLookingAtShip()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            return;
        }
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (!isLookingAtShip)
                {
                    isLookingAtShip = true;
                }
                return;
            }
        }
        
        if (isLookingAtShip)
        {
            isLookingAtShip = false;
        }
    }
    
    void ShowPrompt(bool show)
    {
        if (interactionPrompt != null && interactionPrompt.activeSelf != show)
        {
            interactionPrompt.SetActive(show);
            if (show && promptText != null)
            {
                promptText.text = $"Press F to enter {shipName}";
            }
        }
    }
}