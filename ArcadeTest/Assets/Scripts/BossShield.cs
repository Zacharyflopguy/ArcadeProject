using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShield : MonoBehaviour
{
    public float currentRotationSpeed = 25f;  // Speed at which the shield rotates
    public Transform enemyShip;  // The enemy ship this shield follows
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = enemyShip.position;
        transform.Rotate(Vector3.forward * (currentRotationSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerProjectile"))
        {
            Destroy(other.gameObject);  // Destroy the projectile
        }
    }
}
