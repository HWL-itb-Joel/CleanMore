using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MeleeDamage : MonoBehaviour
{
    public float damageInterval = 0.5f;
    private Dictionary<Collider, float> damageTimers = new Dictionary<Collider, float>();

    
    private void OnTriggerStay(Collider other)
    {
        Debug.Log(other);
        if (other.TryGetComponent<NetworkIdentity>(out var identity))
        {
            
        }
    }
}
