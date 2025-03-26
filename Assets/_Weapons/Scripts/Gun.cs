using UnityEngine;


[CreateAssetMenu(fileName = "NewGun", menuName = "Weapons/Gun")]
public class Gun : OverHeating
{
    [Header("   Gun Type Settings")]
    public float fireRate;
    public int bulletsByShot = 1;
    public bool sprayBullets = false;
    public float sprayIndicator;
    public float bulletsFireRate;

    [Header("   General Settings")]
    public float maxDistance;
    public int damage;
    public int clipSize;
    public int reservedAmmoCapacity;

    [Header("   Alternative Gun Settings")]
    public bool alternativeShoot = false;
    public fireRateType alternativeType;
    public int alternativeBulletsByShoot = 1;
    public bool alternativeSpray = false;
    public float alternativeSprayIndicator;
    public int alternativeDamage;
    public float alternativeFireRate;
}
