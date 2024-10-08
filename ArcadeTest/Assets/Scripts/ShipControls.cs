using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipControls : MonoBehaviour
{
    // Vars
    public InputActionAsset playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction dashAction;
    private InputAction fireAction;

    [Header("Movement Vars")]
    public float moveSpeed = 10f;      // Thrust speed
    public float maxSpeed = 15f;       // Max speed the ship can reach
    public float rotationSpeed = 200f;  // Speed of rotation to face the look direction
    public float deceleration = 2f;     // Deceleration factor when not thrusting
    public float dashDistance = 5f;     // Distance to teleport when dashing
    public float dashCooldown = 2f;     // Cooldown time in seconds

    private Rigidbody2D rb;             // Ship's Rigidbody2D for physics-based movement
    private Vector2 thrustDirection;
    private bool isThrusting;
    private bool canDash = true;        // Can dash right now?
    private bool canFire = true;        // Can fire right now?
    
    [Header("Weapons")]
    public GameObject projectilePrefab;  // Reference to the projectile prefab
    public Transform firePoint;          // Reference to the fire point transform
    public float fireCooldown = 0.5f;    // Cooldown time between shots

    private void Awake()
    {
        // Initialize the input actions
        moveAction = playerInput.FindAction("Move");
        lookAction = playerInput.FindAction("Look");
        dashAction = playerInput.FindAction("Dash");
        fireAction = playerInput.FindAction("Fire");

        // Enable the input actions
        moveAction.Enable();
        lookAction.Enable();
        dashAction.Enable();
        fireAction.Enable();

        // Initialize Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleDash();
        HandleFire();
    }

    void HandleLook()
    {
        // Get the look direction from the right stick
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Calculate the angle from the input
        if (lookInput != Vector2.zero) // Check if there's any input
        {
            float angle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg - 90f;

            // Rotate the ship towards the look direction smoothly
            float targetRotation = Mathf.LerpAngle(rb.rotation, angle, rotationSpeed * Time.deltaTime);
            rb.MoveRotation(targetRotation);
        }
    }

    void HandleMovement()
    {
        // Get movement input direction
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        isThrusting = moveInput.magnitude > 0;

        // If thrusting, add force in the direction of the input
        if (isThrusting)
        {
            thrustDirection = moveInput.normalized; // Use the movement input direction
            rb.AddForce(thrustDirection * (moveSpeed * Time.deltaTime), ForceMode2D.Force);
        }
        else
        {
            // Decelerate when not thrusting
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, deceleration * Time.deltaTime);
        }
        
        // Clamp speed to maxSpeed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void HandleDash()
    {
        // Check if the dash button is pressed and the cooldown is done
        if (dashAction.triggered && canDash && GameManager.instance.stamina >= 10)
        {
            // Perform the dash (teleport forward)
            rb.position += (Vector2)transform.up * dashDistance;

            //Subtract stamina
            GameManager.instance.stamina -= 10;
            
            // Start the cooldown
            StartCoroutine(DashCooldown());
        }
        else if (dashAction.triggered && !canDash || dashAction.triggered && GameManager.instance.stamina < 10)
        {
            GameManager.instance.invalidRumble();
        }
    }

    // Cooldown for the dash ability
    IEnumerator DashCooldown()
    {
        canDash = false;  // Disable dashing during cooldown
        yield return new WaitForSeconds(dashCooldown);  // Wait for cooldown time
        canDash = true;   // Re-enable dashing
    }
    
    void HandleFire()
    {
        // Check if the fire button is pressed and the cooldown is done
        if (fireAction.IsPressed() && canFire)
        {
            // Fire a projectile
            FireProjectile();
            
            // Start the cooldown
            StartCoroutine(FireCooldown());
        }
    }
    
    IEnumerator FireCooldown()
    {
        canFire = false;  // Disable firing during cooldown
        yield return new WaitForSeconds(fireCooldown);  // Wait for cooldown time
        canFire = true;   // Re-enable firing
    }
    
    void FireProjectile()
    {
        // Instantiate a projectile at the fire point position and rotation
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        projectile.SetActive(true);
    }
    
} //