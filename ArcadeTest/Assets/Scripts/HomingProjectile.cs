using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    public float projectileSpeed = 8f;
    public float rotationSpeed = 100f;
    public int damage = 10;
    public float knockBackForce = 10f;
    public Transform playerPos;
    public Rigidbody2D playerRb;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate the projectile towards the player
        TurnTowardsPoint(playerPos.position, rotationSpeed);
        
        //Move the projectile forward
        transform.Translate(Vector3.right * (projectileSpeed * Time.deltaTime));
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier"))
        {
            // Destroy the projectile if it hits a barrier
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            // Deal damage to the player
            GameManager.instance.health -= damage;
            GameManager.instance.OnPlayerDamage();

            // Knock the player back
            Vector2 knockBackDirection = (playerRb.position - new Vector2(transform.position.x, transform.position.y)).normalized;   // Direction of knockback
            playerRb.AddForce(knockBackDirection * knockBackForce, ForceMode2D.Impulse);  // Apply knockback force
            
            //Explosion effect
            GameManager.instance.spawnExplosionEffect(transform.position);

            // Destroy the projectile (blow up)
            Destroy(gameObject);
        }
    }
    
    // Function to rotate the projectile towards a given point
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

        // Rotate the object towards the calculated angle
        gameObject.transform.rotation = Quaternion.RotateTowards(gameObject.transform.rotation, newQuat, singleStep);
    }
}
