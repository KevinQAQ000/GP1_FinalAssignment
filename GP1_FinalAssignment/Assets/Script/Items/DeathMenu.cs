using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    [Header("Position References")]
    // Drag your SpawnPoint object into this slot in the Inspector
    public Transform spawnPoint;

    public static Vector3 lastCheckpointPos = Vector3.negativeInfinity;

    [Header("UI Buttons")]
    public Button respawnButton;
    public Button restartButton;

    void Start()
    {
        // Initial logic: If no checkpoint has been triggered, default to the spawn point position
        if (lastCheckpointPos == Vector3.negativeInfinity && spawnPoint != null)
        {
            lastCheckpointPos = spawnPoint.position;
        }

        respawnButton.onClick.AddListener(RespawnAtCheckpoint);
        restartButton.onClick.AddListener(RestartGame);
    }

    public void RespawnAtCheckpoint()
    {
        ExecuteReset(lastCheckpointPos);
    }

    public void RestartGame()
    {
        // Reset the static checkpoint position
        lastCheckpointPos = Vector3.negativeInfinity;

        // Resume time scale (prevents the game from being frozen after reloading)
        Time.timeScale = 1f;

        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExecuteReset(Vector3 pos)
    {
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();

        if (pc != null)
        {
            // Pro tip: If teleportation fails, it's often due to physics components (like CharacterController) conflicting
            CharacterController cc = pc.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            pc.transform.position = pos;
            pc.ResetStatus(pos);

            if (cc != null) cc.enabled = true;

            gameObject.SetActive(false);
            Time.timeScale = 1f; // Ensure the game resumes running
        }
    }
}