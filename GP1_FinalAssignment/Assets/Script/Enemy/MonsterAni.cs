using UnityEngine;

public class LegStepper : MonoBehaviour
{
    [Header("腿部引用")]
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("行走参数")]
    public float speed = 5f;      // 走路频率
    public float amplitude = 30f; // 摆动幅度（度数）

    void Update()
    {
        // 使用 Time.time 配合 Sin 函数计算当前的摆动角度
        float angle = Mathf.Sin(Time.time * speed) * amplitude;

        // 设置左腿旋转（绕Z轴）
        // Quaternion.Euler(x, y, z)
        leftLeg.localRotation = Quaternion.Euler(0, 0, angle);

        // 设置右腿旋转（角度取反，实现交替步法）
        rightLeg.localRotation = Quaternion.Euler(0, 0, -angle);
    }
}