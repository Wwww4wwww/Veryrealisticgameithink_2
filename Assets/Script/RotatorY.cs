using UnityEngine;

public class RotatorY : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate (new UnityEngine.Vector3 (0,1,0) * Time.deltaTime);
    }
}
