using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum fireRateType { Auto, Semi, OneShot , OverHeating}

public class OverHeating : Weapon
{
    public fireRateType rateType;
    [Header("   OverHeating Settings")]
    public float heatThreshold;
    public float currentHeat;
    public float overheatCooldown;
    public bool isOverheated = false;

    public float baseCoolingRate;
    public float maxCoolingRate;
    public float coolingAccelerationTime;
    public float coolingMultiplier = 0f;
    public bool isCooling = false;
}
