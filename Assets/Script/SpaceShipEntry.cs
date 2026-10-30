using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpaceShipEntry : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Transform playerModel;
    public LayerMask shipLayer = 1;

    private SpaceShipController currentShip;
    private bool isInShip = false;
    private MonoBehaviour[] playerControls;

    // Для диагностики
    private float lastLogTime = 0f;

    private void Start()
    {
        playerControls = GetComponents<MonoBehaviour>();
        System.Collections.Generic.List<MonoBehaviour> list = new System.Collections.Generic.List<MonoBehaviour>(playerControls);
        list.Remove(this);
        playerControls = list.ToArray();
        
        Debug.Log($"[SpaceShipEntry] Start: найдено {playerControls.Length} скриптов управления");
    }

    private void Update()
    {
        // Диагностика каждые 5 секунд
        if (Time.time - lastLogTime > 5f)
        {
            lastLogTime = Time.time;
            Debug.Log($"[SpaceShipEntry] Status: isInShip={isInShip}, currentShip={(currentShip != null ? currentShip.name : "null")}, gameObject.active={gameObject.activeSelf}, enabled={enabled}");
        }

        // Проверяем клавишу F напрямую
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"[SpaceShipEntry] F PRESSED! isInShip={isInShip}");
            
            if (isInShip)
            {
                Debug.Log("[SpaceShipEntry] Вызов ExitShip()");
                ExitShip();
            }
            else
            {
                Debug.Log("[SpaceShipEntry] Вызов TryEnterShip()");
                TryEnterShip();
            
            }
        }
        
    }

    private void TryEnterShip()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 5f, shipLayer))
        {
            Debug.Log($"[TryEnterShip] Hit: {hit.collider.name}");
            SpaceShipController ship = hit.collider.GetComponent<SpaceShipController>();
            if (ship != null)
            {
                EnterShip(ship);
            }
        }
    }

    private void EnterShip(SpaceShipController ship)
    {
        Debug.Log("[EnterShip] START");
        
        if (ship.entryPoint == null)
        {
            Debug.LogError("[EnterShip] entryPoint = null!");
            return;
        }

        currentShip = ship;
        isInShip = true;

        // Отключаем управление игроком
        foreach (var c in playerControls) 
            c.enabled = false;
        GetComponent<CharacterController>().enabled = false;

        playerCamera.gameObject.SetActive(false);
        if (playerModel != null) 
            playerModel.gameObject.SetActive(false);

        transform.SetParent(currentShip.entryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        currentShip.TakeControl(this);
        
        Debug.Log("[EnterShip] COMPLETE");
    }

    public void ExitShip()
    {
        Debug.Log("[ExitShip] START");
        
        if (currentShip == null)
        {
            Debug.LogError("[ExitShip] currentShip = null!");
            return;
        }
        
        if (currentShip.exitPoint == null)
        {
            Debug.LogError("[ExitShip] exitPoint = null! Создайте exitPoint на корабле.");
            return;
        }

        // Открепляем от корабля
        transform.SetParent(null);
        
        // Перемещаем к точке выхода
        Vector3 exitPos = currentShip.exitPoint.position;
        transform.position = exitPos;
        transform.Translate(currentShip.transform.forward * -2f, Space.World);
        
        Debug.Log($"[ExitShip] Перемещён на {transform.position}");

        // Включаем управление игроком
        foreach (var c in playerControls) 
            c.enabled = true;
        GetComponent<CharacterController>().enabled = true;

        playerCamera.gameObject.SetActive(true);
        if (playerModel != null) 
            playerModel.gameObject.SetActive(true);

        // Отключаем управление кораблём
        currentShip.ReleaseControl();
        
        currentShip = null;
        isInShip = false;
        
        Debug.Log("[ExitShip] COMPLETE");
    }
}