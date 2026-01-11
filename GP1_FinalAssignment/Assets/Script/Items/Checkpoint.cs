using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 更新玩家的当前复活点
            DeathMenu.lastCheckpointPos = transform.position;
            Debug.Log("复活点已更新: " + transform.position);
        }
    }
}