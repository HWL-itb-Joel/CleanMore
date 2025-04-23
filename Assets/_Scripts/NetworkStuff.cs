using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkStuff : NetworkBehaviour
{
    [SerializeField] private GameObject FPSCam = null, TpMesh = null;

    void Start()
    {
        if (isLocalPlayer)
        {
            FPSCam.SetActive(true);
            //pMesh.SetActive(false);
        }
        else
        {
            FPSCam.SetActive(false);
            //TpMesh.SetActive(true);
    }
    }
}
