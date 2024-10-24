using System.Collections;
using UnityEngine;

public class MineLayerAI : MonoBehaviour
{
    private enum FearLevel
    {
        Timid,
        Normal,
        Aggressive
    }

    private FearLevel fearLevel;  // The fear level assigned to this enemy

    private enum State
    {
        Idle,
        Dead
    }

    private State currentState = State.Idle;

    public int score = 150;  // Score value for this enemy

    [Header("Movement Vars")]
    public float moveSpeed = 3f;            // Speed at which enemy moves while patrolling
    public float rotateSpeed = 150f;        // Speed at which enemy rotates
    private Rigidbody2D rb;
    private Vector3 patrolPoint;

    [Header("Health Vars")]
    public float health = 120f;             // Enemy's current health
    public float maxHealth = 120f;

    [Header("Mine Laying")]
    public GameObject minePrefab;          // Prefab for the mine
    public float mineTick = 5f;            // Time between laying mines (Normal FearLevel)
    private float nextMineTime = 0f;       // Time when next mine can be laid
    public float timidMineTickFactor = 1.5f; // Timid enemies lay mines less frequently
    public float aggressiveMineTickFactor = 0.7f; // Aggressive enemies lay mines more frequently

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        AssignRandomFearLevel(); // Randomly assign fear level to this enemy

        // Set the patrol point randomly within the screen bounds
        patrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));

        // Set the initial mine lay time based on fear level
        nextMineTime = Time.time + GetMineLayInterval();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Patrol();  // Enemy is patrolling around the screen
                if (Time.time >= nextMineTime)
                {
                    LayMine();  // Lay mine if the time has come
                    nextMineTime = Time.time + GetMineLayInterval();
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
            // Pick a new patrol point
            Vector3 newPatrolPoint;
            do
            {
                newPatrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));
            } while (Vector2.Distance(newPatrolPoint, patrolPoint) < 3f);

            patrolPoint = newPatrolPoint;
        }
    }

    private void LayMine()
    {
        var obj = Instantiate(minePrefab, transform.position, Quaternion.identity);
        obj.SetActive(true);
        score += 15;  // Increase score when laying a mine
    }

    private void Die()
    {
        GameManager.instance.spawnExplosionEffect(transform.position);
        GameManager.instance.addScore(score);
        GameManager.instance.activeEnemies.Remove(gameObject);
        GameManager.instance.OnEnemyKilled(0.1f);
        Destroy(gameObject);
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

    // ================================
    // Helper methods
    // ================================

    private float GetMineLayInterval()
    {
        // Adjust mine laying intervals based on fear level
        switch (fearLevel)
        {
            case FearLevel.Timid:
                return mineTick * timidMineTickFactor;
            case FearLevel.Aggressive:
                return mineTick * aggressiveMineTickFactor;
            case FearLevel.Normal:
            default:
                return mineTick;
        }
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

    void TurnTowardsPoint(Vector2 point, float speed)
    {
        var ship = gameObject;
        var shipPosition = ship.transform.position;

        var thetaOfPoint = Mathf.Atan2(point.y - shipPosition.y, point.x - shipPosition.x) * Mathf.Rad2Deg;

        Quaternion newQuat = Quaternion.Euler(0.0f, 0.0f, thetaOfPoint);

        float singleStep = speed * Time.deltaTime;

        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }
}

