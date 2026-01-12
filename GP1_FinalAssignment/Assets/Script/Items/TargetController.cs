using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    public float health = 100f;            // Initial health
    private bool isDead = false;           // Tracks if the target has fallen
    private Quaternion originalRotation;   // Stores the starting rotation for resetting
    private float maxHealth = 100f;        // Stores maximum health for restoration

    void Start()
    {
        // Record initial rotation to ensure accurate resetting later
        originalRotation = transform.rotation;
        // Initialize maxHealth based on the assigned health value
        maxHealth = health;
    }

    public void TakeDamage(float damage, Vector3 hitPoint) // Receives damage value and hit position
    {
        // If the target is already down, ignore further damage
        if (isDead) return;

        health -= damage; // Subtract health
        Debug.Log($"Target hit! Damage: {damage}, Remaining: {health}");

        if (health <= 0)
        {
            // Convert the world space hit point to the target's local space
            Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
            // Use the local Z value to determine if the hit came from the front or back
            Die(localHitPoint.z);
        }
    }

    void Die(float hitZ) // Handles the falling logic based on local Z-axis hit position
    {
        isDead = true; // Mark as down

        // Logic: Determine rotation direction around X-axis based on hit side
        // hitZ > 0 means hit from front, fall backward (-90 degrees)
        // hitZ < 0 means hit from back, fall forward (90 degrees)
        float angle = hitZ > 0 ? -90f : 90f;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(angle, 0, 0);

        StopAllCoroutines(); // Stop any active routines to prevent movement conflicts
        StartCoroutine(HandleTargetCycle(targetRotation)); // Begin the fall, wait, and reset cycle
    }

    IEnumerator HandleTargetCycle(Quaternion targetRotation) // Manages the animation sequence
    {
        // Smooth Fall
        float elapsed = 0;
        Quaternion startRotation = transform.rotation; // Capture current rotation
        while (elapsed < 0.3f) // Fall duration: 0.3 seconds
        {
            // Spherical linear interpolation for smooth rotation
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation; // Ensure final rotation is exact

        // Wait for 4 seconds before resetting
        yield return new WaitForSeconds(4f);

        // Smooth Reset
        elapsed = 0;
        Quaternion currentRotation = transform.rotation;
        while (elapsed < 0.5f) // Reset duration: 0.5 seconds
        {
            transform.rotation = Quaternion.Slerp(currentRotation, originalRotation, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation; // Return to original state

        // Reset variables for the next interaction
        health = maxHealth;
        isDead = false;
        Debug.Log("Target has reset.");
    }
}