using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour
{
    public float health = 100f;// 初始血量
    private bool isDead = false;// 是否已经倒下
    private Quaternion originalRotation; // 记录初始旋转
    private float maxHealth = 100f;// 最大血量

    void Start()
    {
        // 记录初始旋转，用于后续回正
        originalRotation = transform.rotation;//初始化旋转信息
        maxHealth = health;//初始化最大血量
    }

    public void TakeDamage(float damage, Vector3 hitPoint)//接收伤害的方法 传入伤害值和击中点
    {
        // 如果已经倒下了，就不再接收伤害
        if (isDead) return;

        health -= damage;//扣除血量 
        Debug.Log($"靶子被射线击中！扣除 {damage} 血量，剩余: {health}");

        if (health <= 0)
        {
            // 将世界空间的击中点转换为靶子的本地空间坐标
            Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
            // 传入本地坐标的 Z 值，判断是从前还是后击中的
            Die(localHitPoint.z);
        }
    }

    void Die(float hitZ)//靶子倒下的方法 传入击中点的本地Z轴位置
    {
        isDead = true;//标记为已倒下

        // 逻辑：根据击中点的 Z 轴位置决定绕 X 轴倒下的方向
        // hitZ > 0 说明击中点在靶子前方，靶子往后倒 (-90度)
        float angle = hitZ > 0 ? -90f : 90f;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(angle, 0, 0);

        StopAllCoroutines();// 停止之前的协程，防止多次触发时出现冲突
        StartCoroutine(HandleTargetCycle(targetRotation));// 启动协程处理倒下和回正的过程
    }

    IEnumerator HandleTargetCycle(Quaternion targetRotation)//协程处理靶子倒下、等待和回正的过程
    {
        //平滑倒下
        float elapsed = 0;
        Quaternion startRotation = transform.rotation;//记录当前旋转
        while (elapsed < 0.3f)//倒下过程持续0.3秒
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / 0.3f);//插值计算当前旋转
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;//确保最终旋转精确

        //等待 4 秒 (根据你的需求，之前是3秒，这里设为4)
        yield return new WaitForSeconds(4f);

        //平滑回正
        elapsed = 0;
        Quaternion currentRotation = transform.rotation;
        while (elapsed < 0.5f)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, originalRotation, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = originalRotation;

        //重置状态，准备下一次被击中
        health = maxHealth;
        isDead = false;
        Debug.Log("靶子已复位。");
    }
}