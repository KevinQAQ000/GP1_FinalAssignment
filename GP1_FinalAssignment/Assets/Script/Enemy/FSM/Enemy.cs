using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Enemy;

/// <summary>
/// Enemy Class
/// Handles state switching, loading patrol routes, and enemy logic.
/// </summary>
public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent; // Navigation agent

    public enum EnemyType { Bug, Monster, Sicence_guys } // Enum for easy extension of enemy types
    [Header("Enemy Attributes")]
    public EnemyType enemyType;

    public float enemyHealth; // Enemy health points
    private float baseSpeed; // Records the initial movement speed
    public Slider slider; // Health UI slider
    public Text getGamageText; // Text display for damage taken
    public GameObject deadEffect; // Hit/Death effect prefab

    public GameObject[] wayPointObj; // Array to store route point objects
    public List<Vector3> wayPoints = new List<Vector3>(); // List to store positions of route points
    public int index; // Current waypoint index
    public int nameIndex; // Index to distinguish different enemies
    public Transform targetPoint; // Enemy target transform

    public EnemyBaseState currentState; // Current state of the enemy
    public string currentStateName; // State name displayed in the Inspector
    public PatrolState patrolState = new PatrolState(); // Defined patrol state
    public AttackState attackState = new AttackState(); // Defined attack state (TODO)

    Vector3 targetPosition; // Target position coordinates

    // List of attack targets (Player) detected in the scene
    public List<Transform> attackList = new List<Transform>(); // List of potential attack targets

    public float attackRate; // Attack frequency
    private float nextAttack = 0f; // Timestamp for the next allowed attack
    public float attackRange; // Attack range
    public bool isDead; // Flag for enemy death status

    [Header("Detection Settings")]
    public float detectionRange = 10f; // Radius of the detection sphere
    public float detectionOffset = 2f;  // Forward offset for the detection center
    public LayerMask playerLayer;      // Layer mask for players to optimize performance

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolState = transform.gameObject.AddComponent<PatrolState>();
        attackState = transform.gameObject.AddComponent<AttackState>();
    }

    void Start()
    {
        baseSpeed = agent.speed; // Save the initial speed set in the inspector
        isDead = false; // Initialize enemy as alive
        slider.minValue = 0; // Initialize health slider minimum
        slider.maxValue = enemyHealth; // Initialize health slider maximum based on HP
        slider.value = enemyHealth; // Set current health slider value
        index = 0; // Initialize waypoint index

        // Ensure the object is correctly placed on the NavMesh
        if (agent.isOnNavMesh)
        {
            TransitionToState(new PatrolState());
        }
        else
        {
            Debug.LogError("Enemy is not on NavMesh! Please check scene baking and object position.");
        }
    }

    void Update()
    {
        if (isDead) return;  // Stop execution if the enemy is dead

        // Execute state update logic every frame
        currentState.OnUpdate(this);
    }

    public void MoveToTarget()
    {
        CheckForPlayer(); // Real-time update of players within range

        if (attackList.Count == 0) // No players in range: continue patrolling
        {
            // Adjust speed back to patrol speeds based on type
            switch (enemyType)
            {
                case EnemyType.Bug:
                    agent.speed = baseSpeed;
                    break;
                case EnemyType.Monster:
                    agent.speed = baseSpeed * 0.8f;
                    break;
                case EnemyType.Sicence_guys:
                    agent.speed = baseSpeed * 1.2f;
                    break;
            }

            agent.destination = wayPoints[index];

            // Check if the current waypoint is reached
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Switch to the next waypoint
                index = (index + 1) % wayPoints.Count;
            }
        }
        else // Player detected: move towards the player
        {
            // Increase speed for pursuit based on type
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

            agent.destination = attackList[0].position;
        }
    }

    /// <summary>
    /// Loads the path by clearing the current list and adding child positions of the target object.
    /// </summary>
    public void LoadPath(GameObject go)
    {
        wayPoints.Clear();
        foreach (Transform T in go.transform)
        {
            wayPoints.Add(T.position);
        }
    }

    /// <summary>
    /// Switches the enemy's current state.
    /// </summary>
    public void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        currentStateName = state.GetType().Name; // Update inspector display
        currentState.EnemyState(this); // Call the entry method for the state
    }

    /// <summary>
    /// Processes damage taken by the enemy and handles death.
    /// </summary>
    public void Health(float damage)
    {
        if (isDead) return;

        getGamageText.text = Mathf.Round(damage).ToString(); // Display damage value
        enemyHealth -= damage;
        slider.value = enemyHealth;

        if (slider.value <= 0)
        {
            isDead = true;
            Destroy(Instantiate(deadEffect, transform.position, Quaternion.identity), 3f); // Instantiate effect and destroy after 3s

            // Disable collision upon death
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
            Destroy(gameObject, 10f); // Destroy enemy object after 10s
        }
    }

    /// <summary>
    /// Executes the primary attack action based on enemy type.
    /// </summary>
    public void AttackAction()
    {
        if (attackList.Count > 0 && attackList[0] != null)
        {
            float dist = Vector3.Distance(transform.position, attackList[0].position);

            if (dist < attackRange)
            {
                // Check cooldown and ensure no attack coroutine is currently running
                if (Time.time >= nextAttack && !isAttacking)
                {
                    nextAttack = Time.time + attackRate;

                    // Execute specific attack coroutines
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
        // Add player to attack list (excluding bullets/other triggers)
        if (!attackList.Contains(other.transform) && !isDead)
        {
            attackList.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        attackList.Remove(other.transform);
    }

    /// <summary>
    /// Performs a sphere overlap check to detect players in front of the enemy.
    /// </summary>
    public void CheckForPlayer()
    {
        // Calculate the center of the detection sphere
        Vector3 detectionCenter = transform.position + transform.forward * detectionOffset;

        // Perform sphere detection
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
        // Calculate the center of the detection sphere
        Vector3 detectionCenter = transform.position + transform.forward * detectionOffset;

        // Draw the solid detection sphere
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(detectionCenter, detectionRange);

        // Draw the wireframe outline
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionCenter, detectionRange);
    }

    /// <summary>
    /// Coroutine: Simulates a jumping dash for the Bug type.
    /// </summary>
    IEnumerator BugDash(Vector3 targetPos)
    {
        agent.isStopped = true;
        Vector3 startPos = transform.position;

        // Calculate horizontal direction and landing spot
        Vector3 dashDirection = (targetPos - startPos).normalized;
        dashDirection.y = 0; // Lock horizontal to prevent diagonal flying
        Vector3 dashTarget = startPos + dashDirection * 2f; // Dash 2 meters

        float elapsed = 0f;
        float dashDuration = 0.3f;
        float jumpHeight = 1f;

        // Dash phase
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / dashDuration;

            // Horizontal: Lerp
            Vector3 currentPos = Vector3.Lerp(startPos, dashTarget, percent);

            // Vertical: Parabola (Sine wave)
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;

            transform.position = currentPos;
            yield return null;
        }

        // Momentary pause at landing for impact
        yield return new WaitForSeconds(0.15f);

        // Fast retreat to original position
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

    bool isAttacking = false;

    /// <summary>
    /// Coroutine: Simulates a heavy ramming attack for the Monster.
    /// Logic: Wind-up (move back) -> Fast straight charge -> Hit stun.
    /// </summary>
    IEnumerator MonsterAttack(Vector3 targetPos)
    {
        if (isAttacking) yield break;
        isAttacking = true;
        Debug.Log("Monster starting Charge Attack!");

        agent.isStopped = true;
        Vector3 startPos = transform.position;
        Vector3 attackDir = (targetPos - startPos).normalized;
        attackDir.y = 0;

        // Preparation: Move backwards slightly
        float prepareTime = 0.4f;
        float elapsed = 0f;
        Vector3 preparePos = startPos - attackDir * 0.8f;
        while (elapsed < prepareTime)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, preparePos, elapsed / prepareTime);
            yield return null;
        }

        // The Charge
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

        yield return new WaitForSeconds(0.5f); // Recovery/Stun phase

        agent.isStopped = false;
        isAttacking = false;
        Debug.Log("Monster Charge Ended");
    }

    /// <summary>
    /// Coroutine: Simulates a clinical slide/dash for the Science Guy.
    /// Logic: Quick dash to target -> Stay and rotate (simulating injection/study) -> Recover.
    /// </summary>
    IEnumerator Sicence_guys_Attack(Vector3 targetPos)
    {
        if (isAttacking) yield break;
        isAttacking = true;

        agent.isStopped = true;
        Vector3 startPos = transform.position;

        // 1. Calculate offset: Science guy stops 1 meter in front of the player
        Vector3 attackDirection = (targetPos - startPos).normalized;
        Vector3 offsetTarget = targetPos - attackDirection * 1.0f;

        // 2. Fast movement phase (Slide)
        float moveDuration = 0.15f;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, offsetTarget, elapsed / moveDuration);
            yield return null;
        }

        transform.position = offsetTarget;

        // 3. Staying phase (Simulating research/attack)
        Debug.Log("Science guy performing 'Research'...");

        float stayDuration = 1.0f;
        float stayElapsed = 0f;
        while (stayElapsed < stayDuration)
        {
            stayElapsed += Time.deltaTime;
            // Face the player while attacking
            if (attackList.Count > 0 && attackList[0] != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(attackList[0].position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            yield return null;
        }

        // 4. Recovery
        agent.isStopped = false;
        isAttacking = false;
    }
}