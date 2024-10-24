using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    
    public float projectileSpeed = 10f;  // Speed of the projectile
    
    public int damage = 10; 
    
    // Start is called before the first frame update
    void Start()
    {
        // Destroy the projectile after 10 seconds
        Destroy(gameObject, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        //Move the projectile forward
        transform.Translate(Vector3.right * (projectileSpeed * Time.deltaTime));
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the projectile collides with an enemy
        if (collision.CompareTag("Barrier"))
        {
            // Destroy the projectile
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            GameManager.instance.health -= damage;
            GameManager.instance.OnPlayerDamage();

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}