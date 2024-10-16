using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChargeBossAI : MonoBehaviour
{
    // Enemy states
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Charge,
        Dead
    }

    private State currentState = State.Idle;  // Start in the Idle state

    public int score = 100;  // Score value for this enemy

    [Header("State Transitions")]
    public float detectionRange = 10f;       // Range within which enemy detects player
    public float attackRange = 5f;           // Range within which enemy attacks player
    public float chargeCooldown = 5f;        // Cooldown between charges
    public float chargeSpeed = 10f;          // Speed of the charge attack
    public float chargeLookTime = 1f;        // Time to "look" at the player before charging
    public float healthThresholdStage2 = 50f; // Health threshold for Stage 2
    public Animator animator;

    [Header("Movement Vars")]
    public float moveSpeed = 5f;             // Speed at which enemy moves towards player
    public float rotateSpeed = 200f;         // Speed at which enemy rotates to face player
    public Transform player;                 // Player's transform
    private Rigidbody2D rb;

    [Header("Health Vars")]
    public float health = 100f;              // Enemy's current health
    private bool inStage2 = false;           // Tracks if the boss is in Stage 2

    [Header("Attack Vars")]
    public GameObject projectilePrefab;      // Projectile for Stage 1
    public GameObject stage2ProjectilePrefab; // Projectile for Stage 2
    public Transform firePoint1;             // First fire point for barrage
    public Transform firePoint2;             // Second fire point for barrage
    public int dashDamage = 20;              // Damage dealt to player when dashing
    public float fireRate = 1f;              // Rate of fire in seconds
    private float nextFireTime = 0f;         // When to fire next

    private Vector3 patrolPoint;
    private bool isCharging = false;
    private float chargeStartTime;
    private float nextChargeTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));
        nextChargeTime = Time.time + chargeCooldown;
    }

    private void Update()
    {
        if (health <= 0)
        {
            currentState = State.Dead;
        }

        if (health < healthThresholdStage2 && !inStage2)
        {
            EnterStage2();
        }

        switch (currentState)
        {
            case State.Idle:
                Patrol();
                if (PlayerInRange())
                {
                    currentState = State.Chase;
                }
                break;

            case State.Chase:
                MoveTowardsPlayer();
                if (CanAttack())
                {
                    currentState = State.Attack;
                }
                else if (Time.time >= nextChargeTime)
                {
                    StartCharging();
                }
                break;

            case State.Attack:
                AttackPlayer();
                if (Time.time >= nextChargeTime)
                {
                    StartCharging();
                }
                break;

            case State.Charge:
                ChargeTowardsPlayer();
                break;

            case State.Dead:
                Die();
                break;
        }
    }

    private void Patrol()
    {
        TurnTowardsPoint(patrolPoint, rotateSpeed);
        MoveForward();

        float distanceToPatrolPoint = Vector2.Distance(transform.position, patrolPoint);
        if (distanceToPatrolPoint <= 0.5f)
        {
            patrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));
        }
    }

    private void MoveTowardsPlayer()
    {
        TurnTowardsPoint(player.position, rotateSpeed);
        MoveForward();
    }

    private void AttackPlayer()
    {
        TurnTowardsPoint(player.position, rotateSpeed);
        
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        GameObject projectile = Instantiate(inStage2 ? stage2ProjectilePrefab : projectilePrefab, firePoint1.position, transform.rotation);
        projectile.SetActive(true);
        GameObject projectile2 = Instantiate(inStage2 ? stage2ProjectilePrefab : projectilePrefab, firePoint2.position, transform.rotation);
        projectile2.SetActive(true);
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentState = State.Charge;
        rb.velocity = Vector2.zero;  // Stop moving before charge
        animator.SetBool("isCharging", true);
    }

    private void ChargeTowardsPlayer()
    {
        if (Time.time - chargeStartTime < chargeLookTime)
        {
            // Look at the player but stay still for a moment
            TurnTowardsPoint(player.position, rotateSpeed);
        }
        else
        {
            // Dash toward the player
            rb.velocity = transform.right * (float)(inStage2 ? chargeSpeed * 1.5 : chargeSpeed);
            if (Vector2.Distance(transform.position, player.position) < 1f)
            {
                // Knockback the player (you can customize knockback)
                player.GetComponent<Rigidbody2D>().AddForce(-transform.right * 750);
            }
            isCharging = false;
            nextChargeTime = Time.time + (inStage2 ? chargeCooldown / 2 : chargeCooldown);  // More frequent charge in Stage 2
            currentState = State.Chase;
            animator.SetBool("isCharging", false);
        }
    }

    private void EnterStage2()
    {
        inStage2 = true;
        fireRate /= 2;  // Fire faster in Stage 2
    }

    private void Die()
    {
        GameManager.instance.spawnBigExplosionEffect(transform.position);
        GameManager.instance.addScore(score);
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

    private bool CanAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
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

    private void TurnTowardsPoint(Vector2 point, float speed)
    {
        var thetaOfPoint = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion newQuat = Quaternion.Euler(0.0f, 0.0f, thetaOfPoint);
        float singleStep = speed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newQuat, singleStep);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            TakeDamage(20f);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Player"))
        {
            GameManager.instance.health -= dashDamage;
        }
    }

    private void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            currentState = State.Dead;
        }
    }
}
