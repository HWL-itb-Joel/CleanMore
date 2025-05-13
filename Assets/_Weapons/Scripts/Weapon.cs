using UnityEngine;

public enum WeaponType { Principal, Secundaria, Melee, Arrojadiza }

public abstract class Weapon : ScriptableObject
{
    [Header("   Weapon")]
    public string weaponName;
    public WeaponType weaponType;
    public GameObject feedback;
}
