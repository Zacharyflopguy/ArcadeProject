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

    private void Awake()
    {
        // Initialize the input actions
        moveAction = playerInput.FindAction("Move");
        lookAction = playerInput.FindAction("Look");
        dashAction = playerInput.FindAction("Dash");

        // Enable the input actions
        moveAction.Enable();
        lookAction.Enable();
        dashAction.Enable();

        // Initialize Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleDash();
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

            // Clamp speed to maxSpeed
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
        else
        {
            // Decelerate when not thrusting
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, deceleration * Time.deltaTime);
        }
    }

    void HandleDash()
    {
        // Check if the dash button is pressed and the cooldown is done
        if (dashAction.triggered && canDash)
        {
            // Perform the dash (teleport forward)
            rb.position += (Vector2)transform.up * dashDistance;

            // Start the cooldown
            StartCoroutine(DashCooldown());
        }
    }

    // Cooldown for the dash ability
    IEnumerator DashCooldown()
    {
        canDash = false;  // Disable dashing during cooldown
        yield return new WaitForSeconds(dashCooldown);  // Wait for cooldown time
        canDash = true;   // Re-enable dashing
    }
    
    
} //