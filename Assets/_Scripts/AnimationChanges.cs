using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AnimationChanges : MonoBehaviour
{
    public void changePrimaryWeapon()
    {
        GunController.gunController.PrimaryWeaponIn();
    }
    public void changeSecundaryWeapon()
    {
        GunController.gunController.SecundaryWeaponIn();
    }

    public void CanShootAgain()
    {
        GunController.gunController.ActiveShoot();
    }

    public void Melee()
    {
        GunController.gunController.MeleeIn();
    }

    public void Reload()
    {
        GunController.gunController.ReloadAnimation();
    }

    public void LanchGraneat()
    {
        GunController.gunController.Graneat();
    }
}
