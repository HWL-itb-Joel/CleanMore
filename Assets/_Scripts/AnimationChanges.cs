using UnityEngine;

public class AnimationChanges : MonoBehaviour
{
    public GunController gunController;

    public void changePrimaryWeapon()
    {
        gunController.PrimaryWeaponIn();
    }

    public void changeSecundaryWeapon()
    {
        gunController.SecundaryWeaponIn();
    }

    public void CanShootAgain()
    {
        gunController.ActiveShoot();
    }

    public void Melee()
    {
        gunController.MeleeIn();
    }

    public void Reload()
    {
        gunController.ReloadAnimation();
    }

    public void LanchGraneat()
    {
        gunController.Graneat();
    }
}
