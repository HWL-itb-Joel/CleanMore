using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar(hook = nameof(HealthValueChanged))] float healthValue = 100f;
    [SerializeField] TMP_Text health_txt = null;
    [SerializeField] Slider health_bar = null;

    private void Start()
    {
        if (!isLocalPlayer) return;
        health_txt.text = healthValue.ToString();
        health_bar.value = healthValue;
    }

    [Server]
    public void GetDamage(float damage_)
    {
        healthValue -= damage_;
        
    }

    void HealthValueChanged(float oldValue, float newValue)
    {
        health_txt.text = healthValue.ToString();
        health_bar.value = healthValue;
        if (newValue <= 0)
        {
            print("die");
        }
    }
}
