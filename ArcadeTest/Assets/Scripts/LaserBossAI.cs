using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossAI : MonoBehaviour
{
    // Enemy states
    private enum State
    {
        Idle,
        Chase,
        Attack,
        Stage2Spin,
        Stage3Shield,
        Stage4Spin,
        Dead
    }

    private State currentState = State.Idle;  // Start in Idle state

    [Header("State Transitions")]
    public float detectionRange = 10f;       // Range within which boss detects player
    public float attackRange = 5f;           // Range within which boss attacks player
    public float stage2HealthThreshold = 50f; // Health threshold for Stage 2
    public float stage3HealthThreshold = 30f; // Health threshold for Stage 3
    public float stage4HealthThreshold = 25f; // Health threshold for Stage 4
    public float spinDuration = 2f;           // Time spent spinning during Stage 2 and Stage 4
    public float spinSpeed = 100f;            // Speed of rotation in Stage 2 and 4
    public Animator animator;
    
    [Header("Movement Vars")]
    public float moveSpeed = 5f;            // Speed at which boss moves towards player
    public float rotateSpeed = 200f;        // Speed at which boss rotates to face player
    public Transform player;               // Player's transform
    private Rigidbody2D rb;

    [Header("Health Vars")]
    public float health = 100f;             // Boss's current health

    [Header("Attack Vars")]
    public GameObject projectilePrefab;     // Reference to the projectile prefab
    public Transform firePoint1;
    public Transform firePoint2;
    public Transform firePoint3;
    public Transform firePoint4;
    public float minAttackCooldown = 0.5f;  // Minimum delay between attacks
    public float maxAttackCooldown = 1.5f;  // Maximum delay between attacks

    [Header("Laser Vars")]
    public GameObject laserPrefab;          // Reference to the laser prefab
    public GameObject shieldPrefab;         // Reference to the shield prefab
    public Transform[] laserSpawnPoints;    // Array of potential laser spawn points

    private float attackCooldown = 0f;
    private float spinEndTime = 0f;
    private GameObject activeShield;
    private Vector3 patrolPoint;
    private IEnumerator spinLaserCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8,4), new Vector2(8,-4));
        spinLaserCoroutine = DelaySpinLaser();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Idle:
                Patrol();  // Random patrol behavior
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
                HandleAttack();
                CheckForStageTransition();
                break;

            case State.Stage2Spin:
                HandleStage2Spin();
                CheckForStageTransition();
                break;

            case State.Stage3Shield:
                HandleStage3Shield();
                CheckForStageTransition();
                break;

            case State.Stage4Spin:
                HandleStage4Spin();
                break;

            case State.Dead:
                Die();  // Handle death
                break;
        }
    }

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

    private void HandleAttack()
    {
        MoveTowardsPlayer();
        if (Time.time >= attackCooldown)
        {
            FireProjectiles();  // Fire projectiles from all fire points
            attackCooldown = Time.time + Random.Range(minAttackCooldown, maxAttackCooldown);  // Random delay between attacks
        }
    }

    private void FireProjectiles()
    {
        var obj = Instantiate(projectilePrefab, firePoint1.position, firePoint1.rotation);
        obj.SetActive(true);
        var obj1 = Instantiate(projectilePrefab, firePoint2.position, firePoint2.rotation);
        obj1.SetActive(true);
        var obj2 = Instantiate(projectilePrefab, firePoint3.position, firePoint3.rotation);
        obj2.SetActive(true);
        var obj3 = Instantiate(projectilePrefab, firePoint4.position, firePoint4.rotation);
        obj3.SetActive(true);
    }

    private void CheckForStageTransition()
    {
        if (health <= stage2HealthThreshold && currentState == State.Attack)
        {
            currentState = State.Stage2Spin;
            StartCoroutine(DelaySpinLaser());
        }
        else if (health <= stage3HealthThreshold && currentState == State.Stage2Spin)
        {
            currentState = State.Stage3Shield;
            StartCoroutine(SpawnLasers());
            //shieldPrefab.GetComponent<BossShield>().enemyShip = transform;
            //shieldPrefab.SetActive(true);
        }
        else if (health <= stage4HealthThreshold && currentState == State.Stage3Shield)
        {
            //shieldPrefab.SetActive(false);
            currentState = State.Stage4Spin;
            StartCoroutine(DelaySpinLaser2());
        }
    }

    private void HandleStage2Spin()
    {
        StopMoving();
    }

    private void HandleStage3Shield()
    {
        HandleAttack();  // Attack as usual
    }

    private void HandleStage4Spin()
    {
        StopMoving();
        if (Time.time < spinEndTime)
        {
            StartCoroutine(spinLaserCoroutine);
        }
        else
        {
            spinEndTime = Time.time + spinDuration;
            var obj = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            obj.SetActive(true);
            obj.GetComponent<BossLaser>().enemyShip = transform;
            
            var obj2 = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            obj2.SetActive(true);
            obj2.GetComponent<BossLaser>().enemyShip = transform;
            obj2.GetComponent<BossLaser>().rotationOffset = 90f;
        }
    }

    private void StopMoving()
    {
        rb.velocity = Vector2.zero;  // Stop movement during spinning phase
    }

    private void SpinLaser(int direction)
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime * direction);  // Spin the boss
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

    private void Die()
    {
        // Handle boss death
        GameManager.instance.spawnBigExplosionEffect(transform.position);
        GameManager.instance.isBoss = false;
        GameManager.instance.addScore(4500);
        Destroy(gameObject);
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            health -= 20f;  // Reduce health when hit by a player projectile
            
            Destroy(other.gameObject);  // Destroy the projectile

            if (health <= 0)
            {
                currentState = State.Dead;  // Transition to Dead state if health reaches zero
            }
        }
    }

    private IEnumerator SpawnLasers()
    {
        while (currentState == State.Stage3Shield)
        {
            yield return new WaitForSeconds(2f);

            //Randomly spawn a laser at one of the spawn points
            int randomIndex = Random.Range(0, laserSpawnPoints.Length);
            var obj = Instantiate(laserPrefab, laserSpawnPoints[randomIndex].position, Quaternion.identity);
            obj.GetComponent<BossLaser>().enemyShip = laserSpawnPoints[randomIndex];
            obj.GetComponent<BossLaser>().lifetime = 1f;
            obj.SetActive(true);
        }
    }

    private IEnumerator DelaySpinLaser()
    {
        while (currentState == State.Stage2Spin)
        {
            var obj = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            obj.SetActive(true);
            obj.GetComponent<BossLaser>().enemyShip = transform;
            
            animator.SetBool("isCharging", true);
            yield return new WaitForSeconds(1f);
            animator.SetBool("isCharging", false);
        
            var spinTime = Time.time + spinDuration;
            var direction = Random.Range(0, 2) == 0 ? 1 : -1;
            while (Time.time < spinTime)
            {
                if (currentState != State.Stage2Spin)
                {
                    if(obj != null)
                        obj.GetComponent<BossLaser>().lifetime = 0f;
                }
                SpinLaser(direction);
                yield return null;
            }
        
            yield return new WaitForSeconds(Random.Range(2f, 3.5f));
            spinSpeed += 2f;
        }
    }
    
    private IEnumerator DelaySpinLaser2()
    {
        while (currentState == State.Stage4Spin)
        {
            var obj = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            obj.SetActive(true);
            obj.GetComponent<BossLaser>().enemyShip = transform;
            
            var obj2 = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            obj2.SetActive(true);
            obj2.GetComponent<BossLaser>().enemyShip = transform;
            obj2.GetComponent<BossLaser>().rotationOffset = 90;
            
            animator.SetBool("isCharging", true);
            yield return new WaitForSeconds(1f);
            animator.SetBool("isCharging", false);
        
            var spinTime = Time.time + spinDuration;
            var direction = Random.Range(0, 2) == 0 ? 1 : -1;
            while (Time.time < spinTime)
            {
                if (currentState != State.Stage4Spin)
                {
                    obj.GetComponent<BossLaser>().lifetime = 0f;
                    obj2.GetComponent<BossLaser>().lifetime = 0f;
                }
                SpinLaser(direction);
                yield return null;
            }
        
            yield return new WaitForSeconds(Random.Range(1.1f, 2f));
            spinSpeed += 2f;
        }
    }
}
