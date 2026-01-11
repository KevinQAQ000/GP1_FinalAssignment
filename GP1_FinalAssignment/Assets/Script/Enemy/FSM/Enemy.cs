using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Enemy;

/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线等功能
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;//导航代理

    public enum EnemyType { Bug, Monster, Sicence_guys } // 定义枚举，方便以后扩展
    [Header("怪物属性")]
    public EnemyType enemyType;

    public float enemyHealth; //敌人血量
    private float baseSpeed; // 记录初始移动速度
    public Slider slider;//血量滑动条
    public Text getGamageText;//显示受到伤害的文本
    public GameObject deadEffect;//受击特效


    public GameObject[] wayPointObj; //存放敌人路线点的数组
    public List<Vector3> wayPoints = new List<Vector3>(); //存放敌人路线点位置的列表
    public int index; //下标值
    public int nameIndex;//敌人名称下标，用于区分不同敌人
    public Transform targetPoint;//敌人目标位置

    public EnemyBaseState currentState; //敌人当前状态
    public string currentStateName; // 方便在面板查看状态名
    public PatrolState patrolState = new PatrolState(); //定义敌人巡逻状态
    public AttackState attackState = new AttackState(); //定义敌人攻击状态 TODO

    Vector3 targetPosition; //目标位置

    //敌人的攻击目标列表 场景中有敌人（也就是玩家）用列表存储攻击目标
    public List<Transform> attackList = new List<Transform>();//敌人攻击目标列表

    public float attackRate;//攻击频率
    private float nextAttack = 0f;//下次攻击时间
    public float attackRange; //攻击范围
    public bool isDead;//敌人是否死亡标志

    [Header("检测设置")]
    public float detectionRange = 10f; // 检测范围半径
    public float detectionOffset = 2f;  // 检测中心向前的偏移量
    public LayerMask playerLayer;      // 指定玩家所在的层，提高性能

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();
    }
    void Start()
    {
        baseSpeed = agent.speed; // 获取并保存面板上设置的初始速度
        isDead = false;//初始化敌人未死亡
        slider.minValue = 0;//初始化血量滑动条满血状态
        slider.maxValue = enemyHealth;//初始化血量滑动条最大值为敌人血量
        slider.value = enemyHealth;//初始化血量滑动条当前值为敌人血量
        index = 0;//初始化下标值
        // 建议：确保物体已经贴地
        if (agent.isOnNavMesh)
        {
            TransitionToState(new PatrolState());
        }
        else
        {
            Debug.LogError("敌人不在NavMesh上！请检查场景烘焙和物体位置。");
        }
        //TransitionToState(new PatrolState());//敌人初始状态为巡逻状态
    }

    
    void Update()
    {
        if (isDead) return;  //如果敌人死亡则不再执行后续逻辑
        //敌人移动状态要一直执行
        currentState.OnUpdate(this);//每帧更新当前状态
    }


    public void MoveToTarget()
    {
        CheckForPlayer(); // 实时更新检测范围内的敌人

        if (attackList.Count == 0)//如果攻击目标列表为空，说明没有玩家在攻击范围内，继续巡逻
        {
            switch (enemyType)
            {
                case EnemyType.Bug:
                    agent.speed = baseSpeed; // 恢复正常速度
                    break;
                case EnemyType.Monster:
                    agent.speed = baseSpeed * 0.8f; // 恢复正常速度
                    break;
                case EnemyType.Sicence_guys:
                    agent.speed = baseSpeed * 1.2f; // 恢复正常速度
                    break;
            }
            //如果攻击目标列表为空，说明没有玩家在攻击范围内，继续巡逻
            //targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);//计算朝目标点移动后的位置
            agent.destination = wayPoints[index];
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // 切换到下一个路点
                index = (index + 1) % wayPoints.Count;
            }
            //if (wayPoints.Count > 0)
            //{
            //    agent.destination = wayPoints[index];
            //}

        }
        else//如果攻击目标列表不为空，说明有玩家在攻击范围内，朝玩家移动
        {
            switch (enemyType)
            {
                case EnemyType.Bug:
                    agent.speed = baseSpeed * 1.4f; 
                    break;
                case EnemyType.Monster:
                    agent.speed = baseSpeed * 1.2f;
                    break;
                case EnemyType.Sicence_guys:
                    agent.speed = baseSpeed * 1.6f;
                    break;
            }
            //targetPosition = Vector3.MoveTowards(transform.position, attackList[0].position, agent.speed * Time.deltaTime);//计算朝玩家移动后的位置
            //agent.destination = targetPosition;//设置导航代理的目标位置
            agent.destination = attackList[0].position;
        }
    }

    public void LoadPath(GameObject go)
    {
        wayPoints.Clear();//清空列表
        //遍历路线点对象的子物体，将其位置存入列表
        foreach (Transform T in go.transform)
        {
            wayPoints.Add(T.position);
        }
    }
    /// <summary>
    /// 切换敌人状态
    /// </summary>
    /// <param name="state"></param>
    public void TransitionToState(EnemyBaseState state)//切换敌人状态
    {
        currentState = state;//设置当前状态为传入的状态
        currentStateName = state.GetType().Name; // 更新面板显示的名称
        currentState.EnemyState(this);//调用状态的首次进入方法
    }

    /// <summary>
    /// 敌人收到伤害 扣除血量
    /// </summary>
    /// <param name="damage"></param>
    public void Health(float damage)
    {
        if (isDead) return;//如果敌人已经死亡则不再扣血

        getGamageText.text = Mathf.Round(damage).ToString();//显示受到的伤害值
        enemyHealth -= damage;//扣除血量
        slider.value = enemyHealth;//更新血量滑动条
        if (slider.value <= 0)
        {
            isDead = true; // 标记死亡
            Destroy(Instantiate(deadEffect, transform.position, Quaternion.identity), 3f);//实例化受击特效并在2秒后销毁
            //死亡后身体下移
            Collider col = GetComponent<Collider>();//获取敌人碰撞体
            if (col != null)
            {
                col.enabled = false; // 禁用碰撞体
            }
            Destroy(gameObject, 10f); //3秒后销毁敌人对象
        }
        
    }

    /// <summary>
    /// 敌人攻击玩家，普通攻击
    /// </summary>
    public void AttackAction()
    {
        //检查是否有目标
        if (attackList.Count > 0 && attackList[0] != null)
        {
            //使用 attackList[0] 计算距离，而不是 targetPoint
            float dist = Vector3.Distance(transform.position, attackList[0].position);

            if (dist < attackRange)
            {
                if (Time.time >= nextAttack && !isAttacking) //确保当前没有正在进行的攻击协程
                {
                    nextAttack = Time.time + attackRate;

                    // 执行对应的协程
                    switch (enemyType)
                    {
                        case EnemyType.Bug:
                            StartCoroutine(BugDash(attackList[0].position));
                            break;
                        case EnemyType.Monster:
                            StartCoroutine(MonsterAttack(attackList[0].position));
                            break;
                        case EnemyType.Sicence_guys:
                            StartCoroutine(Sicence_guys_Attack(attackList[0].position));
                            break;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //攻击列表要剔除子弹，否则会一直添加子弹到攻击列表中
        if (!attackList.Contains(other.transform) && !isDead && other.CompareTag("Bullet"))
        {
            attackList.Add(other.transform);//将玩家添加到攻击目标列表
        }
    }
    private void OnTriggerExit(Collider other)
    {
        attackList.Remove(other.transform);//将玩家从攻击目标列表移除
    }

    public void CheckForPlayer()
    {
        // 计算检测中心点位置
        Vector3 detectionCenter = transform.position + transform.forward * detectionOffset;

        // 在新的中心点进行球体检测
        Collider[] colliders = Physics.OverlapSphere(detectionCenter, detectionRange, playerLayer);

        attackList.Clear();
        foreach (var col in colliders)
        {
            if (!isDead)
            {
                attackList.Add(col.transform);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // 计算检测中心点位置
        Vector3 detectionCenter = transform.position + transform.forward * detectionOffset;

        // 绘制填充球体
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(detectionCenter, detectionRange);

        // 绘制外圈线条
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionCenter, detectionRange);
    }

    /// <summary>
    /// 模拟“跳跃冲刺”动作的协程
    /// </summary>
    IEnumerator BugDash(Vector3 targetPos)
    {
        agent.isStopped = true;
        Vector3 startPos = transform.position;

        // 计算水平方向和最终落点
        Vector3 dashDirection = (targetPos - startPos).normalized;
        dashDirection.y = 0; // 锁定水平方向，防止斜着飞
        Vector3 dashTarget = startPos + dashDirection * 2f;// 冲刺2米距离

        float elapsed = 0f;
        float dashDuration = 0.3f; // 略微增加时间，让跳跃过程清晰
        float jumpHeight = 1f;     // 跳跃的最大高度

        //冲刺阶段
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / dashDuration;

            // 水平位移：线性插值
            Vector3 currentPos = Vector3.Lerp(startPos, dashTarget, percent);

            // 垂直位移：抛物线 ( y = 4h * x * (1-x) )
            // percent 为 0.5 时，y 达到最大高度 jumpHeight
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;

            transform.position = currentPos;
            yield return null;
        }

        // 停留攻击点 (落地瞬间)
        // 这里可以执行实际的伤害逻辑，如：DoDamage();
        yield return new WaitForSeconds(0.15f);

        //快速回撤 (此时直接贴地走) 
        elapsed = 0f;
        Vector3 landPos = transform.position;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(landPos, startPos, elapsed / 0.15f);
            yield return null;
        }

        agent.isStopped = false;
    }

    bool isAttacking = false; // 需要在类中定义这个变量
    //Todo: 实现怪物撞击攻击协程
    /// <summary>
    /// 模拟“怪物撞击”动作的协程
    /// 逻辑：蓄力(后退一点) -> 快速直线冲刺 -> 命中停顿
    /// </summary>
    IEnumerator MonsterAttack(Vector3 targetPos)
    {
        if (isAttacking) yield break; // 防止重复触发
        isAttacking = true;
        Debug.Log("怪物开始撞击攻击！");

        agent.isStopped = true;
        Vector3 startPos = transform.position;
        Vector3 attackDir = (targetPos - startPos).normalized;
        attackDir.y = 0;

        // 蓄力：颜色变红（可选）或后退
        float prepareTime = 0.4f;
        float elapsed = 0f;
        Vector3 preparePos = startPos - attackDir * 0.8f; // 稍微加大后退幅度，更明显
        while (elapsed < prepareTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, preparePos, elapsed / prepareTime);
            yield return null;
        }

        // 冲撞
        elapsed = 0f;
        float chargeDuration = 0.2f;
        Vector3 chargeTarget = startPos + attackDir * (attackRange + 1.5f);
        Vector3 posBeforeCharge = transform.position;

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(posBeforeCharge, chargeTarget, elapsed / chargeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // 僵直

        agent.isStopped = false;
        isAttacking = false;
        Debug.Log("怪物撞击结束");
    }

    //Todo: 实现科学家攻击协程 快速移动到目标位置并停留一段时间
    /// <summary>
    /// 模拟“科学家攻击”动作的协程
    /// 逻辑：快速闪现/滑步到玩家附近 -> 在玩家身边停留（模拟施法或注射） -> 结束攻击
    /// </summary>
    IEnumerator Sicence_guys_Attack(Vector3 targetPos)
    {
        if (isAttacking) yield break;
        isAttacking = true;

        agent.isStopped = true; // 停止导航，由协程控制位移
        Vector3 startPos = transform.position;

        // 1. 计算偏移位置：科学家通常不会直接重叠在玩家身上，而是移动到其前方 1 米处
        Vector3 attackDirection = (targetPos - startPos).normalized;
        Vector3 offsetTarget = targetPos - attackDirection * 1.0f;

        // 2. 快速移动阶段 (滑步/闪现)
        float moveDuration = 0.15f; // 极短时间完成位移
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            // 使用 Lerp 实现平滑但极快的移动
            transform.position = Vector3.Lerp(startPos, offsetTarget, elapsed / moveDuration);
            yield return null;
        }

        // 确保位置精准到达
        transform.position = offsetTarget;

        // 3. 停留阶段 (停留时间较长，模拟执行科学操作/攻击)
        // 可以在这里触发伤害事件：DealDamageToPlayer();
        Debug.Log("科学家正在进行‘研究’(攻击)...");

        // 可以在停留时添加一个小效果，比如旋转向玩家
        float stayDuration = 1.0f; // 停留1秒
        float stayElapsed = 0f;
        while (stayElapsed < stayDuration)
        {
            stayElapsed += Time.deltaTime;
            // 攻击时始终盯着玩家
            if (attackList.Count > 0 && attackList[0] != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(attackList[0].position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            yield return null;
        }

        // 4. 结束并恢复
        agent.isStopped = false;
        isAttacking = false;
    }
}
