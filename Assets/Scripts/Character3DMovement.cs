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
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] public int maxAirJumps = 0;

    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] private float coyoteTime = 0.15f;

    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] private float jumpBuffer = 0.15f;
    [SerializeField] private float jumpVelocity = 10f;

    [Header("Current State")]
    public bool canJumpAgain = false;
    private bool desiredJump;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    // private bool pressingJump;
    public bool onGround;
    [SerializeField] private bool currentlyJumping;

    public bool chargeCharacter = true;

    private Rigidbody body;
    private Character3DGround ground;

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

    private bool GetSpaceDown()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard[Key.Space].wasPressedThisFrame;
    }

    public void CheckJump()
    {
        //This function is called when one of the jump buttons (like space or the A button) is pressed.
        if (GetSpaceDown() || GetAButtonDown()) {
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
        if (desiredJump) {
            DoAJump();

            //Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            //This makes sure you can't do the coyote time double jump bug
            return;
        }

        if (!onGround && body.velocity.y < 7f)
        {
            Physics.gravity = new Vector3(0f, -40f, 0f);
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
            Physics.gravity = new Vector3(0f, -20f, 0f);


            //If we have double jump on, allow us to jump again (but only once)
            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);
            velocity.y += jumpVelocity;

            currentlyJumping = true;
        }

        if (jumpBuffer == 0) {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            desiredJump = false;
        }
        body.velocity = velocity;
    }

    private float GetLeftRightFromStickX()
    {
        Gamepad gamepad = Gamepad.current;
        return gamepad.leftStick.ReadValue().x;
    }

    private float GetLeftRightFromAD()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard.FindKeyOnCurrentKeyboardLayout("d").ReadValue() - keyboard.FindKeyOnCurrentKeyboardLayout("a").ReadValue();
    }

    private float GetLeftRight()
    {
        float xbox_controller = GetLeftRightFromStickX();
        float keyboard = GetLeftRightFromAD();

        return (xbox_controller != 0f) ? xbox_controller : keyboard;
    }

    private float GetForwardsBackwardsFromStickY()
    {
        Gamepad gamepad = Gamepad.current;
        return gamepad.leftStick.ReadValue().y;
    }

     private float GetForwardsBackwardsFromWS()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard.FindKeyOnCurrentKeyboardLayout("w").ReadValue() - keyboard.FindKeyOnCurrentKeyboardLayout("s").ReadValue();
    }

    private float GetForwardsBackwards()
    {
        float xbox_controller = GetForwardsBackwardsFromStickY();
        float keyboard = GetForwardsBackwardsFromWS();

        return (xbox_controller != 0f) ? xbox_controller : keyboard;
    }

    private void HorizontalVerticalMovementUpdate()
    {
        float x = GetLeftRight();
        float z = 0;
        if (frontCharacter)
        {
            z = GetForwardsBackwards();
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
