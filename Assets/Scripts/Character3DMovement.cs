using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class Character3DMovement : MonoBehaviour
{
    [Header("Customization Knobs")]
    public float maxSpeed = 5f;
    public float maxAccel = 5f;
    //[SerializeField] private float gravityScale = 1f;
    [SerializeField]
    private bool frontCharacter = true;

    [Header("Jumping Stats")]
    [SerializeField, Range(2f, 5.5f)][Tooltip("Maximum jump height")] public float jumpHeight = 7.3f;
    [SerializeField, Range(0.2f, 1.25f)][Tooltip("How long it takes to reach that height before coming back down")] public float timeToJumpApex;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;

    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] private float coyoteTime = 0.15f;

    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] private float jumpBuffer = 0.15f;

    public float jumpScaleMultiplier = 1;

    private float jumpSpeed;

    [Header("Current State")]
    public bool canJumpAgain = false;
    private bool desiredJump;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    // private bool pressingJump;
    public bool onGround;
    [SerializeField] private bool currentlyJumping;

    public bool chargeCharacter = true;

    // [Header("Current State")]
    // public bool onGround;

    [SerializeField] private AnimationCurve jumpCurve;
    private readonly float hang_time;
    private float start_jump_time;

    private Rigidbody body;
    private Character3DGround ground;

    Character3DMovement()
    {
        /* ----- Jump Stuff ----- */
        hang_time = Mathf.Sqrt((2f * jumpHeight) / 9.8f);
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        ground = GetComponent<Character3DGround>();
        SetChargeCharacter(chargeCharacter, Vector3.zero);
    }

    private void Update()
    {
        onGround = ground.GetOnGround();
        if (!chargeCharacter)
        {
            // TODO deal with whatever
            return;
        }
        CheckJump();
        //Check if we're on ground, using Kit's Ground script

        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0) {
            //Instead of immediately turning off "desireJump", start counting up...
            //All the while, the DoAJump function will repeatedly be fired off
            if (desiredJump) {
                jumpBufferCounter += Time.deltaTime;
                if (jumpBufferCounter > jumpBuffer) {
                    //If time exceeds the jump buffer, turn off "desireJump"
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }
            
        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!currentlyJumping && !onGround) {
            coyoteTimeCounter += Time.deltaTime;
        }
        else {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }
    }

    private float a_button_prev;

    private bool GetAButtonDown()
    {
        Gamepad gamepad = Gamepad.current;
        float a_button = gamepad.buttonSouth.ReadValue();
        bool a_button_down = a_button_prev == 0f && a_button > 0f;
        a_button_prev = a_button;

        return a_button_down;
    }
    public void CheckJump()
    {
        //This function is called when one of the jump buttons (like space or the A button) is pressed.
        if (Input.GetKeyDown(KeyCode.Space) || GetAButtonDown()) {
            desiredJump = true;
        }
    }

    private void FixedUpdate()
    {
        if (!chargeCharacter)
        {
            // TODO deal with whatever
            return;
        }
        HorizontalVerticalMovementUpdate();
        JumpUpdate();
    }

    private void JumpUpdate()
    {
        /*if (desiredJump)
        {
            if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain) {
                desiredJump = false;

                start_jump_time = Time.time;

                currentlyJumping = true;
                Debug.Log("Jump Detected");
            }
        }
        
        if (currentlyJumping)
        {
            float current_time = Time.time;
            float current_time_in_jump = current_time - start_jump_time;
            if (current_time_in_jump + 0.02f > hang_time)
            {
                currentlyJumping = false;
            }
            else
            {

            }
        }*/

        if (desiredJump) {
            DoAJump();

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug
            return;
        }

        if (!onGround && body.velocity.y < 0f)
        {
            Physics.gravity = new Vector3(0f, -25f, 0f);
        }
    }

    private void DoAJump()
    {
        UnityEngine.Vector3 velocity = body.velocity;
        //Create the jump, provided we are on the ground, in coyote time, or have a double jump available
        if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain) {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            Physics.gravity = new Vector3(0f, -15f, 0f);


            //If we have double jump on, allow us to jump again (but only once)
            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

            //If Kit is moving up or down when she jumps (such as when doing a double jump), change the jumpSpeed;
            //This will ensure the jump is the exact same strength, no matter your velocity.
            if (body.velocity.y > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (body.velocity.y < 0f) {
                jumpSpeed += Mathf.Abs(velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed * (1 + (transform.localScale.x - 1) * jumpScaleMultiplier);

            Debug.Log("gravity.y: " + Physics.gravity.y);
            Debug.Log("velocity.y: " + velocity.y);
            currentlyJumping = true;
        }

        if (jumpBuffer == 0) {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
        body.velocity = velocity;
    }

    private void HorizontalVerticalMovementUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = 0;
        if (frontCharacter)
        {
            z = Input.GetAxis("Vertical");
        }

        float targetX = body.velocity.x;
        float targetZ = body.velocity.z;

        if (rightDisabled && x > 0f)
        {
            targetX = 0f;
            Debug.Log("right disabled: ");
        }
        else if (leftDisabled && x < 0f)
        {
            targetX = 0f;
            Debug.Log("left disabled: ");
        }
        else
        {
            targetX = Mathf.MoveTowards(targetX, Quantize(x) * maxSpeed, maxAccel * Time.fixedDeltaTime);
        }

        if (frontCharacter)
        {
            if (downDisabled && z < 0)
            {
                targetZ = 0;
            }
            else
            {
                targetZ = Mathf.MoveTowards(targetZ, Quantize(z) * maxSpeed, maxAccel * Time.fixedDeltaTime);
            }
        }

        // Leave y unchanged so jumping is independent.
        if (chargeCharacter)
        {
            body.velocity = new Vector3(targetX, body.velocity.y, targetZ);
        }
    }

    private float Quantize(float i)
    {
        if (i > 0)
        {
            return 1f;
        }
        else if (i < 0)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    public void SetChargeCharacter(bool inCharge, Vector3 velocity)
    {
        this.chargeCharacter = inCharge;
        body.isKinematic = !inCharge;
        if (inCharge)
        {
            if (!frontCharacter)
            {
                velocity.z = 0f;
            }
            body.velocity = velocity;
        }
    }

    private bool rightDisabled = false;
    private bool leftDisabled = false;
    private bool downDisabled = false;
    public void DisableRight()
    {
        rightDisabled = true;
    }
    public void EnableRight()
    {
        rightDisabled = false;
    }

    public void DisableLeft()
    {
        leftDisabled = true;
    }
    public void EnableLeft()
    {
        leftDisabled = false;
    }

    public void DisableDown()
    {
        downDisabled = true;
    }

    public void EnableDown()
    {
        downDisabled = false;
    }

    public void HitHead()
    {
        if (body.velocity.y > 0)
        {
            body.velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        }
    }

    public Vector3 Velocity()
    {
        return body.velocity;
    }

    public bool AcceleratedGroundCheck(float sunYVelocity, out Vector3 hit)
    {
        return ground.AcceleratedGroundCheck(sunYVelocity, out hit);
    }
}
