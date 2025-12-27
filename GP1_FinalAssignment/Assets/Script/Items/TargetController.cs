using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    public float health = 100f;
    private bool isDead = false;
    private Quaternion originalRotation; // 记录初始旋转
    private float maxHealth = 100f;

    void Start()
    {
        // 记录初始旋转，用于后续回正
        originalRotation = transform.rotation;
        maxHealth = health;
    }

    // --- 核心方法：供你的射线脚本调用 ---
    public void TakeDamage(float damage, Vector3 hitPoint)
    {
        // 如果已经倒下了，就不再接收伤害
        if (isDead) return;

        health -= damage;
        Debug.Log($"靶子被射线击中！扣除 {damage} 血量，剩余: {health}");

        if (health <= 0)
        {
            // 将世界空间的击中点转换为靶子的本地空间坐标
            Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
            // 传入本地坐标的 Z 值，判断是从前还是后击中的
            Die(localHitPoint.z);
        }
    }

    void Die(float hitZ)
    {
        isDead = true;

        // 逻辑：根据击中点的 Z 轴位置决定绕 X 轴倒下的方向
        // hitZ > 0 说明击中点在靶子前方，靶子往后倒 (-90度)
        float angle = hitZ > 0 ? -90f : 90f;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(angle, 0, 0);

        StopAllCoroutines();
        StartCoroutine(HandleTargetCycle(targetRotation));
    }

    IEnumerator HandleTargetCycle(Quaternion targetRotation)
    {
        // 1. 平滑倒下
        float elapsed = 0;
        Quaternion startRotation = transform.rotation;
        while (elapsed < 0.3f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        // 2. 等待 4 秒 (根据你的需求，之前是3秒，这里设为4)
        yield return new WaitForSeconds(4f);

        // 3. 平滑回正
        elapsed = 0;
        Quaternion currentRotation = transform.rotation;
        while (elapsed < 0.5f)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, originalRotation, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation;

        // 4. 重置状态，准备下一次被击中
        health = maxHealth;
        isDead = false;
        Debug.Log("靶子已复位。");
    }
}