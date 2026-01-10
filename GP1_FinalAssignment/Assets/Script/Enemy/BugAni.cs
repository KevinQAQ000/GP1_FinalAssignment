using UnityEngine;

public class SpiderLegStepping : MonoBehaviour
{
    [Header("速率控制")]
    public float currentSpeed = 5f;//摆动频率
    public float range = 40f;//摆动总角度

    [Header("步态设置")]
    [Tooltip("如果勾选，这只脚将与其它脚反向运动")]
    public bool isInverse = false;

    private float offset;
    private Enemy enemyScript;

    void Start()
    {
        //稍微给点随机值，让动作不至于像机械表一样死板
        offset = Random.Range(0f, 0.1f);
        enemyScript = GetComponentInParent<Enemy>();
    }

    void Update()
    {
        if (enemyScript.isDead) return;  //如果敌人死亡则不再执行后续逻辑
        //使用 Sin 函数来实现平滑的来回摆动 (-1 到 1 之间)
        //如果是反向腿，我们在时间上加一个偏移量
        float phase = Time.time * currentSpeed + offset;
        float movement = Mathf.Sin(phase);

        //如果需要反向，直接取反
        if (isInverse)
        {
            movement = -movement;
        }

        //计算最终 Y 轴旋转角度
        float angleY = movement * (range / 2f);

        //应用旋转
        transform.localRotation = Quaternion.Euler(0, angleY, 0);
    }
}