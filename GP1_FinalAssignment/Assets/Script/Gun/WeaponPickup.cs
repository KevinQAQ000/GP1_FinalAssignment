using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    // Set this in the Inspector: e.g., Item A = 0, Item B = 1
    public int weaponID;

    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation in degrees per second")]
    public float rotationSpeed = 50f;

    void Update()
    {
        // Rotate around the world Y-axis (upward axis)
        // Space.World ensures it rotates around the axis perpendicular to the ground, 
        // regardless of the object's local tilt.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the Player
        if (other.CompareTag("Player"))
        {
            // Access the WeaponManager script on the player
            WeaponManager manager = other.GetComponent<WeaponManager>();

            if (manager != null)
            {
                // Unlock the weapon via the manager
                manager.UnlockWeapon(weaponID);

                // Remove the pickup item from the scene
                Destroy(gameObject);
            }
        }
    }
}