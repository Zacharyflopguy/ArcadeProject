using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaser : MonoBehaviour
{
    public Transform enemyShip;  // The enemy ship this laser follows
    public float rotationOffset = 0f;
    public float fadeInOutTime = 1f;  // Time for fade in and fade out
    public float lifetime = 5f;  // Total lifetime of the laser
    public int damage = 10;  // Damage dealt to the player
    public SpriteRenderer laserRenderer;  // Sprite Renderer for the laser's visual
    private float elapsedTime = 0f;  // Timer to track the laser's lifespan
    private bool dealsDamage = false;  // Whether the laser is currently active for damage
    public float damageCooldown = 0.5f;  // Time in seconds before laser can deal damage again
    private float lastDamageTime = 0f;  // Tracks the last time damage was dealt

    // Start is called before the first frame update
    void Start()
    {
        // Start with the laser fully transparent
        Color laserColor = laserRenderer.color;
        laserColor.a = 0f;
        laserRenderer.color = laserColor;

        // Start the fade in, rotate, and fade out sequence
        StartCoroutine(FadeAndRotate());
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure the laser follows the enemy ship's position
        if (enemyShip is null)
        {
            Destroy(gameObject);
        }
        else
        {
            if(GameManager.instance.isBoss == false)
            {
                Destroy(gameObject);
            }
            transform.position = enemyShip.position;
            transform.rotation =  Quaternion.Euler(0f, 0f, enemyShip.eulerAngles.z + rotationOffset);
        }
    }

    // Coroutine to handle fade in, rotation, and fade out
    private IEnumerator FadeAndRotate()
    {
        // Fade in phase
        float fadeInTime = 0f;
        while (fadeInTime < fadeInOutTime)
        {
            fadeInTime += Time.deltaTime;
            Color color = laserRenderer.color;
            color.a = Mathf.Lerp(0f, 1f, fadeInTime / fadeInOutTime);
            laserRenderer.color = color;
            yield return null;
        }

        // Start the rotation and enable damage
        dealsDamage = true;

        // Rotate and increase speed over time
        float rotationTime = 0f;
        while (rotationTime < lifetime - fadeInOutTime)
        {
            rotationTime += Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable damage and begin fade out
        dealsDamage = false;

        // Fade out phase
        float fadeOutTime = 0f;
        while (fadeOutTime < fadeInOutTime)
        {
            fadeOutTime += Time.deltaTime;
            Color color = laserRenderer.color;
            color.a = Mathf.Lerp(1f, 0f, fadeOutTime / fadeInOutTime);
            laserRenderer.color = color;
            yield return null;
        }

        // Destroy the laser after its lifetime is complete
        Destroy(gameObject);
    }

    // Handle collision with the player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && dealsDamage)
        {
            // Check if enough time has passed since the last damage instance
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                GameManager.instance.health -= damage;
                lastDamageTime = Time.time;  // Update the time damage was dealt
            }
        }
    }
}
