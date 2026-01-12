using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

/// <summary>
/// Weapon internal sound clips
/// </summary>
[System.Serializable]
public class SoundClips
{
    public AudioClip shootSound;            // Standard firing sound
    public AudioClip silencerShootASound;   // Silenced firing sound
    public AudioClip reloadSoundAmmotLeft;  // Reload sound 1 (e.g., magazine out)
    public AudioClip reloadSoundOutfAmmo;   // Reload sound 2 (e.g., magazine in)
    public AudioClip aimSound;              // Aiming down sights sound
}

public class AutomaticGun : Weapon
{
    private PlayerController playerController; // Reference to the player controller
    private Camera mainCamera;                 // Reference to the main camera
    public float normalFOV = 60f;              // Default Field of View
    public float aimFOV = 40f;                 // Field of View when aiming (lower = more zoom)
    public float fovSpeed = 10f;               // Speed of FOV transition

    public bool IS_AUTORRIFLE;                 // Is the weapon an automatic rifle?
    public bool IS_SEMIGUN;                    // Is the weapon a semi-auto gun?

    public Transform ShootPoint;               // Raycast origin point
    public Transform BulletShootPoint;         // Muzzle point for bullet trail effects
    public Transform CasingBulletSpawnPoint;   // Ejection port for shell casings

    [Header("Prefabs and Effects")]
    public Transform bulletPrefab;             // Bullet visual prefab
    public Transform casingPrefab;             // Shell casing prefab
    public GameObject bulletHolePrefab;        // Prefab for bullet hole decals
    public float bulletHoleLifeTime = 5f;      // How long bullet holes stay in the scene

    [Header("Weapon Attributes")]
    public float range;                        // Maximum range of the weapon
    public float fireRate;                     // Time between shots
    public float damage;                       // Damage per hit
    private float originRate;                  // Stores original fire rate
    private float SpreadFactor;                // Current accuracy offset/deviation
    private float fireTimer;                   // Timer to track weapon firing cooldown
    private float bulletForce;                 // Physics force applied to bullet prefab
    public int bulletMag;                      // Magazine capacity
    public int currentBullets;                 // Current ammo in the magazine
    public int BulletLeft;                     // Spare ammo remaining
    public float boltShootOffset;              // Distance the bolt moves back when firing
    public float boltReturnSpeed;              // Speed at which the bolt returns
    private Vector3 boltOriginalLocalPos;      // Stores original local position of the bolt
    public float returnTime = 0.05f;           // Time to complete the bolt return
    public CinemachineImpulseSource impulseSource;

    [Header("Reload Animation Components")]
    public Transform magazineObj;              // The magazine game object
    public Transform boltObj;                  // The bolt/trigger game object
    public Vector3 magRelPos = new Vector3(0.25f, -0.7f, 0); // Offset for magazine removal
    public Vector3 magRelRot = new Vector3(0, 30, 0);        // Rotation for magazine removal
    public float moveDuration = 0.3f;          // Duration of magazine movement

    [Header("Visual Effects")]
    public Light muzzleFlashLight;             // Light source for muzzle flash
    private float lightDuraing;                // Duration the flash stays visible
    public ParticleSystem muzzlePatic;         // Muzzle flash particle system
    public ParticleSystem sparkPatic;          // Spark particle system
    public int minSparkEmission = 1;           // Minimum sparks per shot
    public int maxSparkEmission = 7;           // Maximum sparks per shot

    [Header("Audio")]
    public AudioSource mainAudioSource;        // Weapon's main audio source
    public SoundClips soundClips;              // Collection of sound clips

    [Header("UI")]
    public Image[] crossQuarterIgms;           // The four parts of the crosshair
    public float currentExpandingDegree;       // Current crosshair spread size
    private float crossExpandDegree;           // Base expansion increment per frame
    public float maxExpandingDegree;           // Maximum allowed crosshair spread
    public Text ammoTextUI;                    // Text displaying current ammo
    public Text shootModeTextUI;               // Text displaying current fire mode

    public PlayerController.MovementState state; // Current player movement state
    private bool isAiming;                       // Whether the player is aiming (ADS)
    private Vector3 sniperingRiflePosition;      // Default hip-fire weapon position
    public Vector3 sniperingRifleOnPosition;     // Aimed weapon position (ADS)
    public float aimSpeed = 10f;                 // Speed of the ADS transition

    [Header("Input Settings")]
    public KeyCode reloadInputName = KeyCode.R;       // Reload key
    public KeyCode inspectInputName = KeyCode.I;      // Inspect weapon key
    public KeyCode GunShootModeInputName = KeyCode.B; // Switch firing mode key
    public ShootMode shootingName;                    // Current fire mode enum
    private bool GunShootInput;                       // Input flag for shooting
    private int modeNum;                              // Fire mode index
    private string shootModeName;                     // Fire mode display name

    private bool isReloading = false;                 // Lock to prevent multiple reloads

    public void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        mainAudioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        if (mainCamera != null) normalFOV = mainCamera.fieldOfView;

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        sniperingRiflePosition = transform.localPosition; // Record initial weapon position
        muzzleFlashLight.enabled = false;                 // Ensure flash is off initially
        crossExpandDegree = 20f;
        maxExpandingDegree = 60f;
        lightDuraing = 0.03f;
        range = 300f;
        bulletForce = 800f;
        BulletLeft = bulletMag * 3;     // Start with 3 spare magazines
        currentBullets = bulletMag;     // Start with a full magazine
        boltOriginalLocalPos = boltObj.localPosition;
        fireTimer = fireRate;

        // Initialize weapon mode based on gun type
        if (IS_AUTORRIFLE)
        {
            modeNum = 1;
            shootModeName = "Automatic";
            shootingName = ShootMode.AutoRifle;
            UpdateAmmoUI();
        }
        else if (IS_SEMIGUN)
        {
            modeNum = 0;
            shootModeName = "SemiAuto";
            shootingName = ShootMode.SemiGun;
            UpdateAmmoUI();
        }
    }

    private void Update()
    {
        if (IS_AUTORRIFLE)
        {
            // Toggle between Auto and Semi modes
            if (Input.GetKeyDown(GunShootModeInputName) && modeNum != 1)
            {
                modeNum = 1;
                shootModeName = "Automatic";
                shootingName = ShootMode.AutoRifle;
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(GunShootModeInputName) && modeNum != 0)
            {
                modeNum = 0;
                shootModeName = "SemiAuto";
                shootingName = ShootMode.SemiGun;
                UpdateAmmoUI();
            }

            switch (shootingName)
            {
                case ShootMode.AutoRifle:
                    GunShootInput = Input.GetMouseButton(0); // Continuous firing
                    fireRate = 0.13f;
                    break;
                case ShootMode.SemiGun:
                    GunShootInput = Input.GetMouseButtonDown(0); // Single click per shot
                    fireRate = 0.2f;
                    break;
            }
        }
        else
        {
            GunShootInput = Input.GetMouseButtonDown(0);
        }

        // Crosshair expansion logic based on movement state
        state = playerController.state;
        if (state == PlayerController.MovementState.Walking
            && Vector3.SqrMagnitude(playerController.moveDirection) > 0
            && state != PlayerController.MovementState.Running
            && state != PlayerController.MovementState.Crouching)
        {
            ExpaningCrossUpdate(crossExpandDegree);
        }
        else if (state == PlayerController.MovementState.Running)
        {
            ExpaningCrossUpdate(maxExpandingDegree * 2);
        }
        else // Idle or Crouching
        {
            ExpaningCrossUpdate(crossExpandDegree);
        }

        // Fire logic
        if (GunShootInput && currentBullets > 0 && !isReloading)
        {
            GunFire();
            impulseSource.GenerateImpulse(); // Trigger screen shake
        }

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        // Reload trigger
        if (Input.GetKeyDown(reloadInputName) && currentBullets < bulletMag && BulletLeft > 0 && !isReloading)
        {
            StartCoroutine(ReloadRoutine());
        }

        // ADS (Aim Down Sights) Logic
        if (Input.GetMouseButton(1) && !isReloading && !playerController.isRun)
        {
            isAiming = true;
            AimIn();
            transform.localPosition = Vector3.Lerp(transform.localPosition, sniperingRifleOnPosition, Time.deltaTime * aimSpeed);
        }
        else
        {
            isAiming = false;
            AimOut();
            transform.localPosition = Vector3.Lerp(transform.localPosition, sniperingRiflePosition, Time.deltaTime * aimSpeed);
        }

        // Set spread based on aiming state
        SpreadFactor = isAiming ? 0.001f : 0.05f;

        // Smoothly adjust Camera FOV
        if (mainCamera != null)
        {
            float targetFOV = isAiming ? aimFOV : normalFOV;
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovSpeed);
        }

        if (Input.GetKeyDown(inspectInputName))
        {
            // Placeholder for inspect logic
        }
    }

    public override void GunFire()
    {
        if (fireTimer < fireRate || currentBullets <= 0)
        {
            return;
        }

        // Trigger bolt animation when firing
        StopCoroutine("BoltFireRoutine");
        StartCoroutine(BoltFireRoutine());

        // Play visual effects
        StartCoroutine(MuzzleFlashLight());
        muzzlePatic.Emit(1);
        sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission));
        StartCoroutine(Shoot_Cross());

        // Calculate fire direction with spread
        float xSpread = Random.Range(-SpreadFactor, SpreadFactor) * 50f;
        float ySpread = Random.Range(-SpreadFactor, SpreadFactor) * 50f;
        Quaternion spreadRotation = Quaternion.Euler(xSpread, ySpread, 0);
        Vector3 shootDirection = spreadRotation * ShootPoint.forward;

        Debug.DrawRay(ShootPoint.position, shootDirection * 20f, Color.red, 2f);

        // Physics Raycast for hit detection
        if (Physics.Raycast(ShootPoint.position, shootDirection, out RaycastHit hit, range))
        {
            // Instantiate bullet hole decal
            GameObject hole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.LookRotation(hit.normal));
            hole.transform.SetParent(hit.collider.transform);
            Destroy(hole, bulletHoleLifeTime);

            // Instantiate bullet visual trail
            Transform bullet = Instantiate(bulletPrefab, BulletShootPoint.position, BulletShootPoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletForce;
            Destroy(bullet.gameObject, 0.05f);

            // Handle damage logic for general targets
            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage, hit.point);
            }

            // Specific check for Enemy tag
            if (hit.transform.CompareTag("Enemy"))
            {
                hit.transform.gameObject.GetComponent<Enemy>().Health(damage);
            }
        }

        // Spawn shell casing
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);

        // Audio and UI updates
        mainAudioSource.clip = soundClips.shootSound;
        mainAudioSource.Play();
        fireTimer = 0f;
        currentBullets--;
        UpdateAmmoUI();
        ExpendCross(30f);
        impulseSource.GenerateImpulse();
    }

    public IEnumerator MuzzleFlashLight()
    {
        muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(lightDuraing);
        muzzleFlashLight.enabled = false;
    }

    public override void AimIn()
    {
        // Hide crosshair UI when aiming
        for (int i = 0; i < crossQuarterIgms.Length; i++)
        {
            if (crossQuarterIgms[i].gameObject.activeSelf)
                crossQuarterIgms[i].gameObject.SetActive(false);
        }
    }

    public override void AimOut()
    {
        // Show crosshair UI when not aiming
        for (int i = 0; i < crossQuarterIgms.Length; i++)
        {
            if (!crossQuarterIgms[i].gameObject.activeSelf)
                crossQuarterIgms[i].gameObject.SetActive(true);
        }
    }

    public override void DoReloadAnimation() { }

    public override void Reload()
    {
        if (currentBullets == bulletMag || BulletLeft <= 0) return;

        int bulletsToLoad = bulletMag - currentBullets;
        int bulletsToReduce = BulletLeft >= bulletsToLoad ? bulletsToLoad : BulletLeft;

        BulletLeft -= bulletsToReduce;
        currentBullets += bulletsToReduce;
        UpdateAmmoUI();
    }

    public override void ExpaningCrossUpdate(float expanDegree)
    {
        if (currentExpandingDegree < expanDegree - 5)
        {
            ExpendCross(150 * Time.deltaTime);
        }
        else if (currentExpandingDegree > expanDegree + 5)
        {
            ExpendCross(-300 * Time.deltaTime);
        }
    }

    /// <summary>
    /// Updates the crosshair UI offset based on expansion value
    /// </summary>
    public void ExpendCross(float add)
    {
        currentExpandingDegree += add;
        currentExpandingDegree = Mathf.Clamp(currentExpandingDegree, 0, maxExpandingDegree);

        crossQuarterIgms[0].transform.localPosition = new Vector3(-currentExpandingDegree, 0, 0); // Left
        crossQuarterIgms[1].transform.localPosition = new Vector3(currentExpandingDegree, 0, 0);  // Right
        crossQuarterIgms[2].transform.localPosition = new Vector3(0, currentExpandingDegree, 0);  // Top
        crossQuarterIgms[3].transform.localPosition = new Vector3(0, -currentExpandingDegree, 0); // Bottom
    }

    public IEnumerator Shoot_Cross()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            ExpendCross(Time.deltaTime * 500);
        }
    }

    public void UpdateAmmoUI()
    {
        ammoTextUI.text = currentBullets.ToString() + " / " + BulletLeft.ToString();
        shootModeTextUI.text = shootModeName;
    }

    private IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;
        isReloading = true;

        // Cache initial positions for precise restoration
        Vector3 originalMagPos = magazineObj.localPosition;
        Quaternion originalMagRot = magazineObj.localRotation;
        Vector3 originalBoltPos = boltObj.localPosition;

        // Play first reload sound
        mainAudioSource.PlayOneShot(soundClips.reloadSoundAmmotLeft);
        yield return new WaitForSeconds(1.0f);

        // Magazine exit animation
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            t = t * t * (3f - 2f * t); // SmoothStep curve

            magazineObj.localPosition = Vector3.Lerp(originalMagPos, originalMagPos + magRelPos, t);
            magazineObj.localRotation = Quaternion.Lerp(originalMagRot, Quaternion.Euler(magRelRot), t);
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);

        // Magazine insertion animation
        elapsed = 0;
        float returnDuration = moveDuration * 0.8f;
        mainAudioSource.PlayOneShot(soundClips.reloadSoundOutfAmmo);

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Acceleration curve

            magazineObj.localPosition = Vector3.Lerp(originalMagPos + magRelPos, originalMagPos, t);
            magazineObj.localRotation = Quaternion.Lerp(Quaternion.Euler(magRelRot), originalMagRot, t);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // Bolt pull back animation
        Vector3 targetBoltPos = originalBoltPos + new Vector3(0, 0, -0.25f);
        elapsed = 0;
        float pullBackTime = 0.15f;
        while (elapsed < pullBackTime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Pow(1f - (elapsed / pullBackTime), 2); // EaseOutQuad
            boltObj.localPosition = Vector3.Lerp(originalBoltPos, targetBoltPos, t);
            yield return null;
        }

        // Bolt snap back animation
        elapsed = 0;
        float snapBackTime = 0.07f;
        while (elapsed < snapBackTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapBackTime;
            boltObj.localPosition = Vector3.Lerp(targetBoltPos, originalBoltPos, t);
            yield return null;
        }

        // Final cleanup and state update
        magazineObj.localPosition = originalMagPos;
        boltObj.localPosition = originalBoltPos;
        Reload();
        isReloading = false;
    }

    private IEnumerator BoltFireRoutine()
    {
        // Move bolt instantly (recoil)
        boltObj.localPosition = boltOriginalLocalPos + new Vector3(0, 0, boltShootOffset);

        yield return new WaitForSeconds(0.01f); // Very short delay for visual clarity

        // Return bolt smoothly
        float elapsed = 0;
        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnTime;
            boltObj.localPosition = Vector3.Lerp(boltObj.localPosition, boltOriginalLocalPos, t);
            yield return null;
        }
        boltObj.localPosition = boltOriginalLocalPos;
    }

    /// <summary>
    /// Fully refills ammo (used for supply stations)
    /// </summary>
    public void RefillMaxAmmo()
    {
        BulletLeft = bulletMag * 3;
        currentBullets = bulletMag;
        UpdateAmmoUI();
        Debug.Log("Weapon ammo refilled!");
    }

    public enum ShootMode
    {
        AutoRifle,
        SemiGun
    }
}