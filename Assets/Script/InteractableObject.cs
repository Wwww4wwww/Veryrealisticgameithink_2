using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Interaction")]
    public string objectName = "Object";
    public KeyCode interactKey = KeyCode.E;
    
    [Header("UI")]
    public GameObject interactionPrompt;
    public TMPro.TextMeshProUGUI promptText;
    
    private bool isInRange = false;
    private Crosshair crosshair;
    
    private void Start()
    {
        crosshair = FindObjectOfType<Crosshair>();
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    private void Update()
    {
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                if (promptText != null)
                    promptText.text = $"Press {interactKey} to {objectName}";
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
    
    protected virtual void Interact()
    {
        Debug.Log($"Interacted with {objectName}");
        // Переопределите в дочерних классах
    }
}