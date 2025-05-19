using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkStuff : NetworkBehaviour
{
    [SerializeField] private GameObject FPSCam = null, TPFullEmployee, EmployeeOnline;
    [SerializeField] private SkinnedMeshRenderer mochila, brazos3p, cabeza, cuerpo, flusflus, graneat, motofregona, tiepodgun;
    [SerializeField] private Camera Camera, gunCam;

    void Start()
    {
        if (isLocalPlayer)
        {
            FPSCam.SetActive(true);
            TPFullEmployee.SetActive(true);
            EmployeeOnline.SetActive(false);
            mochila.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            brazos3p.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            cabeza.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            cuerpo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            flusflus.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            graneat.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            motofregona.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            tiepodgun.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            Camera.enabled = true;
            gunCam.enabled = true;
        }
        else
        {
            FPSCam.SetActive(false);
            TPFullEmployee.SetActive(false);
            EmployeeOnline.SetActive(true);
            mochila.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            brazos3p.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            cabeza.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            cuerpo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            flusflus.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            graneat.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            motofregona.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            tiepodgun.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            Camera.enabled = false;
            gunCam.enabled = false;
        }
    }
}
