using UnityEngine;

public class Trigger : MonoBehaviour
{
    public LayerMask layerMask;
    public bool isTriggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            isTriggered = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            isTriggered = false;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (layerMask == (layerMask | (1 << other.gameObject.layer)))
        {
            isTriggered = true;
        }
    }
}