using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAI : MonoBehaviour
{
    public int score = 100; // Score value for this enemy

    [Header("Healing Variables")]
    public float healAmount = 10f; // Amount of health to heal
    public float healTick = 1f; // Time between heals
    public float healRange = 3f; // Radius within which to heal other enemies
    public float selfHealTick = 2f; // Time between self heals
    private float nextHealTime = 0f; // Next time the enemy can heal
    private float nextSelfHealTime = 0f; // Next time the enemy can heal itself
    public CircleCollider2D healCollider; // Collider for healing range

    [Header("Movement Vars")]
    public float moveSpeed = 5f; // Speed at which enemy moves towards another enemy
    public Transform player; // Reference to the player (not used for this enemy)

    [Header("Health Vars")]
    public float health = 100f; // Enemy's current health
    public float maxHealth = 100f; // Enemy's maximum health
    
    private Vector3 patrolPoint;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8,4), new Vector2(8,-4));
    }

    private void Update()
    {
        GameObject targetEnemy = FindClosestEnemy();
        
        if (targetEnemy != null)
        {
            MoveTowardsEnemy(targetEnemy, 1.5f); // Move towards the enemy

            if (Time.time >= nextHealTime)
            {
                HealEnemies();
                nextHealTime = Time.time + healTick; // Set the next heal time
            }
            if (Time.time >= nextSelfHealTime)
            {
                if (health < maxHealth)
                {
                    health += healAmount; // Heal itself
                    GameManager.instance.spawnHealEffect(transform);
                }
                
                if (health > maxHealth)
                {
                    health = maxHealth;
                }
                nextSelfHealTime = Time.time + selfHealTick; // Set the next self-heal time
            }
        }
        else
        {
            Patrol();
        }
    }

    private GameObject FindClosestEnemy()
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in GameManager.instance.activeEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance && distance <= healRange)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
    
    private void MoveTowardsEnemy(GameObject enemy, float desiredDistance)
    {
        // Calculate the direction from the enemy to this object
        Vector2 direction = (enemy.transform.position - transform.position).normalized;

        // Calculate the desired position based on the enemy's position and the desired distance
        Vector2 targetPosition = (Vector2)enemy.transform.position - direction * desiredDistance;

        // Move towards the target position
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Turn to face the enemy
        TurnTowardsPoint(enemy.transform.position, 150f); // Adjust rotation speed as needed
    }

    private void HealEnemies()
    {
        // Create a list to hold the colliders that overlap with the SphereCollider2D
        List<Collider2D> overlappingColliders = new List<Collider2D>();

        // Create a ContactFilter2D for the overlap query
        ContactFilter2D contactFilter = new ContactFilter2D
        {
            useTriggers = true // Include trigger colliders
        };

        // Perform the overlap check
        Physics2D.OverlapCollider(healCollider, contactFilter, overlappingColliders);

        // Loop through each collider found in the overlapping area
        foreach (var collider in overlappingColliders)
        {
            if (collider.gameObject == gameObject) continue; 
            
            if (!collider.CompareTag("Enemy")) continue;
            
            if(collider.TryGetComponent(out EnemyAI enemyAI))
            {
                if (enemyAI.health < enemyAI.maxHealth)
                {
                    enemyAI.health += healAmount; //Heal the enemy
                    GameManager.instance.spawnHealEffect(enemyAI.transform);
                }
                
                if (enemyAI.health > enemyAI.maxHealth)
                {
                    enemyAI.health = enemyAI.maxHealth;
                }
            }
            else if(collider.TryGetComponent(out DoubleAI doubleAI))
            {
                if (doubleAI.health < doubleAI.maxHealth)
                {
                    doubleAI.health += healAmount; //Heal the enemy
                    GameManager.instance.spawnHealEffect(doubleAI.transform);
                }
                
                if (doubleAI.health > doubleAI.maxHealth)
                {
                    doubleAI.health = doubleAI.maxHealth;
                }
            }
            else if(collider.TryGetComponent(out BombAI bombAI))
            {
                if (bombAI.health < bombAI.maxHealth)
                {
                    bombAI.health += healAmount; //Heal the enemy
                    GameManager.instance.spawnHealEffect(bombAI.transform);
                }
                
                bombAI.health += healAmount; // Heal the enemy
                if (bombAI.health > bombAI.maxHealth)
                {
                    bombAI.health = bombAI.maxHealth;
                }
            }
        }
    }
    

    private void Die()
    {
        GameManager.instance.spawnExplosionEffect(transform.position);
        GameManager.instance.addScore(score);
        Destroy(gameObject);
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
    
    private void Patrol()
    {
        TurnTowardsPoint(patrolPoint, 150f);
        
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
    
    private void MoveForward()
    {
        Vector2 baseSpeed = Vector2.right * moveSpeed;
        gameObject.transform.Translate(baseSpeed * Time.deltaTime);
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
            Die();
        }
    }
}

