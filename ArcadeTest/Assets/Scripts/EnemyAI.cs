using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    // Enum for fear levels
    private enum FearLevel
    {
        Timid,
        Normal,
        Aggressive
    }

    private FearLevel fearLevel;  // The fear level assigned to this enemy

    // Enemy states
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Retreat,
        Dead
    }

    private State currentState = State.Idle;  // Start in the Idle state

    [Header("State Transitions")]
    public float detectionRange = 10f;       // Range within which enemy detects player
    public float attackRange = 5f;           // Range within which enemy attacks player
    public float retreatHealthThreshold = 50f;  // Health threshold for retreating
    public float retreatDuration = 3f;       // Time to retreat before returning to another state
    public float randomAttackDelayMin = 0.5f; // Minimum delay between attacks
    public float randomAttackDelayMax = 1.5f; // Maximum delay between attacks

    [Header("Movement Vars")]
    public float moveSpeed = 5f;            // Speed at which enemy moves towards player
    public float rotateSpeed = 200f;        // Speed at which enemy rotates to face player
    public float retreatSpeed = 7f;         // Speed for retreating
    public Transform player;               // Player's transform
    private Rigidbody2D rb;

    [Header("Health Vars")]
    public float health = 100f;             // Enemy's current health

    private float attackCooldown = 0f;      // Cooldown between attacks
    private float retreatEndTime = 0f;      // Time when retreat ends
    private Vector3 patrolPoint;

    [Header("Fear Level Settings")]
    public float timidRetreatHealthThreshold = 70f;   // Health threshold for timid enemies to retreat
    public float aggressiveRetreatHealthThreshold = 20f; // Lower threshold for aggressive enemies to retreat
    public float timidAttackCooldownFactor = 1.5f;   // Timid enemies attack less often
    public float aggressiveAttackCooldownFactor = 0.7f; // Aggressive enemies attack more frequently
    
    [Header("Prefab References")]
    public GameObject projectilePrefab;  // Reference to the projectile prefab

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        AssignRandomFearLevel(); // Randomly assign fear level to this enemy

        patrolPoint = RandomPointOnScreen(new Vector2(-8,4), new Vector2(8,-4));
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Patrol();  // Random patrol behavior or idle behavior
                if (PlayerInRange())
                {
                    currentState = State.Chase;  // Switch to chase state if player is detected
                }
                break;

            case State.Chase:
                MoveTowardsPlayer();  // Follow player
                if (CanAttack()) 
                {
                    currentState = State.Attack;  // Switch to attack when in range
                }
                break;

            case State.Attack:
                
                MoveTowardsPlayer();
                
                if (Time.time >= attackCooldown)
                {
                    AttackPlayer();  // Shoot at player
                    attackCooldown = Time.time + Random.Range(randomAttackDelayMin, randomAttackDelayMax) * GetAttackCooldownFactor();  // Adjust delay based on fear level
                }

                if (ShouldRetreat())
                {
                    currentState = State.Retreat;  // Retreat if needed
                    retreatEndTime = Time.time + retreatDuration;
                }
                break;

            case State.Retreat:
                RetreatFromPlayer();  // Move away from player
                if (Time.time > retreatEndTime)
                {
                    patrolPoint = RandomPointOnScreen(new Vector2(-8,4), new Vector2(8,-4));
                    currentState = State.Idle;  // Return to idle after retreating
                }
                break;

            case State.Dead:
                Die();  // Handle death
                break;
        }
    }

    // ================================
    // State-specific behavior methods
    // ================================

    private void Patrol()
    {
        TurnTowardsPoint(patrolPoint, rotateSpeed);
        
        MoveForward();

        // Check if the enemy is close enough to the patrol point
        float distanceToPatrolPoint = Vector2.Distance(transform.position, patrolPoint);

        if (distanceToPatrolPoint <= 0.5f)
        {
            // Pick a new patrol point far enough away
            Vector3 newPatrolPoint;
            do
            {
                newPatrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));
            } while (Vector2.Distance(newPatrolPoint, patrolPoint) < 3f);

            patrolPoint = newPatrolPoint;
        }
    }

    private void MoveTowardsPlayer()
    {
        TurnTowardsPoint(player.position, rotateSpeed);
        
        MoveForward();
    }

    private void RetreatFromPlayer()
    {
        TurnTowardsPoint(player.position, -rotateSpeed);
        
        MoveForward();
    }

    private void AttackPlayer()
    {
        var projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        projectile.SetActive(true);
    }

    private void Die()
    {
        GameManager.instance.spawnExplosionEffect(transform.position);
        Destroy(gameObject);
    }

    // ================================
    // Helper methods for conditions
    // ================================

    private bool PlayerInRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }

    private bool CanAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    private bool ShouldRetreat()
    {
        // Logic for retreating based on fear level
        switch (fearLevel)
        {
            case FearLevel.Timid:
                return health < timidRetreatHealthThreshold;  // Timid enemies retreat sooner
            case FearLevel.Normal:
                return health < retreatHealthThreshold;  // Normal behavior
            case FearLevel.Aggressive:
                return health < aggressiveRetreatHealthThreshold;  // Aggressive enemies retreat later
        }
        return false;
    }

    // ================================
    // Random Fear Level Assignment
    // ================================

    private void AssignRandomFearLevel()
    {
        // Randomly assign a fear level on start
        int randomValue = Random.Range(0, 3);  // Random number between 0 and 2
        switch (randomValue)
        {
            case 0:
                fearLevel = FearLevel.Timid;
                break;
            case 1:
                fearLevel = FearLevel.Normal;
                break;
            case 2:
                fearLevel = FearLevel.Aggressive;
                break;
        }
    }

    private float GetAttackCooldownFactor()
    {
        // Adjust attack cooldowns based on fear level
        switch (fearLevel)
        {
            case FearLevel.Timid:
                return timidAttackCooldownFactor;  // Timid enemies attack less frequently
            case FearLevel.Normal:
                return 1f;  // Normal enemies use default cooldown
            case FearLevel.Aggressive:
                return aggressiveAttackCooldownFactor;  // Aggressive enemies attack more frequently
        }
        return 1f;
    }

    // ================================
    // Damage and health system
    // ================================

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            TakeDamage(20f);  // Reduce health when hit by a player projectile
            Destroy(other.gameObject);  // Destroy the projectile
        }
    }

    private void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            currentState = State.Dead;  // Transition to Dead state if health reaches zero
        }
    }
    
    // ================================
    // Other Helper Methods
    // ================================
    
    private Vector3 RandomPointOnScreen(Vector2 topLeft, Vector2 bottomRight)
    {
        float randomX = Random.Range(topLeft.x, bottomRight.x);
        float randomY = Random.Range(bottomRight.y, topLeft.y);
        return new Vector3(randomX, randomY, 0f);
    }
    
    private void MoveForward()
    {
        Vector2 baseSpeed = Vector2.right * moveSpeed;
        gameObject.transform.Translate(baseSpeed * Time.deltaTime);
    }

    //Takes a point and rotates the ship towards the point
    void TurnTowardsPoint(Vector2 point, float speed)
    {
        var ship = gameObject;
        var shipPosition = ship.transform.position;

        //Calculate angle to look at point
        var thetaOfPoint = Mathf.Atan2(point.y - shipPosition.y, point.x - shipPosition.x) * Mathf.Rad2Deg;
        
        //Store in a Euler Quaternion
        Quaternion newQuat = Quaternion.Euler(0.0f, 0.0f, thetaOfPoint);
        

        //Note that a negative speed turns ship away from point
        float singleStep = speed * Time.deltaTime;
        
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }
}