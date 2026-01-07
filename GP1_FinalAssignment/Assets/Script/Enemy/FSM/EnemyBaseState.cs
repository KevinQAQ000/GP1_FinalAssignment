using UnityEngine;

/// <summary>
/// 抽象 EnemyBaseState
/// 用于扩展实现敌人的各种状态
/// </summary>
public abstract class EnemyBaseState : MonoBehaviour
{
    public abstract void EnemyState(Enemy enemy);//首次进入状态

    public abstract void OnUpdate(Enemy enemy);//每帧更新状态
}
