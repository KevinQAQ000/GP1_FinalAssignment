using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    [Header("位置参考")]
    // 在 Inspector 面板把你的 SpawnPoint 拖进去
    public Transform spawnPoint;

    public static Vector3 lastCheckpointPos = Vector3.negativeInfinity;

    [Header("UI 按钮")]
    public Button respawnButton;
    public Button restartButton;

    void Start()
    {
        // 初始逻辑：如果从未触发过存档点，则使用出生点位置
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
        // 重置静态存档位置
        lastCheckpointPos = Vector3.negativeInfinity;

        // 恢复时间流速（防止重载后游戏是静止的）
        Time.timeScale = 1f;

        // 重新加载当前活跃场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExecuteReset(Vector3 pos)
    {
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();

        if (pc != null)
        {
            // 核心贴士：如果传送不生效，通常是因为物理组件（如 CharacterController）在冲突
            CharacterController cc = pc.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            pc.transform.position = pos;
            pc.ResetStatus(pos);

            if (cc != null) cc.enabled = true;

            gameObject.SetActive(false);
            Time.timeScale = 1f; // 确保游戏恢复运行
        }
    }
}