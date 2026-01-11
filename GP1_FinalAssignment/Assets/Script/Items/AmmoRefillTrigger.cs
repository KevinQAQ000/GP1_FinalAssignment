using UnityEngine;
using System.Collections;

public class AmmoRefillTrigger : MonoBehaviour
{
    public float delayTime = 3f; // 延迟时间
    private bool isTriggered = false; // 防止重复触发

    // 碰撞检测
    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞的是否是玩家 (确保你的玩家对象 Tag 设为 "Player")
        if (other.CompareTag("Player") && !isTriggered)
        {
            Debug.Log("玩家进入补给范围，3秒后补给...");
            StartCoroutine(RefillRoutine(other.gameObject));
            isTriggered = true; // 开始倒计时后锁定，防止多次触发
        }
    }

    private IEnumerator RefillRoutine(GameObject player)
    {
        // 等待3秒
        yield return new WaitForSeconds(delayTime);

        // 获取玩家身上或子物体中的 AutomaticGun 脚本
        // 如果玩家可以换枪，建议获取当前的 Weapon 基类或具体脚本
        AutomaticGun gun = player.GetComponentInChildren<AutomaticGun>();

        if (gun != null)
        {
            gun.RefillMaxAmmo();
            // 播放补给成功音效（可选）
            // AudioSource.PlayClipAtPoint(refillSound, transform.position);
        }

        // 补给完后，销毁补给箱或者重置触发状态
        // Destroy(gameObject); // 如果是一次性的就销毁
        isTriggered = false;    // 如果是循环使用的就重置
    }
}