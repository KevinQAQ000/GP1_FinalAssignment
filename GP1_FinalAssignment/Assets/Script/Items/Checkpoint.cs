using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Update the player's current respawn point
            DeathMenu.lastCheckpointPos = transform.position;

            Debug.Log("Checkpoint updated to: " + transform.position);
        }
    }
}