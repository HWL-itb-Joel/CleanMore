using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [Header("Guns Selected")]
    private Gun weaponInfo;
    public Gun primaryWeapon;
    public GameObject primaryWeaponObj;
    public Gun secundayWeapon;
    public GameObject secundayWeaponObj;
    public MeleeWeapon meleeWeapon;
    public GameObject meleeWeaponObj;
    public ThrowableWeapon throwableWeapon1;
    public ThrowableWeapon throwableWeapon2;

    [Header("Input Settings")]
    public InputActionAsset PlayerInputs;
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction lookAction;
    private InputAction scrollWeapons;
    private InputAction alternativeShoot;

    public Animator Animations;

    private Vector2 lookInput;
    private float scroll;

    [Header("Gun Settings")]
    public int _currentAmmoInClip;
    public int _ammoInReserve;
    public bool _canShoot;
    public bool _canReload;
    public bool _canAlternShoot;
    bool run;
    private bool shootEnded;
    public bool _gunEnabled;
    private bool firstThroweableEnabled;

    public int _currentAmmoInClipReserve;
    public int _ammoInReserveReserve;

    [Header("Mouse Settings")]
    public float mouseSensitiity = 1;
    Vector2 _currentRotation;

    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float maxSway = 0.1f;
    public float smoothSpeed = 6f;

    private Vector3 initialPosition;
    private Quaternion camRotation;
    [Header("Sway Settings")]
    public bool randomizeRecoil;
    public Vector2 randomRecoilConstrains;

    public static GunController gunController;

    private void Awake()
    {
        if (GunController.gunController != this && GunController.gunController != null)
        {
            Destroy(this);
        }
        else
        {
            GunController.gunController = this;
        }
        GunController.gunController = this;
        camRotation = GetComponentInParent<Transform>().rotation;
        weaponInfo = primaryWeapon;
        scrollWeapons = PlayerInputs.FindActionMap("OnGround").FindAction("PrimaryWeapon");
        fireAction = PlayerInputs.FindActionMap("OnGround").FindAction("Fire");
        reloadAction = PlayerInputs.FindActionMap("OnGround").FindAction("Reload");
        lookAction = PlayerInputs.FindActionMap("OnGround").FindAction("Look");
        alternativeShoot = PlayerInputs.FindActionMap("OnGround").FindAction("AlternativeShoot");

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        scrollWeapons.performed += context => scroll = context.ReadValue<float>();
    }

    private void OnEnable()
    {
        alternativeShoot.Enable();
        scrollWeapons.Enable();
        fireAction.Enable();
        reloadAction.Enable();
        lookAction.Enable();
    }

    private void OnDisable()
    {
        alternativeShoot.Disable();
        scrollWeapons.Disable();
        fireAction.Disable();
        reloadAction.Disable();
        lookAction.Disable();
    }

    private void Start()
    {
        initialPosition = transform.localPosition;
        _currentAmmoInClip = weaponInfo.clipSize;
        _ammoInReserve = weaponInfo.reservedAmmoCapacity;
        _currentAmmoInClipReserve = secundayWeapon.clipSize;
        _ammoInReserveReserve = secundayWeapon.reservedAmmoCapacity;
        _canShoot = true;
        _canReload = true;
        _canAlternShoot = true;
        weaponInfo.currentHeat = 0;
        firstThroweableEnabled = true;
        _gunEnabled = true;
    }

    private void Update()
    {
        //transform.rotation = Quaternion.Euler(camRotation.x -90, camRotation.y, 0);
        DetermineRotation();
        if (fireAction.IsPressed() && !MultiplayerFPSMovement.FPSMovement.isRunning)
        {
            if (_canShoot && _currentAmmoInClip > 0 && _gunEnabled)
            {
                StartCoroutine(ShootGun());
            }
            else if (!_gunEnabled && weaponInfo.weaponType == WeaponType.Melee)
            {
                //Desarrollo melee
            }
            else if (!_gunEnabled && weaponInfo.weaponType == WeaponType.Arrojadiza)
            {
                //arrojadiza
                if (firstThroweableEnabled)
                {

                }
                else
                {

                }
            }
        }
        else if (weaponInfo.rateType == fireRateType.OneShot && !fireAction.IsPressed() && shootEnded && _gunEnabled)
        {
            _canShoot = true;
        }

        if (MultiplayerFPSMovement.FPSMovement.isRunning)
        {
            _canShoot = false;
            Animations.SetBool("isRunning", true);
        }
        else if (!MultiplayerFPSMovement.FPSMovement.isRunning && !fireAction.IsPressed())
        {
            _canShoot = true;
            Animations.SetBool("isRunning", false);
        }



        if (reloadAction.IsPressed() && _currentAmmoInClip < weaponInfo.clipSize && _ammoInReserve > 0 && weaponInfo.weaponType != WeaponType.Secundaria && _canReload)
        {
            Animations.SetTrigger("Reloading");
            _canReload = false;
            _canShoot = false;
        }
        else if (reloadAction.IsPressed() && _currentAmmoInClip < weaponInfo.clipSize && weaponInfo.weaponType == WeaponType.Secundaria)
        {
            _currentAmmoInClip = weaponInfo.clipSize;
        }
        if (scroll != 0)
        {
            ChangeGunWeapon();
            _canShoot = true;
        }

        if (alternativeShoot.IsPressed() && weaponInfo.alternativeShoot && _canAlternShoot)
        {
            ChangeAlternativeShoot();
            _canShoot = true;
        }
        else if (!alternativeShoot.IsPressed())
        {
            _canAlternShoot = true;
        }
        if (!weaponInfo.isOverheated)
        {
            if (weaponInfo.currentHeat > 0)
            {
                // Si no estamos disparando, comenzar el enfriamiento progresivo
                if (!weaponInfo.isCooling)
                {
                    weaponInfo.isCooling = true;
                    StartCoroutine(IncreaseCoolingRate());
                }

                float coolingRate = Mathf.Lerp(weaponInfo.baseCoolingRate, weaponInfo.maxCoolingRate, weaponInfo.coolingMultiplier);
                weaponInfo.currentHeat -= coolingRate * Time.deltaTime;
                weaponInfo.currentHeat = Mathf.Max(0, weaponInfo.currentHeat);
            }
        }
    }

    void DetermineRotation()
    {
        Vector2 mouseAxis = new Vector2(lookInput.x, -lookInput.y);

        mouseAxis *= mouseSensitiity;
        _currentRotation += mouseAxis;

        _currentRotation.y = Mathf.Clamp(_currentRotation.y, -85, 50);

        float mouseX = lookInput.x * swayAmount;
        float mouseY = lookInput.y * swayAmount;

        mouseX = Mathf.Clamp(mouseX, -maxSway, maxSway);
        mouseY = Mathf.Clamp(mouseY, -maxSway, maxSway);

        Vector3 targetPosition = new Vector3(initialPosition.x - mouseX, initialPosition.y - mouseY, initialPosition.z);

        // Interpolar suavemente la posición del arma
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);

        transform.root.localRotation = Quaternion.AngleAxis(_currentRotation.x, Vector3.up);
        transform.parent.localRotation = Quaternion.AngleAxis(_currentRotation.y, Vector3.right);
    }

    void DetermineRecoil()
    {
        transform.localPosition -= Vector3.forward * 0.075f;

        if (randomizeRecoil)
        {
            float xRecoil = Random.Range(-randomRecoilConstrains.x, randomRecoilConstrains.x);
            float yRecoil = Random.Range(-randomRecoilConstrains.y, randomRecoilConstrains.y);

            Vector2 recoild = new Vector2(xRecoil, yRecoil);

            _currentRotation += recoild;
        }
    }

    IEnumerator ShootGun()
    {
        Debug.Log("Pium");
        if (weaponInfo.rateType == fireRateType.Auto)
        {
            _canShoot = false;
            _currentAmmoInClip--;
            DetermineRecoil();
            RatCastForEnemy(weaponInfo.sprayBullets);
            yield return new WaitForSeconds(weaponInfo.fireRate);
            _canShoot = true;
        }
        else if (weaponInfo.rateType == fireRateType.Semi)
        {
            _canShoot = false;
            for (int i = 0; i < weaponInfo.bulletsByShot; i++)
            {
                _currentAmmoInClip--;
                DetermineRecoil();
                RatCastForEnemy(weaponInfo.sprayBullets);
                yield return new WaitForSeconds(weaponInfo.bulletsFireRate);
            }
            yield return new WaitForSeconds(weaponInfo.fireRate);
            _canShoot = true;
        }
        else if (weaponInfo.rateType == fireRateType.OneShot)
        {
            for (int i = 0; i < weaponInfo.bulletsByShot; i++)
            {
                if (_currentAmmoInClip <= 0) break;
                _canShoot = false;
                shootEnded = false;
                _currentAmmoInClip--;
                DetermineRecoil();
                RatCastForEnemy(weaponInfo.sprayBullets);
            }
            
            yield return new WaitForSeconds(weaponInfo.fireRate);
            shootEnded = true;
        }
        else if (weaponInfo.rateType == fireRateType.OverHeating && !weaponInfo.isOverheated)
        {
            weaponInfo.isCooling = false;
            weaponInfo.currentHeat += 0.1f;
            Debug.Log(" Heat level: " + weaponInfo.currentHeat);
            if (weaponInfo.currentHeat >= weaponInfo.heatThreshold)
            {
                StartCoroutine(Overheat());
            }
        }
    }

    void RatCastForEnemy(bool srpay)
    {
        RaycastHit hit;
        Vector3 sprayIndicator = new Vector3(Random.Range(-weaponInfo.sprayIndicator, weaponInfo.sprayIndicator), Random.Range(-weaponInfo.sprayIndicator, weaponInfo.sprayIndicator),0);
        if (!srpay)
        {
            sprayIndicator = Vector2.zero;
        }

        Vector3 dir = (Camera.main.transform.forward * 160) + sprayIndicator;
        if (Physics.Raycast(Camera.main.transform.position, dir, out hit, weaponInfo.maxDistance))
        {
            //Debug.DrawRay(Camera.main.transform.position, dir, Color.red, 834485f);
            try
            {
                GameObject feedback = Instantiate(weaponInfo.feedback);
                feedback.transform.position = hit.point;
                feedback.transform.rotation = Quaternion.Euler(hit.normal);
                Destroy(feedback, 3f);

                if (hit.transform.TryGetComponent<IEnemyHealth>(out IEnemyHealth r))
                {
                    r.TakeDamage(weaponInfo.damage);
                }
            }
            catch { }
        }
    }

    void ChangeGunWeapon()
    {
        int ammoInClip = _currentAmmoInClip;
        int ammoReserve = _ammoInReserve;

        if (weaponInfo == primaryWeapon)
        {
            weaponInfo = secundayWeapon;
            Animations.SetTrigger("ChangeWeapon");

        }
        else if (weaponInfo == secundayWeapon)
        {
            weaponInfo = primaryWeapon;
            Animations.SetTrigger("ChangeWeapon");
        }

        _ammoInReserve = _ammoInReserveReserve;
        _currentAmmoInClip = _currentAmmoInClipReserve;

        _ammoInReserveReserve = ammoReserve;
        _currentAmmoInClipReserve = ammoInClip;
    }

    public void Reload()
    {
        int amountNeeded = weaponInfo.clipSize - _currentAmmoInClip;
        if (amountNeeded >= _ammoInReserve)
        {
            _currentAmmoInClip += _ammoInReserve;
            _ammoInReserve -= amountNeeded;
        }
        else
        {
            _currentAmmoInClip = weaponInfo.clipSize;
            _ammoInReserve -= amountNeeded;
        }
    }

    #region Animations

    public void PrimaryWeaponIn()
    {
        primaryWeaponObj.SetActive(true);
        secundayWeaponObj.SetActive(false);
        Animations.SetLayerWeight(1, 0);
    }

    public void SecundaryWeaponIn()
    {
        primaryWeaponObj.SetActive(false);
        secundayWeaponObj.SetActive(true);
        Animations.SetLayerWeight(1, 1);
    }

    public void ReloadAnimation()
    {
        Reload();
        _canReload = true;
        _canShoot = true;
    }
    #endregion

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.forward * 180,Color.red);
    }*/

    void ChangeAlternativeShoot()
    {
        _canAlternShoot = false;
        Debug.Log("Alternative Shoot Enabled");
        fireRateType rateType = weaponInfo.rateType;
        int tempBulletsByShoot = weaponInfo.bulletsByShot;
        bool tempSparyBullets = weaponInfo.sprayBullets;
        float tempSprayIndicator = weaponInfo.sprayIndicator;
        int tempDamage = weaponInfo.damage;
        float tempFireRate = weaponInfo.fireRate;

        weaponInfo.rateType = weaponInfo.alternativeType;
        weaponInfo.bulletsByShot = weaponInfo.alternativeBulletsByShoot;
        weaponInfo.sprayBullets = weaponInfo.alternativeSpray;
        weaponInfo.sprayIndicator = weaponInfo.alternativeSprayIndicator;
        weaponInfo.damage = weaponInfo.alternativeDamage;
        weaponInfo.fireRate = weaponInfo.alternativeFireRate;

        weaponInfo.alternativeType = rateType;
        weaponInfo.alternativeBulletsByShoot = tempBulletsByShoot;
        weaponInfo.alternativeSpray = tempSparyBullets;
        weaponInfo.alternativeSprayIndicator = tempSprayIndicator;
        weaponInfo.alternativeDamage = tempDamage;
        weaponInfo.alternativeFireRate = tempFireRate;
    }

    private IEnumerator Overheat()
    {
        weaponInfo.isOverheated = true;
        Debug.Log("🚨 Weapon overheated! Cooling down...");
        yield return new WaitForSeconds(weaponInfo.overheatCooldown);
        weaponInfo.currentHeat = 0f;
        weaponInfo.isOverheated = false;
        Debug.Log("✅ Weapon cooled down. Ready to fire!");
    }

    private IEnumerator IncreaseCoolingRate()
    {
        weaponInfo.coolingMultiplier = 0f;
        while (weaponInfo.isCooling && weaponInfo.coolingMultiplier < 1f)
        {
            weaponInfo.coolingMultiplier += Time.deltaTime / weaponInfo.coolingAccelerationTime;
            weaponInfo.coolingMultiplier = Mathf.Clamp01(weaponInfo.coolingMultiplier);
            yield return null;
        }
    }

}
