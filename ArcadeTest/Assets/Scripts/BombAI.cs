using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BombAI : MonoBehaviour
{
    // Enemy states
    private enum State
    {
        Idle,
        Chase,
        Explode,
        Dead
    }

    private State currentState = State.Idle;  // Start in the Idle state
    private Vector3 patrolPoint;  // Random point for enemy to patrol

    [Header("State Transitions")]
    public float detectionRange = 10f;       // Range within which enemy detects player
    public float explosionRange = 1f;        // Range within which enemy explodes
    public float frenzyHealthThreshold = 30f;  // Health threshold to enter frenzy mode
    public float frenzySpeedMultiplier = 1.5f;  // Speed multiplier when in frenzy mode

    [Header("Movement Vars")]
    public float minMoveSpeed = 5f;             // Speed at which enemy moves towards player
    public float maxMoveSpeed = 10f;            // Speed at which enemy moves towards player
    public float rotateSpeed = 200f;         // Speed at which enemy rotates to face player
    public Transform player;                // Player's transform
    private Rigidbody2D rb;
    private bool isFrenzied = false;          // Tracks whether enemy is in frenzy mode
    private float moveSpeed;

    [Header("Health Vars")]
    public float health = 100f;              // Enemy's current health
    public float maxHealth = 100f;           // Enemy's maximum health

    [Header("Explosion Vars")]
    public float explosionDamage = 50f;      // Damage dealt to player when enemy explodes
    public float explosionForce = 10f;       // Force applied to player on explosion
    public float maxPushDistance = 5f;       // Maximum distance for applying push force
    public Rigidbody2D playerRb;             // Player's Rigidbody2D

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8,4), new Vector2(8,-4));
    }

    private void Update()
    {
        // Calculate the distance between the bomb and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If the player is within detection distance, adjust speed based on proximity
        if (distanceToPlayer <= detectionRange)
        {
            // Move faster the closer the bomb is to the player
            float speedFactor = 1f - Mathf.Clamp01(distanceToPlayer / detectionRange);
            moveSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, speedFactor);  // Move faster as distance decreases
        }
        else
        {
            // If the player is out of detection range, move at minimum speed
            moveSpeed = minMoveSpeed;
        }
        
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
                MoveTowardsPlayer();  // Chase player
                CheckHealthFrenzy();  // Check if the enemy should enter frenzy state
                if (PlayerInExplosionRange())
                {
                    currentState = State.Explode;  // Switch to explosion when close enough
                }
                break;

            case State.Explode:
                Explode();  // Trigger explosion
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
        
        MoveForward(moveSpeed);

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

        float speed = isFrenzied ? moveSpeed * frenzySpeedMultiplier : moveSpeed;
        MoveForward(speed);
    }

    private void Explode()
    {
        // Calculate the distance between the bomb and the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If the player is within the maximum push distance
        if (distanceToPlayer <= maxPushDistance)
        {
            // Calculate the damage as a proportion of the distance
            float damageFactor = 1f - Mathf.Clamp01(distanceToPlayer / maxPushDistance); // 1 at point of explosion, 0 at max distance
            int finalDamage = Mathf.RoundToInt(explosionDamage * damageFactor);

            // Deal damage to the player
            GameManager.instance.health -= finalDamage;

            // Apply knockback force to the player based on proximity
            Vector2 pushDirection = (player.position - transform.position).normalized;
            float pushForce = Mathf.Lerp(explosionForce, 0f, distanceToPlayer / maxPushDistance); // Strong push at close range, weak at far
            playerRb.AddForce(pushDirection * pushForce, ForceMode2D.Impulse);
        }

        currentState = State.Dead;  // Switch to dead state
    }

    private void Die()
    {
        GameManager.instance.spawnBigExplosionEffect(transform.position);
        GameManager.instance.activeEnemies.Remove(gameObject);
        Destroy(gameObject);
    }

    // ================================
    // Helper methods for AI conditions
    // ================================

    private bool PlayerInRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }

    private bool PlayerInExplosionRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= explosionRange;
    }

    private void CheckHealthFrenzy()
    {
        if (!isFrenzied && health < frenzyHealthThreshold)
        {
            isFrenzied = true;  // Enter frenzy state when health is low
        }
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
            GameManager.instance.addScore(250);
            currentState = State.Explode;  // Explode when health reaches zero
        }
    }

    // ================================
    // Other Helper Methods
    // ================================

    private void MoveForward(float speed)
    {
        Vector2 baseSpeed = Vector2.right * speed;
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

        // Note that a negative speed turns ship away from point
        float singleStep = speed * Time.deltaTime;

        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }
    
    private Vector3 RandomPointOnScreen(Vector2 topLeft, Vector2 bottomRight)
    {
        float randomX = Random.Range(topLeft.x, bottomRight.x);
        float randomY = Random.Range(bottomRight.y, topLeft.y);
        return new Vector3(randomX, randomY, 0f);
    }
}

