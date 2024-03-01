using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] TMP_Text speedText;
    
    void Update()
    {
        // fix the speed to 2 decimal places
        speedText.text = $"Speed: {Mathf.Round(rb.velocity.magnitude * 3.6f)} km/h";
    }
}
