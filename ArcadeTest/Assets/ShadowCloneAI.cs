using System.Collections;
using UnityEngine;

public class ShadowCloneAI : MonoBehaviour
{
    private enum State
    {
        Patrol,
        Attack,
        Dead
    }

    private State currentState = State.Patrol;

    public int score = 0;

    [Header("State Transitions")] 
    public float detectionRange = 10f;
    public float attackRange = 8f;

    public float dieTime = 4f;

    [Header("Movement Vars")] public float moveSpeed = 5f;
    public float rotateSpeed = 200f;
    public Transform player;
    private Rigidbody2D rb;

    [Header("Health Vars")] public float health = 100f;
    public float maxHealth = 100f;

    private float attackCooldown = 0f;
    private Vector3 patrolPoint;

    [Header("Prefab References")] 
    public GameObject projectilePrefab;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        patrolPoint = RandomPointOnScreen(new Vector2(-8, 4), new Vector2(8, -4));

        StartCoroutine(FadeIn());
        StartCoroutine(DieAfterTime(dieTime));
    }

    private void Update()
    {
        
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                if (PlayerInRange())
                {
                    currentState = State.Attack;
                }

                break;

            case State.Attack:
                MoveTowardsPlayer();
                if (Time.time >= attackCooldown && PlayerInAttackRange())
                {
                    AttackPlayer();
                    attackCooldown = Time.time + Random.Range(0.5f, 1.5f);
                }

                break;

            case State.Dead:
                Die();
                break;
        }

        if (!GameManager.instance.isBoss)
        {
            currentState = State.Dead;
        }
    }

    // ================================
    // Fading Behavior
    // ================================

    private IEnumerator FadeOut()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        for (float alpha = 1f; alpha >= 0; alpha -= 0.1f)
        {
            color.a = alpha;
            spriteRenderer.color = color;
            yield return new WaitForSeconds(0.05f);
        }
        // Ensure that the alpha is exactly 0 after fading in
        color.a = 0f;
        spriteRenderer.color = color;
    }

    private IEnumerator FadeIn()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        for (float alpha = 0; alpha <= 1f; alpha += 0.1f)
        {
            rb.velocity = Vector2.zero;
            color.a = alpha;
            spriteRenderer.color = color;
            yield return new WaitForSeconds(0.05f);
        }
        // Ensure that the alpha is exactly 1 after fading in
        color.a = 1f;
        spriteRenderer.color = color;
    }

    // ================================
    // State-specific behavior methods
    // ================================

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
        var projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        projectile.SetActive(true);
    }

    private void Die()
    {
        // Handle death and clean up
        GameManager.instance.spawnBigExplosionEffect(transform.position);
        GameManager.instance.activeEnemies.Remove(gameObject);
        Destroy(gameObject);
    }

    // ================================
    // Helper methods
    // ================================

    private bool PlayerInRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
    }
    
    private bool PlayerInAttackRange()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
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

        // Calculate angle to look at point
        var thetaOfPoint = Mathf.Atan2(point.y - shipPosition.y, point.x - shipPosition.x) * Mathf.Rad2Deg;

        // Store in a Euler Quaternion
        Quaternion newQuat = Quaternion.Euler(0.0f, 0.0f, thetaOfPoint);

        // Note that a negative speed turns ship away from point
        float singleStep = speed * Time.deltaTime;

        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            TakeDamage(20f);
            Destroy(other.gameObject);
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
    
    private IEnumerator DieAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(FadeOut());
        yield return new WaitForSeconds(0.5f);
        currentState = State.Dead;
    }
}
   


