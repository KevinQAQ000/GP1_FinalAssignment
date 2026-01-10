using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// 敌人进入巡逻状态
/// </summary>
/// 
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        //随机加载敌人巡逻路线
        enemy.LoadPath(enemy.wayPointObj[WaypointManager.Instance.usingIndex[enemy.nameIndex]]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        enemy.MoveToTarget();//让敌人朝当前导航点移动
        //计算敌人当前位置与当前导航点的距离
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);
        if (distance <= 0.5f)//如果距离小于0.5米，说明到达当前导航点
        {
           enemy.index++;//下标值加1，指向下一个导航点
           if (enemy.index >= enemy.wayPoints.Count)//如果下标值超出范围
           {
               enemy.index = 0;//重置下标值，重新开始巡逻
           }
        }
        //Debug.Log(distance);

        //To do: 巡逻状态切换到攻击状态
        if (enemy.attackList.Count > 0)
        {
            enemy.TransitionToState(enemy.attackState);
        }

    }


}
