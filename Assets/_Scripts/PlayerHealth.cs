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
    [SerializeField] MultiplayerFPSMovement fpsScript;

    private void Start()
    {
        if (!isLocalPlayer) return;
        health_txt.text = healthValue.ToString();
        health_bar.value = healthValue;
        fpsScript.GetComponent<MultiplayerFPSMovement>();
    }

    [Server]
    public void GetDamage(float damage_)
    {
        if (!isLocalPlayer) return;
        healthValue -= damage_;
        if (healthValue <= 0)
        {
            fpsScript.enabled = false;
            print("die");
        }
    }

    void HealthValueChanged(float oldValue, float newValue)
    {
        health_txt.text = healthValue.ToString();
        health_bar.value = healthValue;
    }
}
