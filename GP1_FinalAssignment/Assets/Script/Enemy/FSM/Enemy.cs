using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人类
/// 实现状态切换，加载敌人巡逻路线等功能
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public GameObject[] wayPointObj; //存放敌人路线点的数组
    public List<Vector3> wayPoints = new List<Vector3>(); //存放敌人路线点位置的列表
    public int index; //下标值

    public EnemyBaseState currentState; //敌人当前状态
    private PatrolState patrolState = new PatrolState(); //定义敌人巡逻状态
    //private AttackState attackState = new AttackState(); //定义敌人攻击状态 TODO

    Vector3 targetPosition; //目标位置

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        index = 0;
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

}
