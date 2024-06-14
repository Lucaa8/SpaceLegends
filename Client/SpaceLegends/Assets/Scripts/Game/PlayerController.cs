using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //Jump section
    /*
     * In order to work nicely, please do not forget to change those settings in your RigidBody2D;
     * - Collision detections : Continuous
     * - Sleeping Mode : Never Sleep
     * - Interpolate : Interpolate
     * - Freeze Rotation Z : Checked
     * - Add a Physic 2D Material with Friction and Bounciness to 0 into the RigidBody2D component
     * - Check in your editor that the below values (jumpForce, coyoteTime, etc... have the right values, 8, 0.11, etc...
     * - Set the gravityScale of your RigidBody2D to something like 7 so he falls quicker
     */
    private bool isGrounded;
    public float jumpForce = 8f;
    // Coyote timings jump
    public float coyoteTime = 0.11f; //Coyote duration in seconds
    private float coyoteTimeCounter;
    // Allow/Disallow jumping during certains intervals
    private bool isJumping = false;
    private float jumpBufferTime = 0.1f; // Seconds in which the player cant jump after the initial jump (to avoid double jumps)
    private float jumpBufferCounter;
    // Jump cut
    private float jumpTimeCounter;
    public float jumpTime = 0.25f;
    private bool isSpaceReleased = true;
    // Increase gravity at peak y position during jump
    private float gravityScale; // You must set this value in the RigidBody2D of your player (to work with those other values in this script, must be 7 with the default -9.81 gravity in the Physics 2D of the project)
    public float fallGravityMultiplier = 1.4f;

    // Left/Right movements section
    // This section will use the FixedUpdate and AddForce to handle smooth movement (e.g. If the player is going right full speed, it'll have a very small timing in which he wont go full speed to the left if he suddently change the directionnal key)
    public float speed = 5f;
    private float direction = 0f;
    // At which rate (how long it takes) the player reach the maximum speed (float speed attribute) after pressing the directionnal key
    public float acceleration = 8f;
    // At which rate the player reach 0 (idle) after releasing the directionnal key
    public float deccelleration = 9f;
    // Linear function to reach the max/min speed
    public float velPower = 1f;

    // The player
    private Rigidbody2D player;

    private Animator animator;

    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gravityScale = player.gravityScale;
    }

    void Update()
    {

        // Checks if the player is on the ground
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, .6f, LayerMask.GetMask("Ground"));

        // If its on the ground and allowed to jump update the coyote time to full time
        if (isGrounded && jumpBufferCounter <= 0f)
        {
            coyoteTimeCounter = coyoteTime;
            isJumping = false;
        }
        else
        {
            // If its not on the ground anymore, decrease the time that he can still initiate a jump
            coyoteTimeCounter -= Time.deltaTime;
        }

        // If the player hits jump, can still jump (grounded or coyote) and is allowed to (he wont be allowed next to this for the jumpBufferCounter seconds)
        // Replace Input.GetButton("Jump") by Input.GetButtonDown("Jump") if you want to obligate the player to release and repress the jump button to jump again
        if (Input.GetButton("Jump") && coyoteTimeCounter > 0f && jumpBufferCounter <= 0f)
        {
            player.velocity = new Vector2(player.velocity.x, jumpForce);
            coyoteTimeCounter = 0f;
            isJumping = true;
            jumpBufferCounter = jumpBufferTime;
            isSpaceReleased = false;
            jumpTimeCounter = jumpTime;
        }

        // Jump cut (The player can hold space to jump higher)
        if(Input.GetButton("Jump") && !isSpaceReleased)
        {
            if (jumpTimeCounter > 0f)
            {
                player.velocity = new Vector2(player.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;  
            }
            else
            {
                isJumping = false;
            }
        }

        // If the player released the jump button then he cant jump cut anymore (avoid weird double jump mid air)
        if (Input.GetButtonUp("Jump"))
        {
            isSpaceReleased = true;
        }

        // Increase gravity when the player reached his peak y jump position so he fall quicker
        if (player.velocity.y < 0f)
        {
            player.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            player.gravityScale = gravityScale;
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        animator.SetBool("OnGround", isGrounded);

    }

    void FixedUpdate()
    {
        // Left and right movement
        direction = Input.GetAxisRaw("Horizontal"); //Either -1 if left key is pressed, 0 if none and 1 if right key is pressed
        float targetSpeed = direction * speed;
        float speedDif = targetSpeed - player.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deccelleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        animator.SetFloat("Speed", Mathf.Abs(player.velocity.x));
        // Vector right to prevent affecting up/down velocity (which is alreary handled by jump)
        player.AddForce(movement * Vector2.right);
    }
}
