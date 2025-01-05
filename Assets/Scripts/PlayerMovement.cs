using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    private int jumpCount;
    private int maxJumps = 2;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private NetworkVariable<bool> isFacingRightNetwork = new NetworkVariable<bool>(true);

    private void Start()
    {
        if (!IsOwner) return;

        // Subscribe to facing direction changes
        isFacingRightNetwork.OnValueChanged += (oldValue, newValue) =>
        {
            Vector3 localScale = transform.localScale;
            localScale.x = newValue ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
            transform.localScale = localScale;
        };
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Apply the current facing direction on spawn
        Vector3 localScale = transform.localScale;
        localScale.x = isFacingRightNetwork.Value ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }

    void Update()
    {
        if (!IsOwner) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            jumpCount++;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        Flip();

        if (IsGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;

            if (IsOwner)
            {
                // Update the NetworkVariable
                SetFacingDirectionServerRpc(isFacingRight);
            }
        }
    }

    [ServerRpc]
    private void SetFacingDirectionServerRpc(bool facingRight)
    {
        isFacingRightNetwork.Value = facingRight;
    }
}



//code to try out the smoothness

/*using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;
    private int jumpCount;
    private int maxJumps = 2;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private NetworkVariable<bool> isFacingRightNetwork = new NetworkVariable<bool>(true);
    private float targetScaleX;

    private void Start()
    {
        if (!IsOwner) return;

        // Initialize the target scale
        targetScaleX = transform.localScale.x;

        // Subscribe to facing direction changes
        isFacingRightNetwork.OnValueChanged += (oldValue, newValue) =>
        {
            targetScaleX = newValue ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        };
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Apply the initial flip state
        targetScaleX = isFacingRightNetwork.Value ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            // Smoothly interpolate the local scale on non-owners
            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Lerp(localScale.x, targetScaleX, Time.deltaTime * 10f); // Adjust smoothing speed
            transform.localScale = localScale;
        }

        if (!IsOwner) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpCount++;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        Flip();

        if (IsGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;

            if (IsOwner)
            {
                // Update the NetworkVariable
                SetFacingDirectionServerRpc(isFacingRight);
            }
        }
    }

    private void UpdateFlipState(bool facingRight)
    {
        targetScaleX = facingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
    }

    [ServerRpc]
    private void SetFacingDirectionServerRpc(bool facingRight)
    {
        isFacingRightNetwork.Value = facingRight;
    }
}
*/