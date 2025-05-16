using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AnimationChangesOnline : NetworkBehaviour
{
    public GameObject Tiepod;
    public GameObject FlusFlus;
    public GameObject Graneat;
    public GameObject Motofregona;
    public void changePrimaryWeaponOnline()
    {
        Graneat.SetActive(false);
        Tiepod.SetActive(true);
        FlusFlus.SetActive(false);
        Motofregona.SetActive(false);
    }
    public void changeSecundaryWeaponOnline()
    {
        Graneat.SetActive(false);
        Tiepod.SetActive(false);
        FlusFlus.SetActive(true);
        Motofregona.SetActive(false);
    }

    public void MeleeOnline()
    {
        Graneat.SetActive(false);
        Tiepod.SetActive(false);
        FlusFlus.SetActive(false);
        Motofregona.SetActive(true);
    }

    public void LanchGraneatOnline()
    {
        Graneat.SetActive(false);
    }
}
