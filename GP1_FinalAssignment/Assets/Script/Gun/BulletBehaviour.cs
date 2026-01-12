using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    [Tooltip("Prefab for the impact visual effect (e.g., sparks)")]
    public GameObject hitEffectPrefab;

    // Triggered when the bullet's Collider interacts with another Collider
    private void OnCollisionEnter(Collision collision)
    {
        if (hitEffectPrefab != null)
        {
            // Get the first point of contact
            ContactPoint contact = collision.contacts[0];

            // Spawn the effect at the contact point, rotated to face outward from the surface
            Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        }

        // Remove the bullet from the scene
        Destroy(gameObject);
    }
}