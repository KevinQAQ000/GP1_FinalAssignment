using UnityEngine;

/// <summary>
/// Abstract EnemyBaseState
/// Used to extend and implement various enemy states.
/// </summary>
public abstract class EnemyBaseState : MonoBehaviour
{
    /// <summary>
    /// Called when first entering the state.
    /// </summary>
    /// <param name="enemy">The enemy instance transitioning into this state.</param>
    public abstract void EnemyState(Enemy enemy);

    /// <summary>
    /// Called every frame to update the state logic.
    /// </summary>
    /// <param name="enemy">The enemy instance currently in this state.</param>
    public abstract void OnUpdate(Enemy enemy);
}