using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    
    public float projectileSpeed = 10f;  // Speed of the projectile
    
    // Start is called before the first frame update
    void Start()
    {
        // Destroy the projectile after 5 seconds
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        //Move the projectile forward
        transform.Translate(Vector3.up * (projectileSpeed * Time.deltaTime));
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the projectile collides with an enemy
        if (collision.CompareTag("Barrier"))
        {
            // Destroy the projectile
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            // Destroy the enemy
            Destroy(collision.gameObject);

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}
