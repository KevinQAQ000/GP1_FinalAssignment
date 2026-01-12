using UnityEngine;
using System.Collections;

public class AmmoRefillTrigger : MonoBehaviour
{
    [Tooltip("Delay in seconds before ammo is refilled")]
    public float delayTime = 3f;

    private bool isTriggered = false; // Prevents multiple simultaneous triggers

    // Collision detection
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player (Ensure player tag is set to "Player")
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("Player entered refill zone. Refilling in 3 seconds...");
            StartCoroutine(RefillRoutine(other.gameObject));
            isTriggered = true; // Lock trigger while countdown is active
        }
    }

    private IEnumerator RefillRoutine(GameObject player)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayTime);

        // Attempt to find the AutomaticGun script on the player or its children
        // Note: If the player swaps weapons, consider fetching the active weapon via a base class
        AutomaticGun gun = player.GetComponentInChildren<AutomaticGun>();

        if (gun != null)
        {
            gun.RefillMaxAmmo();

            // Optional: Play a sound effect upon successful refill
            // AudioSource.PlayClipAtPoint(refillSound, transform.position);
        }

        // After refilling, decide whether to destroy the object or reset it
        // Destroy(gameObject); // Use this for one-time pickup items
        isTriggered = false;    // Use this to allow the refill to be used again
    }
}