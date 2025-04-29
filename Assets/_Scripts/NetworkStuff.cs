using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkStuff : NetworkBehaviour
{
    [SerializeField] private GameObject FPSCam = null, TpMeshArms = null, TpMeshWeapon = null;

    void Start()
    {
        if (isLocalPlayer)
        {
            FPSCam.SetActive(true);
            //TpMeshArms.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            //TpMeshWeapon.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        else
        {
            FPSCam.SetActive(false);
            //TpMeshArms.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            //TpMeshWeapon.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    }
}
