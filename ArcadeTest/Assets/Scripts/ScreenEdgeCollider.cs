using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEdgeCollider : MonoBehaviour
{
    [Tooltip("The direction the player should be pushed when colliding with this edge. Use normalized vectors like (1, 0), (0, 1), (-1, 0), (0, -1) for cardinal directions.")]
    public Vector2 pushDirection = Vector2.zero;  // Specify the push direction (e.g., up, down, left, right)

    public float maxPushBackForce = 20f;  // Maximum force that can be applied to push the player back
    public float forceMultiplier = 2f;    // Multiplier to control how much force is applied based on the player's speed

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PushPlayerOut(collision);
        
        if(collision.CompareTag("PlayerProjectile"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Continuously push the player out if they stay inside the barrier
        PushPlayerOut(collision);
    }

    // Method to apply the pushback force
    private void PushPlayerOut(Collider2D collision)
    {
        // Check if the colliding object has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                // Get the player's speed
                float playerSpeed = playerRb.velocity.magnitude;

                // Calculate the force to apply based on the player's speed
                float pushBackForce = Mathf.Min(playerSpeed * forceMultiplier, maxPushBackForce);

                // Ensure the push direction is normalized
                if (pushDirection == Vector2.zero)
                {
                    Debug.LogWarning("Push direction is not set on the edge collider! Please set it in the Inspector.");
                    return;
                }

                pushDirection.Normalize();

                // Apply the force in the specified push direction
                playerRb.AddForce(pushDirection * pushBackForce, ForceMode2D.Impulse);
            }
        }
    }
}