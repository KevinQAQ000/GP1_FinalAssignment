using UnityEngine;

/// <summary>
/// State representing the enemy entering its attack behavior.
/// </summary>
public class AttackState : EnemyBaseState
{
    public override void EnemyState(Enemy enemy)
    {
        // Set the first target in the attack list as the current target point.
        enemy.targetPoint = enemy.attackList[0];
    }

    public override void OnUpdate(Enemy enemy)
    {
        // If the enemy has no targets, transition back to the patrol state.
        if (enemy.attackList.Count == 0)
        {
            enemy.TransitionToState(enemy.patrolState);
            return; // Exit early to avoid null references in the logic below.
        }

        // If multiple targets are present, prioritize the closest one.
        if (enemy.attackList.Count > 1)
        {
            for (int i = 0; i < enemy.attackList.Count; i++)
            {
                // Compare the horizontal distance between the enemy and potential targets.
                // Update the target point if a closer target is found.
                if (Mathf.Abs(enemy.transform.position.x - enemy.attackList[i].position.x)
                    < Mathf.Abs(enemy.transform.position.x - enemy.targetPoint.position.x))
                {
                    enemy.targetPoint = enemy.attackList[i];
                }
            }
        }

        // If there is only one target, set it as the primary target point.
        if (enemy.attackList.Count == 1)
        {
            enemy.targetPoint = enemy.attackList[0];
        }

        // TODO: Implement the logic for the enemy attacking the player.
        if (enemy.targetPoint.CompareTag("Player"))
        {
            enemy.AttackAction();
        }

        // Move the enemy towards the current target position.
        enemy.MoveToTarget();
    }
}