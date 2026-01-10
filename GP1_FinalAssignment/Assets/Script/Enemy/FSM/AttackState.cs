using UnityEngine;

/// <summary>
/// 敌人进入攻击状态
/// </summary>
public class AttackState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        enemy.targetPoint = enemy.attackList[0];//将攻击列表中的第一个目标设为当前目标
    }
    public override void OnUpdate(Enemy enemy)
    {
        //当前敌人没有目标的时候，切换回巡逻状态
        if (enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
        }

        //当前敌人有目标，可能存在多个目标，优先攻击最近的目标
        if (enemy.attackList.Count > 1)
        {
            //找到最近的目标
            for (int i = 0; i < enemy.attackList.Count; i++)
            {
                //判断敌人和攻击列表里的多个目标的距离差，找到最近的目标
                if (Mathf.Abs(enemy.transform.position.x - enemy.attackList[i].position.x) 
                    < Mathf.Abs(enemy.transform.position.x - enemy.targetPoint.position.x))
                {
                    enemy.targetPoint = enemy.attackList[i];//将最近的目标设为当前目标
                }
            }
        }

        //当前敌人只有一个目标，就只找List中的第一个目标进行攻击
        if (enemy.attackList.Count == 1)
        {
            enemy.targetPoint = enemy.attackList[0];//将攻击列表中的第一个目标设为当前目标
        }

        //TODO: 实现敌人攻击玩家逻辑
        if (enemy.targetPoint.CompareTag("Player"))
        {
            enemy.AttackAction();
        }
        enemy.MoveToTarget();//让敌人朝当前目标位置移动
    }

}
