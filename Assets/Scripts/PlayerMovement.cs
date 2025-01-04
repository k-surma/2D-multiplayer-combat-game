using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    private int jumpCount; // Tracks the number of jumps
    private int maxJumps = 2; // Maximum allowed jumps

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;



    void Update()
    {

        if (!IsOwner) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        // Log the horizontal movement for debugging
        //Debug.Log($"Horizontal Input: {horizontal}");

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            // Perform the jump and log the details
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            jumpCount++;
            //Debug.Log($"Jump! Count: {jumpCount}, Velocity: {rb.linearVelocity}");
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            Debug.Log($"Jump button released, reducing upward velocity: {rb.linearVelocity}");
        }

        Flip();

        // Reset jump count when grounded
        if (IsGrounded())
        {
            //if (jumpCount > 0) Debug.Log("Player grounded. Resetting jump count.");
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement and log it
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        Debug.Log($"FixedUpdate -> Velocity: {rb.linearVelocity}");
    }

    private bool IsGrounded()
    {
        // Checks if the player is touching the ground and logs the result
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
        Debug.Log($"IsGrounded: {grounded}");
        return grounded;
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
            //Debug.Log($"Player flipped. FacingRight: {isFacingRight}");
        }
    }
}

