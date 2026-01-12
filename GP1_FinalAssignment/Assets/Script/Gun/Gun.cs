using Cinemachine;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Bullets")]
    public Bullet BulletPrefab; // Reference to the bullet prefab
    public Transform FirePoint; // Point where the bullet spawns

    [Header("Reload/Fire Rate")]
    public float ReloadTime = 1f; // The current cooldown remaining before the next shot
    public float ReloadTimer = 0f; // The fixed delay between shots (Fire Rate)

    [Header("Audio")]
    public AudioSource GunAudio; // Reference to the AudioSource component
    public AudioClip ShootClip; // Audio clip for the firing sound

    [Header("ScreenShake")]
    public CinemachineImpulseSource GunShake; // Cinemachine Impulse for camera shake

    [Header("GunType")]
    public GunType gunType; // Current selected gun type

    public enum GunType // Enum to define different firing behaviors
    {
        Pistol,
        Rifle,
        SubmachineGun,
        Shotgun
    }

    void Update()
    {
        // Countdown the reload timer based on real time
        ReloadTime -= Time.deltaTime;

        // If the cooldown hasn't finished, prevent firing
        if (ReloadTime > 0f)
            return;

        // Determine firing logic based on the selected gun type
        switch (gunType)
        {
            case GunType.Pistol:
                ReloadTimer = 0.5f; // Semi-auto: 0.5s delay
                if (Input.GetMouseButtonDown(0)) // Triggered on initial click
                {
                    Fire();
                }
                break;

            case GunType.Rifle:
                ReloadTimer = 0.1f; // Full-auto: 0.1s delay
                if (Input.GetMouseButton(0)) // Triggered while holding down
                {
                    Fire();
                }
                break;

            case GunType.SubmachineGun:
                ReloadTimer = 0.05f; // High fire rate: 0.05s delay
                if (Input.GetMouseButton(0))
                {
                    Fire();
                }
                break;

            case GunType.Shotgun:
                ReloadTimer = 1f; // Slow fire rate: 1s delay
                if (Input.GetMouseButtonDown(0))
                {
                    ReloadTime = ReloadTimer;

                    // Spawn 5 bullets with randomized spread to simulate a shotgun blast
                    for (int i = 0; i < 5; i++)
                    {
                        // Calculate a random offset for the spread
                        float spreadAngle = Random.Range(-10f, 10f);
                        Quaternion spreadRotation = Quaternion.Euler(FirePoint.rotation.eulerAngles + new Vector3(0, spreadAngle, 0));

                        // Instantiate bullet with spread rotation
                        Instantiate(BulletPrefab, FirePoint.position, spreadRotation);
                    }
                    TriggerEffects();
                }
                break;
        }
    }

    // Helper method to handle standard firing logic
    private void Fire()
    {
        ReloadTime = ReloadTimer; // Reset the cooldown
        Instantiate(BulletPrefab, FirePoint.position, FirePoint.rotation); // Spawn the bullet
        TriggerEffects();
    }

    // Helper method to trigger screen shake and audio
    private void TriggerEffects()
    {
        if (GunShake != null) GunShake.GenerateImpulse(); // Trigger camera shake
        if (GunAudio != null && ShootClip != null) GunAudio.PlayOneShot(ShootClip); // Play sound
    }
}