using UnityEngine;

public class SimpleWalkAnimation : MonoBehaviour
{
    [Header("腿部引用")]
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("走路设置")]
    public float speed = 5.0f;      // 摆动速度
    public float maxAngle = 30.0f;  // 最大摆动角度

    void Update()
    {
        // 使用 Mathf.Sin 随时间产生 -1 到 1 的变化
        // Time.time * speed 决定了步伐的快慢
        float movement = Mathf.Sin(Time.time * speed);

        // 计算当前帧的角度
        float angle = movement * maxAngle;

        // 应用旋转
        // 左腿向前摆时，右腿应该向后摆，所以右腿取反 (-angle)
        leftLeg.localRotation = Quaternion.Euler(0, 0, angle);
        rightLeg.localRotation = Quaternion.Euler(0, 0, -angle);
    }
}