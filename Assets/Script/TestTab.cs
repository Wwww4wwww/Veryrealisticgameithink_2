using UnityEngine;

public class TestTab : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("TAB DETECTED in TestTab");
        }
    }
}