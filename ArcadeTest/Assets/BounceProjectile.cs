using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceProjectile : MonoBehaviour
{
    public float projectileSpeed = 10f;  // Speed of the projectile
    public int damage = 10; 
    public int numBounces = 3;  // Number of times the projectile can bounce
    private Vector2 currentDirection;  // Stores the direction the projectile is moving

    private void Start()
    {
        // Initialize the direction the projectile should move based on the object's current facing direction
        currentDirection = transform.right;

        // Destroy the projectile after 20 seconds
        Destroy(gameObject, 20f);
    }

    private void Update()
    {
        // Move the projectile in the current direction
        transform.Translate(currentDirection * (projectileSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Barrier"))
        {
            if (numBounces > 0)
            {
                // Get the collision normal from the barrier
                Vector2 collisionNormal = collision.GetComponent<Collider2D>().bounds.center - transform.position;

                // Reflect the current direction based on the normal of the barrier
                currentDirection = Vector2.Reflect(currentDirection, collisionNormal.normalized);

                // Decrease the number of bounces left
                numBounces--;
            }
            else
            {
                // If no bounces left, destroy the projectile
                Destroy(gameObject);
            }
        }
        else if (collision.CompareTag("Player"))
        {
            // Damage the player
            GameManager.instance.health -= damage;

            // Destroy the projectile on impact with the player
            Destroy(gameObject);
        }
    }
}