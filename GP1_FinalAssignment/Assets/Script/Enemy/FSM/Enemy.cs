using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线等功能
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;

    public float enemyHealth; //敌人血量
    public Slider slider;//血量滑动条
    public Text getGamageText;//显示受到伤害的文本
    public GameObject deadEffect;//受击特效


    public GameObject[] wayPointObj; //存放敌人路线点的数组
    public List<Vector3> wayPoints = new List<Vector3>(); //存放敌人路线点位置的列表
    public int index; //下标值
    public int nameIndex;//敌人名称下标，用于区分不同敌人

    public EnemyBaseState currentState; //敌人当前状态
    private PatrolState patrolState = new PatrolState(); //定义敌人巡逻状态
    //private AttackState attackState = new AttackState(); //定义敌人攻击状态 TODO

    Vector3 targetPosition; //目标位置

    public bool isDead;//敌人是否死亡标志

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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
        targetPosition = Vector3.MoveTowards(transform.position, wayPoints[index], agent.speed * Time.deltaTime);//计算朝目标点移动后的位置
        agent.destination = targetPosition;//设置导航代理的目标位置
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
    
}
