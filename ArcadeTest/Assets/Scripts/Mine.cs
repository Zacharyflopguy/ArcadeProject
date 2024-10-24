using System.Collections;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public float blastRadius = 5f;       // Radius of the explosion
    public float detonationDelay = 0.1f; // Optional slight delay before detonation
    public float explosionForce = 500f;  // Force to push the player back
    public int damage = 20;              // Damage dealt to the player

    private bool detonated = false;      // To prevent multiple detonations

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the mine should detonate (Player or PlayerProjectile trigger)
        if (other.CompareTag("Player") || other.CompareTag("PlayerProjectile"))
        {
            if (!detonated)
            {
                StartCoroutine(Detonate(other));
            }
            
            if(other.CompareTag("PlayerProjectile"))
            {
                Destroy(other.gameObject);
            }
        }
    }

    private IEnumerator Detonate(Collider2D trigger)
    {
        detonated = true;

        // Optionally, wait a small delay before detonation
        yield return new WaitForSeconds(detonationDelay);

        GameManager.instance.spawnBigExplosionEffect(transform.position);

        // Check for player within blast radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Deal damage to the player
                GameManager.instance.health -= damage;
                GameManager.instance.OnPlayerDamage();

                // Apply knockback force
                Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    Vector2 direction = hit.transform.position - transform.position;
                    playerRb.AddForce(direction.normalized * explosionForce);
                }
            }
        }

        // Destroy the mine after the explosion
        Destroy(gameObject);
    }

    // Optional: Visualize the blast radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
