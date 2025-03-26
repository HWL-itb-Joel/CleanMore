using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowable", menuName = "Weapons/Throwable")]
public class ThrowableWeapon : Weapon
{
    public GameObject throwablePrefab;
    [Range(1,10)]
    public int maxCapacity;
}
