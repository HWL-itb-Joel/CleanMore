using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkStuff : NetworkBehaviour
{
    [SerializeField] private GameObject FPSCam = null, TpMeshArms = null, TPFullEmployee, meshHead;

    void Start()
    {
        if (isLocalPlayer)
        {
            FPSCam.SetActive(true);
            TPFullEmployee.SetActive(false);
            meshHead.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            TpMeshArms.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            FPSCam.SetActive(false);
            TPFullEmployee.SetActive(true);
            meshHead.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            TpMeshArms.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
