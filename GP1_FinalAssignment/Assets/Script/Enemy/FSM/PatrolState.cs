using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Enemy enters the patrol state.
/// </summary>
public class PatrolState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        // Randomly load the enemy's patrol path
        enemy.LoadPath(enemy.wayPointObj[WaypointManager.Instance.usingIndex[enemy.nameIndex]]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        // Move the enemy toward the current waypoint
        enemy.MoveToTarget();

        // Calculate the distance between the enemy's current position and the current waypoint
        float distance = Vector3.Distance(enemy.transform.position, enemy.wayPoints[enemy.index]);

        // If the distance is less than 0.5m, the enemy has reached the current waypoint
        if (distance <= 0.5f)
        {
            // Increment the index to point to the next waypoint
            enemy.index++;

            // If the index goes out of bounds, reset it to restart the patrol loop
            if (enemy.index >= enemy.wayPoints.Count)
            {
                enemy.index = 0;
            }
        }
        //Debug.Log(distance);

        // Transition from Patrol State to Attack State if targets are detected
        if (enemy.attackList.Count > 0)
        {
            enemy.TransitionToState(enemy.attackState);
        }
    }
}