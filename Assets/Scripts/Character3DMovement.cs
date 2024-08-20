using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.VFX;

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
    private SpriteRenderer visual;
    private Animator charAnimation;
    public CharacterState animatorState = CharacterState.IDLE;
    public bool xReversed { get => visual.flipX; }
    public Action hitCollider;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource playerWalk;
    [SerializeField]
    private AudioSource shadowWalk;
    [SerializeField]
    private AudioSource jump;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        ground = GetComponent<Character3DGround>();
        visual = GetComponentInChildren<SpriteRenderer>();
        charAnimation = GetComponent<Animator>();
        animatorState = CharacterState.IDLE;
    }

    private void Start()
    {
        SetChargeCharacter(chargeCharacter, Vector3.zero);
        charAnimation.Play("IdleAnimation");
    }

    private void Update()
    {
        onGround = ground.GetOnGround();
        if (!chargeCharacter)
        {
            if (playerWalk.isPlaying)
            {
                playerWalk.Stop();
            }
            if (shadowWalk.isPlaying)
            {
                shadowWalk.Stop();
            }
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
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }

        if (onGround)
        {
            if (body.velocity.x != 0f)
            {
                visual.flipX = body.velocity.x < 0f;
                
                if (animatorState != CharacterState.WALK)
                {
                    charAnimation.Play("WalkAnimation");
                    animatorState = CharacterState.WALK;

                    // Walk sound 1
                    if (frontCharacter)
                    {
                        if (!playerWalk.isPlaying)
                        {
                            playerWalk.Play();
                        }
                    }
                    else
                    {
                        if (!shadowWalk.isPlaying)
                        {
                            shadowWalk.Play();
                        }
                    }
                    // End walk sound
                }
            }
            else if (body.velocity.z != 0f)
            {
                visual.flipX = body.velocity.z < 0f;

                if (animatorState != CharacterState.WALK)
                {
                    charAnimation.Play("WalkAnimation");
                    animatorState = CharacterState.WALK;

                    // Walk sound 2
                    if (frontCharacter)
                    {
                        if (!playerWalk.isPlaying)
                        {
                            playerWalk.Play();
                        }
                    }
                    else
                    {
                        if (!shadowWalk.isPlaying)
                        {
                            shadowWalk.Play();
                        }
                    }
                    // End walk sound
                }
            }
            else
            {
                if (animatorState != CharacterState.IDLE)
                {
                    charAnimation.Play("IdleAnimation");
                    animatorState = CharacterState.IDLE;

                    // Walk sound stop
                    if (frontCharacter)
                    {
                        if (playerWalk.isPlaying)
                        {
                            playerWalk.Stop();
                        }
                    }
                    else
                    {
                        if (shadowWalk.isPlaying)
                        {
                            shadowWalk.Stop();
                        }
                    }
                    // End walk sound
                }
            }
        }
        else
        {
            if (body.velocity.x != 0f)
            {
                visual.flipX = body.velocity.x < 0f;
            }

            if (body.velocity.y > 0f && animatorState != CharacterState.JUMP_UP)
            {
                charAnimation.Play("JumpAnimation");
                animatorState = CharacterState.JUMP_UP;
            }
            else if (body.velocity.y < 0f && animatorState != CharacterState.JUMP_DOWN)
            {
                charAnimation.Play("CoastDownAnimation");
                animatorState = CharacterState.JUMP_DOWN;
            }
        }
    }

    public void FollowCharacterAnimate(CharacterState newAnim, bool newXReversed)
    {
        // just always do this
        visual.flipX = newXReversed;

        if (animatorState == newAnim)
        {
            // pass
            return;
        }

        animatorState = newAnim;

        if (newAnim == CharacterState.IDLE)
        {
            charAnimation.Play("IdleAnimation");
        }
        else if (newAnim == CharacterState.WALK)
        {
            charAnimation.Play("WalkAnimation");
        }
        else if (newAnim == CharacterState.JUMP_UP)
        {
            charAnimation.Play("JumpAnimation");
        }
        else if (newAnim == CharacterState.JUMP_DOWN)
        {
            charAnimation.Play("CoastDownAnimation");
        }
    }

    private float a_button_prev;

    private bool GetAButtonDown()
    {
        Gamepad gamepad = Gamepad.current;
        float a_button = 0f;
        if (gamepad != null)
        {
            a_button = gamepad.buttonSouth.ReadValue();
        }
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
        if(TutorialToggles.LEFT_RIGHT)
        {
            JumpUpdate();
        }
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
            velocity.y += jumpVelocity * (1 + (transform.localScale.x - 1) * 0.5f);

            if (frontCharacter)
            {
                jump.Play();
            }

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
        if (gamepad != null)
        {
            return gamepad.leftStick.ReadValue().x;
        }
        else
        {
            return 0f;
        }
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
        if (gamepad != null)
        {
            return gamepad.leftStick.ReadValue().y;
        }
        else
        {
            return 0f;
        }
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

    [SerializeField] private float DeadZoneZAxis = 0.2f;

    private void HorizontalVerticalMovementUpdate()
    {
        float x = 0;
        if(TutorialToggles.LEFT_RIGHT)
        {
            x = GetLeftRight();
        }
        
        float z = 0;
        if (frontCharacter)
        {
            z = GetForwardsBackwards();
            if (z > 0f && z < DeadZoneZAxis)
            {
                z = 0f;
            }
            else if (z < 0f && z > -1f * DeadZoneZAxis)
            {
                z = 0f;
            }
        }

        float targetX = body.velocity.x;
        float targetZ = body.velocity.z;

        if (rightDisabled && x > 0f)
        {
            targetX = 0f;
        }
        else if (leftDisabled && x < 0f)
        {
            targetX = 0f;
        }
        else
        {
            targetX = Mathf.MoveTowards(targetX, Quantize(x) * maxSpeed, maxAccel * Time.fixedDeltaTime);
        }

        if (frontCharacter && TutorialToggles.DEPTH_WALKING)
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
    // private bool upDisabled = false;
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
    // public void DisableUp()
    // {
    //     upDisabled = true;
    // }

    // public void EnableUp()
    // {
    //     upDisabled = false;
    // }

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

    // Called in fixed update but interpolates movement
    public void RigidbodyMovePosition(Vector3 destination)
    {
        body.MovePosition(destination);
    }

    public void SetVisualEnabled(bool enabled)
    {
        visual.enabled = enabled;
    }

    private void OnCollisionEnter(Collision maybeGround)
    {
        Debug.Log($"Collision enter for {name}");
        if (!chargeCharacter)
        {
            ground.RefreshGround();
        }
        hitCollider?.Invoke();
    }
}

public enum CharacterState
{
    IDLE,
    WALK,
    JUMP_UP,
    JUMP_DOWN
}