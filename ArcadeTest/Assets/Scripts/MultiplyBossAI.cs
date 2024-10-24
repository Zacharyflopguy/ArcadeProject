using System.Collections;
using UnityEngine;

public class MultiplyBossAI : MonoBehaviour
{
    // Boss states
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    private State currentState = State.Idle;  // Start in the Idle state

    [Header("State Transitions")]
    public float detectionRange = 10f;       // Range within which boss detects player
    public float attackRange = 5f;           // Range within which boss attacks player
    public float randomAttackDelayMin = 0.5f; // Minimum delay between attacks
    public float randomAttackDelayMax = 1.5f; // Maximum delay between attacks

    [Header("Movement Vars")]
    public float moveSpeed = 5f;            // Speed at which boss moves towards player
    public float rotateSpeed = 200f;        // Speed at which boss rotates to face player
    public Transform player;                // Player's transform
    private Rigidbody2D rb;

    [Header("Health Vars")]
    public float health = 100f;             // Boss's current health
    public int stage = 1;                   // The current stage of the boss (1: big, 2: medium, 3: small)

    [Header("Splitting Vars")]
    public GameObject nextBossPrefab;       // The prefab for the next stage of the boss
    public int numSplits = 2;               // Number of splits when this boss dies

    [Header("Projectile Vars")]
    public GameObject projectilePrefab;     // Reference to the projectile prefab
    public Transform firePoint1;
    public Transform firePoint2;

    private float attackCooldown = 0f;      // Cooldown between attacks
    private Vector3 patrolPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));
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
                    attackCooldown = Time.time + Random.Range(randomAttackDelayMin, randomAttackDelayMax);
                }
                break;

            case State.Dead:
                SplitOrDie();  // Handle splitting or dying based on the stage
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

    private void AttackPlayer()
    {
        StartCoroutine(FireAtPlayer());
    }

    private void SplitOrDie()
    {
        if (stage < 3) // If not the final stage, split into smaller bosses
        {
            var angle = Random.insideUnitCircle.normalized;
            
            for (int i = 0; i < numSplits; i++)
            {
                GameObject newBoss = Instantiate(nextBossPrefab, transform.position, Quaternion.identity);
                newBoss.SetActive(true);
                Rigidbody2D rbNewBoss = newBoss.GetComponent<Rigidbody2D>();
                // Apply force to push them outward from the current boss's explosion
                if (i == 0)
                {
                    rbNewBoss.AddForce(angle * 2f, ForceMode2D.Impulse);  
                }
                else
                {
                    rbNewBoss.AddForce(-angle * 2f, ForceMode2D.Impulse);
                }
            }
        }

        if (stage == 1)
        {
            GameManager.instance.spawnBigExplosionEffect(transform.position); // Spawn explosion effect
        }
        else
        {
            GameManager.instance.spawnExplosionEffect(transform.position); // Spawn explosion effect
        }

        switch (stage)
        {
            case 1: GameManager.instance.addScore(1000); break;
            case 2: GameManager.instance.addScore(500); break;
            case 3: GameManager.instance.addScore(250); break;
        }
        
        CheckIfLastBossAlive();
        
        Destroy(gameObject);  // Destroy the current boss
    }

    // ================================
    // Helper methods for AI conditions
    // ================================
    
    // Function to check if this is the last boss alive
    private void CheckIfLastBossAlive()
    {
        // Find all active bosses in the game
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        // Check if there are no other bosses left (except this one)
        if (bosses.Length == 1)  // If only one boss (this one) is alive
        {
            GameManager.instance.isBoss = false;
            GameManager.instance.OnEnemyKilled(1);
        }
    }

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

    private void MoveForward()
    {
        Vector2 baseSpeed = Vector2.right * moveSpeed;
        gameObject.transform.Translate(baseSpeed * Time.deltaTime);
    }

    private void TurnTowardsPoint(Vector2 point, float speed)
    {
        var ship = gameObject;
        var shipPosition = ship.transform.position;

        // Calculate angle to look at point
        var thetaOfPoint = Mathf.Atan2(point.y - shipPosition.y, point.x - shipPosition.x) * Mathf.Rad2Deg;
        
        // Store in a Euler Quaternion
        Quaternion newQuat = Quaternion.Euler(0.0f, 0.0f, thetaOfPoint);
        
        // Rotate ship towards point
        float singleStep = speed * Time.deltaTime;
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }

    private IEnumerator FireAtPlayer()
    {
        var obj1 = Instantiate(projectilePrefab, firePoint1.position, transform.rotation);
        obj1.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        var obj2 = Instantiate(projectilePrefab, firePoint2.position, transform.rotation);
        obj2.SetActive(true);
    }
    
    private Vector3 RandomPointOnScreen(Vector2 topLeft, Vector2 bottomRight)
    {
        float randomX = Random.Range(topLeft.x, bottomRight.x);
        float randomY = Random.Range(bottomRight.y, topLeft.y);
        return new Vector3(randomX, randomY, 0f);
    }
}
