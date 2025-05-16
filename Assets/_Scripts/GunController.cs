using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class GunController : NetworkBehaviour
{
    [Header("Guns Selected")]
    private Gun weaponInfo;
    public Gun primaryWeapon;
    public GameObject primaryWeaponObj;
    public Gun secundayWeapon;
    public GameObject secundayWeaponObj;
    public MeleeWeapon meleeWeapon;
    bool canSwitchMelee;
    bool meleeEquiped;
    public GameObject meleeWeaponObj;
    public BoxCollider meleeCollider;
    public GameObject GraneatRig;
    public Transform GraneatSpawnPoint;
    public GameObject graneatPref;
    public Vector3 launchGraneat;

    [Header("UI Settings")]
    public TextMeshProUGUI currentAmmo;
    public TextMeshProUGUI maxAmmo;

    public TextMeshProUGUI currentWeapon;
    public GameObject HUD;
    public GameObject MenuOnPlay;
    public GameObject MenuControlls;
    bool onMenu;
    public Image feedback;

    [Header("Input Settings")]
    [SerializeField] public PlayerInput PlayerInputs;
    private InputAction fireAction;
    private InputAction reloadAction;
    private InputAction lookAction;
    private InputAction scrollWeapons;
    private InputAction alternativeShoot;
    private InputAction equipMelee;
    private InputAction throwAction;

    public Animator Animations;
    public Animator ThirtPersonAnim;

    private Vector2 lookInput;
    private float scroll;

    [Header("Gun Settings")]
    public int _currentAmmoInClip;
    public int _ammoInReserve;
    public bool _canShoot;
    public bool _canReload;
    public bool _canAlternShoot;
    bool _canThrow;
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

    [SerializeField] TrailRenderer bulletTrail;
    //[SerializeField] ParticleSystem ImpactParticleSystem;
    [SerializeField] Transform primarySpawnPos;
    [SerializeField] Transform secondarySpawnPos;
    [SerializeField] Image actualWeaponSprite;

    private void Awake()
    {
        if (!isLocalPlayer) return;
        onMenu = false;
        meleeEquiped = false;
        canSwitchMelee = true;
        meleeCollider = meleeWeaponObj.GetComponent<BoxCollider>();
        primaryWeaponObj.SetActive(true);
        camRotation = GetComponentInParent<Transform>().rotation;
        weaponInfo = primaryWeapon;
        actualWeaponSprite.sprite = weaponInfo.Sprite;
        scrollWeapons = PlayerInputs.actions.FindActionMap("OnGround").FindAction("PrimaryWeapon");
        fireAction = PlayerInputs.actions.FindActionMap("OnGround").FindAction("Fire");
        reloadAction = PlayerInputs.actions.FindActionMap("OnGround").FindAction("Reload");
        lookAction = PlayerInputs.actions.FindActionMap("OnGround").FindAction("Look");
        alternativeShoot = PlayerInputs.actions.FindActionMap("OnGround").FindAction("AlternativeShoot");
        equipMelee = PlayerInputs.actions.FindActionMap("OnGround").FindAction("Melee");
        throwAction = PlayerInputs.actions.FindActionMap("OnGround").FindAction("Throweable");

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
        _canThrow = true;
        _canReload = true;
        _canAlternShoot = true;
        weaponInfo.currentHeat = 0;
        firstThroweableEnabled = true;
        _gunEnabled = true;
        feedback.color = Color.clear;
        mouseSensitiity = Settings.instance.sensitivityValue;
        ActualizeUI();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (!onMenu)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                MultiplayerFPSMovement.FPSMovement.enabled = false;
                onMenu = true;
                HUD.SetActive(false);
                MenuOnPlay.SetActive(true);
                MenuControlls.SetActive(false);
            }

            //transform.rotation = Quaternion.Euler(camRotation.x -90, camRotation.y, 0);
            DetermineRotation();
            if (fireAction.IsPressed() && !MultiplayerFPSMovement.FPSMovement.isRunning)
            {
                Animations.SetBool("isShooting", true);
                ThirtPersonAnim.SetBool("isShooting", true);
                if (_canShoot && _currentAmmoInClip > 0 && _gunEnabled && (weaponInfo.weaponType == WeaponType.Principal || weaponInfo.weaponType == WeaponType.Secundaria))
                {
                    StartCoroutine(ShootGun());
                }
                else if (meleeEquiped)
                {
                    StartCoroutine(ShootMelee());
                }
            }
            else if (weaponInfo.rateType == fireRateType.OneShot && !fireAction.IsPressed() && shootEnded && _gunEnabled)
            {
                _canShoot = true;
            }
            else if (!fireAction.IsPressed())
            {
                Animations.SetBool("isShooting", false);
                ThirtPersonAnim.SetBool("isShooting", false);
            }

            if (MultiplayerFPSMovement.FPSMovement.isRunning)
            {
                _canShoot = false;
                Animations.SetBool("isRunning", true);
                ThirtPersonAnim.SetBool("isRunning", true);
            }
            else if (!MultiplayerFPSMovement.FPSMovement.isRunning && !fireAction.IsPressed())
            {
                _canShoot = true;
                Animations.SetBool("isRunning", false);
                ThirtPersonAnim.SetBool("isRunning", false);
            }

            if (reloadAction.IsPressed() && _currentAmmoInClip < weaponInfo.clipSize && _ammoInReserve > 0 && _canReload && _gunEnabled)
            {
                Animations.SetTrigger("Reload");
                ThirtPersonAnim.SetTrigger("Reload");
                _canReload = false;
                _canShoot = false;
            }
            if (scroll != 0 && _gunEnabled && !meleeEquiped && !MultiplayerFPSMovement.FPSMovement.isRunning)
            {
                _canReload = false;
                _canShoot = false;
                Animations.SetTrigger("ChangeWeapon");
                ThirtPersonAnim.SetTrigger("ChangeWeapon");
                Animations.SetTrigger("switchWeapon");
                ThirtPersonAnim.SetTrigger("switchWeapon");
                ChangeGunWeapon();
            }

            if (throwAction.IsPressed() && _canThrow)
            {
                _canThrow = false;
                Animations.SetTrigger("Graneat");
                ThirtPersonAnim.SetTrigger("Graneat");
                GraneatRig.SetActive(true);
                meleeWeaponObj.SetActive(false);
                primaryWeaponObj.SetActive(false);
                secundayWeaponObj.SetActive(false);
                if (weaponInfo.weaponType == WeaponType.Principal)
                {
                    _gunEnabled = true;
                    Animations.SetBool("isPrimaryEnabled", true);
                    ThirtPersonAnim.SetBool("isPrimaryEnabled", true);
                }
                else if (weaponInfo.weaponType == WeaponType.Secundaria)
                {
                    _gunEnabled = true;
                    Animations.SetBool("isPrimaryEnabled", false);
                    ThirtPersonAnim.SetBool("isPrimaryEnabled", false);
                }
            }

            if (equipMelee.IsPressed() && canSwitchMelee)
            {
                canSwitchMelee = false;
                if (!meleeEquiped && (weaponInfo.weaponType == WeaponType.Principal || weaponInfo.weaponType == WeaponType.Secundaria))
                {
                    meleeEquiped = true;
                    _gunEnabled = false;
                    Animations.SetTrigger("ChangeWeapon");
                    Animations.SetTrigger("switchMelee");
                }
                else if (meleeEquiped)
                {
                    if (weaponInfo.weaponType == WeaponType.Principal)
                    {
                        meleeEquiped = false;
                        _gunEnabled = true;
                        Animations.SetTrigger("ChangeWeapon");
                        Animations.SetTrigger("switchMelee");
                        Animations.SetBool("isPrimaryEnabled", true);
                    }
                    else if (weaponInfo.weaponType == WeaponType.Secundaria)
                    {
                        meleeEquiped = false;
                        _gunEnabled = true;
                        Animations.SetTrigger("ChangeWeapon");
                        Animations.SetTrigger("switchMelee");
                        Animations.SetBool("isPrimaryEnabled", false);
                    }
                    
                }
            }

            if (alternativeShoot.IsPressed() && weaponInfo.alternativeShoot && _canAlternShoot)
            {
                Animations.SetTrigger("switchShoot");
                ChangeAlternativeShoot();
                _canShoot = true;
            }
            else if (!alternativeShoot.IsPressed())
            {
                _canAlternShoot = true;
            }
            if (!meleeWeapon.isOverheated && !fireAction.IsPressed())
            {
                meleeCollider.enabled = false;
                if (meleeWeapon.currentHeat > 0)
                {
                    // Si no estamos disparando, comenzar el enfriamiento progresivo
                    if (!meleeWeapon.isCooling)
                    {
                        meleeWeapon.isCooling = true;
                        StartCoroutine(IncreaseCoolingRate());
                    }

                    float coolingRate = Mathf.Lerp(meleeWeapon.baseCoolingRate, meleeWeapon.maxCoolingRate, meleeWeapon.coolingMultiplier);
                    meleeWeapon.currentHeat -= coolingRate * Time.deltaTime;
                    meleeWeapon.currentHeat = Mathf.Max(0, meleeWeapon.currentHeat);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                onMenu = false;
                MultiplayerFPSMovement.FPSMovement.enabled = true;
                HUD.SetActive(true);
                MenuControlls.SetActive(false);
                MenuOnPlay.SetActive(false);
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

        float normalizedLookY = Mathf.InverseLerp(50, -85f, _currentRotation.y);

        // Pasarlo al Animator
        ThirtPersonAnim.SetFloat("UpRange", normalizedLookY);


        Vector3 targetPosition = new Vector3(initialPosition.x - mouseX, initialPosition.y - mouseY, initialPosition.z);

        if (!MultiplayerFPSMovement.FPSMovement.grounded)
        {
            float fallSpeed = MultiplayerFPSMovement.FPSMovement.currentMovement.y;
            float verticalOffset = Mathf.Clamp01(-fallSpeed / 10f) * swayAmount;

            Vector3 targetPos = initialPosition + new Vector3(0, verticalOffset, 0);

            targetPosition = targetPosition + targetPos;
        }

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
            ActualizeUI();
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
                ActualizeUI();
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
                ActualizeUI();
                DetermineRecoil();
                RatCastForEnemy(weaponInfo.sprayBullets);
            }
            
            yield return new WaitForSeconds(weaponInfo.fireRate);
            shootEnded = true;
        }
        else if (weaponInfo.rateType == fireRateType.OverHeating && !weaponInfo.isOverheated)
        {
            weaponInfo.isCooling = false;
            meleeCollider.enabled = true;
            weaponInfo.currentHeat += 0.1f;
            Debug.Log(" Heat level: " + weaponInfo.currentHeat);
            if (weaponInfo.currentHeat >= weaponInfo.heatThreshold)
            {
                StartCoroutine(Overheat());
            }
        }
    }

    public void ActiveGamePlay()
    {
        onMenu = false;
        MultiplayerFPSMovement.FPSMovement.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator ShootMelee()
    {
        if (meleeWeapon.rateType == fireRateType.Auto)
        {
            Animations.SetBool("isShooting", true);
            _canShoot = false;
            _currentAmmoInClip--;
            DetermineRecoil();
            RatCastForEnemy(weaponInfo.sprayBullets);
            yield return new WaitForSeconds(weaponInfo.fireRate);
            _canShoot = true;
        }
        else if (meleeWeapon.rateType == fireRateType.OneShot)
        {
            Animations.SetTrigger("shoot");
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
        else if (meleeWeapon.rateType == fireRateType.OverHeating && !weaponInfo.isOverheated)
        {
            meleeWeapon.isCooling = false;
            meleeCollider.enabled = true;
            meleeWeapon.currentHeat += 0.1f;
            Debug.Log(" Heat level: " + meleeWeapon.currentHeat);
            if (meleeWeapon.currentHeat >= meleeWeapon.heatThreshold)
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
                if (weaponInfo.weaponType == WeaponType.Principal)
                {
                    TrailRenderer trail = Instantiate(bulletTrail, primarySpawnPos.position, Quaternion.identity);

                    StartCoroutine(SpawnTrail(trail, hit));
                }
                else if (weaponInfo.weaponType == WeaponType.Secundaria)
                {
                    TrailRenderer trail = Instantiate(bulletTrail, secondarySpawnPos.position, Quaternion.identity);

                    StartCoroutine(SpawnTrail(trail, hit));
                }

                if (hit.collider.TryGetComponent<NetworkIdentity>(out var identity))
                {
                    if (identity.TryGetComponent<IEnemyHealth>(out IEnemyHealth r))
                    {
                        CmdDamageEnemy(identity, weaponInfo.damage);
                    }
                }
            }
            catch { }
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPos = Trail.transform.position;

        while(time < 0.1f)
        {
            time += Time.deltaTime;
            Trail.transform.position = Vector3.Lerp(startPos, hit.point, time);

            yield return null;
        }

        Trail.transform.position = hit.point;
        //Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));

        Destroy(Trail.gameObject, Trail.time);
    }

    void ChangeGunWeapon()
    {
        int ammoInClip = _currentAmmoInClip;
        int ammoReserve = _ammoInReserve;

        if (weaponInfo == primaryWeapon)
        {
            weaponInfo = secundayWeapon;

        }
        else if (weaponInfo == secundayWeapon)
        {
            weaponInfo = primaryWeapon;
        }

        _ammoInReserve = _ammoInReserveReserve;
        _currentAmmoInClip = _currentAmmoInClipReserve;

        _ammoInReserveReserve = ammoReserve;
        _currentAmmoInClipReserve = ammoInClip;
    }

    public void Reload()
    {
        if (weaponInfo.weaponType == WeaponType.Secundaria)
        {
            _currentAmmoInClip = weaponInfo.clipSize;
            return;
        }
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
        if (_ammoInReserve <= 0)
        {
            _ammoInReserve = 0;
        }
    }

    #region Animations
    public void ActiveShoot()
    {
        _canAlternShoot = true;
        _canReload = true;
        _canShoot = true;
    }

    public void PrimaryWeaponIn()
    {
        ActualizeUI();
        currentWeapon.text = weaponInfo.name;
        primaryWeaponObj.SetActive(true);
        secundayWeaponObj.SetActive(false);
        meleeWeaponObj.SetActive(false);
        canSwitchMelee = true;
        actualWeaponSprite.sprite = weaponInfo.Sprite;
    }

    public void SecundaryWeaponIn()
    {
        ActualizeUI();
        currentWeapon.text = weaponInfo.name;
        primaryWeaponObj.SetActive(false);
        secundayWeaponObj.SetActive(true);
        meleeWeaponObj.SetActive(false);
        canSwitchMelee = true;
        actualWeaponSprite.sprite = weaponInfo.Sprite;
    }

    public void MeleeIn()
    {
        currentAmmo.text = "XX";
        maxAmmo.text = "XX";
        currentWeapon.text = meleeWeapon.name;
        primaryWeaponObj.SetActive(false);
        secundayWeaponObj.SetActive(false);
        meleeWeaponObj.SetActive(true);
        canSwitchMelee = true;
        actualWeaponSprite.sprite = meleeWeapon.Sprite;
    }

    public void ReloadAnimation()
    {
        Reload();
        ActualizeUI();
        _canReload = true;
        _canShoot = true;
    }

    [Command]
    public void CmdDamageEnemy(NetworkIdentity enemyID, int damage)
    {
        if (enemyID.TryGetComponent<IEnemyHealth>(out var enemy))
        {
            enemy.TakeDamage(damage);
            enemy.FlashOnHit(); // opcionalmente puedes hacer que esto se vea en todos
        }
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
        Animations.SetTrigger("overheated");
        meleeWeapon.isOverheated = true;
        Debug.Log("🚨 Weapon overheated! Cooling down...");
        yield return new WaitForSeconds(meleeWeapon.overheatCooldown);
        meleeWeapon.currentHeat = 0f;
        meleeWeapon.isOverheated = false;
        Debug.Log("✅ Weapon cooled down. Ready to fire!");
    }

    private IEnumerator IncreaseCoolingRate()
    {
        meleeWeapon.coolingMultiplier = 0f;
        while (weaponInfo.isCooling && meleeWeapon.coolingMultiplier < 1f)
        {
            meleeWeapon.coolingMultiplier += Time.deltaTime / meleeWeapon.coolingAccelerationTime;
            meleeWeapon.coolingMultiplier = Mathf.Clamp01(meleeWeapon.coolingMultiplier);
            yield return null;
        }
    }

    private void ActualizeUI()
    {
        currentAmmo.text = _currentAmmoInClip.ToString();
        maxAmmo.text = _ammoInReserve.ToString();
    }

    public void StartFlashFeedBack()
    {
        StartCoroutine(FlashFeedbackDamage());
    }

    public IEnumerator FlashFeedbackDamage()
    {
        feedback.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        float duration = 0.5f; // Duración del fade-out
        float elapsed = 0f;
        Color startColor = Color.white;
        Color endColor = new Color(1f, 1f, 1f, 0f); // Color blanco con alpha 0

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            feedback.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        feedback.color = endColor; // Asegurarse de que termine en transparente
    }


    public void Graneat()
    {
        _canThrow = true;
        GameObject grenadeInstance = Instantiate(graneatPref, GraneatSpawnPoint.position, Quaternion.identity);

        Rigidbody rb = grenadeInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 throwDirection = Camera.main.transform.forward;
            float throwForce = 15f; // ajusta esto a tu gusto
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

    }
}
