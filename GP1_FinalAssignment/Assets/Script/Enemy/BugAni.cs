using UnityEngine;

public class SpiderLegStepping : MonoBehaviour
{
    [Header("Speed Control")]
    [Tooltip("The frequency of the leg swing movement.")]
    public float currentSpeed = 5f;

    [Header("Swing Range")]
    [Tooltip("The total arc angle of the swing.")]
    public float range = 40f;

    [Header("Gait Settings")]
    [Tooltip("If checked, this leg will move in the opposite direction of the standard movement.")]
    public bool isInverse = false;

    private float offset;
    private Enemy enemyScript;

    void Start()
    {
        // Add a slight random offset to prevent the movement from looking perfectly synchronized/robotic.
        offset = Random.Range(0f, 0.1f);
        enemyScript = GetComponentInParent<Enemy>();
    }

    void Update()
    {
        // Stop execution if the enemy is dead.
        if (enemyScript.isDead) return;

        // Use the Sine function to achieve a smooth back-and-forth oscillation (between -1 and 1).
        float phase = Time.time * currentSpeed + offset;
        float movement = Mathf.Sin(phase);

        // Invert the movement if the isInverse flag is set.
        if (isInverse)
        {
            movement = -movement;
        }

        // Calculate the final rotation angle for the Y-axis.
        float angleY = movement * (range / 2f);

        // Apply the rotation locally.
        transform.localRotation = Quaternion.Euler(0, angleY, 0);
    }
}