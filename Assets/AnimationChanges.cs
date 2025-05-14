using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationChanges : MonoBehaviour
{
    public void changePrimaryWeapon()
    {
        GunController.gunController.PrimaryWeaponIn();
        print("primariaIn");
    }
    public void changeSecundaryWeapon()
    {
        GunController.gunController.SecundaryWeaponIn();
    }

    public void Reload()
    {
        GunController.gunController.ReloadAnimation();
    }
}
