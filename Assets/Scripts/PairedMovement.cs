using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PairedMovement : MonoBehaviour
{
    [Header("Key References")]
    [SerializeField]
    public Character3DMovement frontCharacter;
    [SerializeField]
    public Character3DMovement backCharacter;

    public float mouseJumpMultiplier = 3.5f;
    // Probably needs more, tbd
    [SerializeField]
    public GameObject sun;

    private Plane backPlane = new Plane(Vector3.back, new Vector3(0, 0, 16));
    private Plane frontPlane = new Plane(Vector3.back, Vector3.zero);

    [SerializeField]
    private Vector3 playerHeight = new Vector3(0f, 0.9f, 0f);
    [SerializeField]
    private Vector3 playerWidth = new Vector3(0.5f, 0f, 0f);

    [Header("Sun")]
    [SerializeField]
    private float sunOffsetY = 0;
    private float sunYVelocity;

    [SerializeField]
    private float sunMaxY = 10f;
    [SerializeField]
    private float sunMinY = -10f;

    [SerializeField]
    private float sunZPosition = -5f;

    private Character3DMovement leadCharacter;
    private Character3DMovement followCharacter;

    [SerializeField]
    private LayerMask ground;

    [SerializeField] private float KillY = -100f;

    [Header("Sound")]
    [SerializeField]
    private AudioSource sunSound;

    private void Awake()
    {
        TutorialToggles.SetShadowState += SetShadowState;
    }

    private void Start()
    {
        leadCharacter = frontCharacter;
        followCharacter = backCharacter;
        frontCharacter.hitCollider += PlayerHitCollider;
        backCharacter.hitCollider += ShadowHitCollider;
    }

    private void PlayerHitCollider()
    {
        // if (!frontCharacter.chargeCharacter)
        //{
            FollowCharacterCollisions(frontCharacter);

            if (collisionUp && sunYVelocity > 0)
            {
                TestHandoff();
                sunYVelocity = 0;
            }
        //}
    }

    private void ShadowHitCollider()
    {
        if (!backCharacter.chargeCharacter)
        {
            FollowCharacterCollisions(backCharacter);

            if (collisionUp && sunYVelocity < 0)
            {
                sunYVelocity = 0;
            }
        }
    }

    public void KillPlayer()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    [SerializeField] private float maxAccelShadow = 1f;
    [SerializeField] private float maxVelShadow = 5f;

    private int sunDesiredDelta = 0;

    private bool sunSoundPlayedYet = false;

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        float left_trigger = 0f;
        float right_trigger = 0f;
        // times negative one because we want left trigger to move sun down
        if (gamepad != null)
        {
            left_trigger = -1f * Quantize(gamepad.leftTrigger.ReadValue());
            right_trigger = Quantize(gamepad.rightTrigger.ReadValue());
        }

        float total_trigger = left_trigger + right_trigger;

        float left_shoulder = 0f;
        float right_shoulder = 0f;
        if (gamepad != null)
        {
            left_shoulder = -1f * gamepad.leftShoulder.ReadValue();
            right_shoulder = gamepad.rightShoulder.ReadValue();
        }
        
        float total_shoulder = left_shoulder + right_shoulder;

        Keyboard keyboard = Keyboard.current;
        bool sunDeltaUp = keyboard[Key.UpArrow].IsPressed();
        bool sunDeltaDown = keyboard[Key.DownArrow].IsPressed();

        float sunDeltaY;
        if (sunDeltaUp && sunDeltaDown)
        {
            sunDeltaY = 0f;
        }
        else if (sunDeltaUp && !sunDeltaDown)
        {
            sunDeltaY = -1f;
        }
        else if (!sunDeltaUp && sunDeltaDown)
        {
            sunDeltaY = 1f;
        }
        else
        {
            sunDeltaY = 0f;
        }

        float totalController = total_shoulder + total_trigger;

        if (sunDeltaY > 0 || totalController > 0)
        {
            sunDesiredDelta = 1;
        }
        else if (sunDeltaY < 0 || totalController < 0)
        {
            sunDesiredDelta = -1;
        }
        else
        {
            sunDesiredDelta = 0;
        }

        if (sunYVelocity != 0 && !sunSound.isPlaying)
        {
            if (!sunSoundPlayedYet)
            {
                sunSound.Play();
                sunSoundPlayedYet = true;
            }
            else
            {
                sunSound.UnPause();
            }
        }
        else if (sunYVelocity == 0 && sunSound.isPlaying)
        {
            sunSound.Pause();
        }

        followCharacter.FollowCharacterAnimate(leadCharacter.animatorState, leadCharacter.xReversed);

        if (leadCharacter.gameObject.transform.position.y < KillY || followCharacter.gameObject.transform.position.y < KillY)
        {
            KillPlayer();
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

    private void LateUpdate()
    {
        sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sunZPosition);
    }

    private void FixedUpdate()
    {
        float distance1 = Vector3.Distance(sun.transform.position, frontCharacter.transform.position);
        float distance2 = Vector3.Distance(backCharacter.transform.position, frontCharacter.transform.position);
        
        // sunYVelocity = Mathf.Clamp(sunYVelocity, -1f, 1f);

        float sunAdjustRatio;
        if (leadCharacter == frontCharacter)
        {
            sunAdjustRatio = distance2 / distance1;
        }
        else
        {
            sunAdjustRatio = distance1 / distance2;
        }

        float sunAdjustedVelocity = sunYVelocity * sunAdjustRatio;     

        TestHandoff();

        UpdateSun();

        if (followCharacter == frontCharacter && sunAdjustedVelocity < 0)
        {
            // Hard set position here because otherwise it breaks.
            if (followCharacter.AcceleratedGroundCheck(sunAdjustedVelocity, out Vector3 thingHit))
            {
                Vector3 followCharacterPos = followCharacter.transform.position;
                followCharacter.transform.position = new Vector3(followCharacterPos.x, thingHit.y + playerHeight.y * followCharacter.transform.localScale.y, followCharacterPos.z);

                Vector3 newVelocity = leadCharacter.Velocity();
                newVelocity.y += Mathf.Abs(sunYVelocity) * mouseJumpMultiplier;

                followCharacter.SetChargeCharacter(true, newVelocity);
                leadCharacter.SetChargeCharacter(false, Vector3.zero);

                sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sun.transform.position.z);
            }
        }
        else if (followCharacter == backCharacter && sunAdjustedVelocity > 0)
        {
            // Hard set position here because otherwise it breaks.
            if (followCharacter.AcceleratedGroundCheck(sunAdjustedVelocity, out Vector3 thingHit))
            {
                Vector3 followCharacterPos = followCharacter.transform.position;
                followCharacter.transform.position = new Vector3(followCharacterPos.x, thingHit.y + playerHeight.y * followCharacter.transform.localScale.y, followCharacterPos.z);

                Vector3 newVelocity = leadCharacter.Velocity();
                newVelocity.y += Mathf.Abs(sunYVelocity) * mouseJumpMultiplier;

                followCharacter.SetChargeCharacter(true, newVelocity);
                leadCharacter.SetChargeCharacter(false, Vector3.zero);
                sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sun.transform.position.z);
            }
        }

        // sunYVelocity = 0;

        leadCharacter = frontCharacter.chargeCharacter ? frontCharacter : backCharacter;
        followCharacter = frontCharacter.chargeCharacter ? backCharacter : frontCharacter;

        Vector3 sunPos = sun.transform.position;
        Vector3 direction = (leadCharacter.transform.position - sun.transform.position).normalized;

        // Debug.DrawRay(sunPos, direction, Color.yellow, 1);

        float along;

        if (followCharacter == backCharacter)
        {
            backPlane.Raycast(new Ray(sunPos, direction), out along);
        }
        else
        {
            frontPlane.SetNormalAndPosition(Vector3.back, new Vector3(0, 0, frontCharacter.transform.position.z));
            frontPlane.Raycast(new Ray(sunPos, direction), out along);
        }

        Vector3 followCharacterFinal = sunPos + direction * along;
        // Debug.Log($"{Time.fixedTime} Follow character math between {followCharacterFinal.y} and {followCharacter.transform.position.y}. Sun Y Velocity {sunYVelocity}. {sunOffsetY}");

        followCharacter.RigidbodyMovePosition(sunPos + direction * along);

        //Shadow Scale
        Vector3 shadowHeightDirection = (frontCharacter.transform.position + playerHeight - sun.transform.position).normalized;
        backPlane.Raycast(new Ray(sunPos, shadowHeightDirection), out float shadowAlong);

        float shadowY = ((sunPos + shadowHeightDirection * shadowAlong).y - backCharacter.transform.position.y) / (playerHeight.y * 2f);
        // Debug.Log($"Shadow Y {(shadowHeightDirection * shadowAlong).y}, centerY {backCharacter.transform.position.y}");
        if (shadowY <= 0)
        {
            Debug.Log("It's broken");
        }

        // Vector3 shadowWidthDirection = (frontCharacter.transform.position + playerWidth - sun.transform.position).normalized;
        // backPlane.Raycast(new Ray(sunPos, shadowWidthDirection), out float shadowWidthAlong);
        // float shadowX = (sunPos + shadowWidthDirection * shadowWidthAlong).x - backCharacter.transform.position.x;

        float distanceSun = Mathf.Abs(sun.transform.position.z - frontCharacter.transform.position.z);
        float characterShadow = Mathf.Abs(frontCharacter.transform.position.z - backCharacter.transform.position.z);

        // Z scale
        float ratio = (distanceSun + characterShadow) / distanceSun;

        backCharacter.transform.localScale = new Vector3(ratio, ratio, 1f);

        // Debug.DrawRay(sunPos, direction * along, Color.white);
        // Debug.DrawRay(sunPos, shadowHeightDirection * shadowAlong, Color.red);
        // Debug.DrawRay(sunPos, shadowWidthDirection * shadowWidthAlong, Color.green);

        FollowCharacterCollisions(followCharacter);
        
        if (collisionRight)
        {
            leadCharacter.DisableRight();
        }
        else
        {
            leadCharacter.EnableRight();
        }

        if (collisionLeft)
        {
            leadCharacter.DisableLeft();
        }
        else
        {
            leadCharacter.EnableLeft();
        }

        if (collisionUp && leadCharacter.Velocity().y > 0f)
        {
            leadCharacter.HitHead();
        }

        // Annoying edge case where the player is in charge and makes the shadow bigger but the shadow hits its head
        if (frontCharacter.chargeCharacter && frontCharacter.Velocity().z < 0f)
        {
            FollowCharacterCollisions(backCharacter);
            if (collisionUp)
            {
                frontCharacter.DisableDown();
            }
            else
            {
                frontCharacter.EnableDown();
            }
        }
        else if (frontCharacter.chargeCharacter && frontCharacter.Velocity().z > 0f)
        {
            FollowCharacterCollisions(backCharacter);
            if (collisionDown)
            {
                if (backCharacter.onGround)
                {
                    Vector3 newVelocity = leadCharacter.Velocity();
                    followCharacter.SetChargeCharacter(true, newVelocity);
                    leadCharacter.SetChargeCharacter(false, Vector3.zero);
                }
            }
        }
        else
        {
            frontCharacter.EnableDown();
        }
    }

    private void UpdateSun()
    {
        // Update intended velocity
        if (sunDesiredDelta > 0)
        {
            sunYVelocity = Mathf.MoveTowards(sunYVelocity, maxVelShadow, maxAccelShadow);
        }
        else if (sunDesiredDelta < 0)
        {
            sunYVelocity = Mathf.MoveTowards(sunYVelocity, -maxVelShadow, maxAccelShadow);
        }
        else
        {
            sunYVelocity = Mathf.MoveTowards(sunYVelocity, 0f, maxAccelShadow);
        }

        // Then check for constraints
        if (sunYVelocity > 0)
        {
            FollowCharacterCollisions(frontCharacter);
            bool playerUp = collisionUp;
            FollowCharacterCollisions(backCharacter);
            bool shadowDown = collisionDown;

            // Debug.LogWarning($"{Time.fixedTime} Charge is {leadCharacter.name}: Vel > 0: Did collision check, player has {playerUp} shadow has {shadowDown}");
            // if (playerColliders.Length > 0 && shadowColliders.Length > 0)
            if (playerUp && shadowDown)
            {
                // Debug.LogError($"{Time.fixedTime} Y Velocity should be 0");
                float ratio = Mathf.Abs(sun.transform.position.z - frontCharacter.transform.position.z) / Mathf.Abs(backCharacter.transform.position.z - frontCharacter.transform.position.z);
                
                sunOffsetY = (frontCharacter.transform.position.y - backCharacter.transform.position.y) * ratio;

                sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sunZPosition);
                
                sunYVelocity = 0;
            }
        }
        else if (sunYVelocity < 0)
        {
            // player down or shadow up
            FollowCharacterCollisions(frontCharacter);
            bool playerDown = collisionDown;
            FollowCharacterCollisions(backCharacter);
            bool shadowUp = collisionUp;

            // Debug.Log($"Vel < 0: Did collision check, player has {playerDown} shadow has {shadowUp}");

            if (playerDown && shadowUp)
            {
                sunYVelocity = 0;
            }
        }

        if (TutorialToggles.LIGHT_HEIGHT)
        {
            // Debug.Log($"{Time.fixedTime} Y Velocity is {sunYVelocity}");
            sunOffsetY += sunYVelocity;
            sunOffsetY = Mathf.Clamp(sunOffsetY, sunMinY, sunMaxY);
        }
        else
        {
            sunYVelocity = 0;
        }
    }

    private void TestHandoff()
    {
        if (sunYVelocity > 0 && backCharacter.onGround && frontCharacter.chargeCharacter)
        {
            // Set back character in control.
            backCharacter.SetChargeCharacter(true, frontCharacter.Velocity());
            frontCharacter.SetChargeCharacter(false, Vector3.zero);
        }
        else if (sunYVelocity < 0 && frontCharacter.onGround && backCharacter.chargeCharacter)
        {
            frontCharacter.SetChargeCharacter(true, backCharacter.Velocity());
            backCharacter.SetChargeCharacter(false, Vector3.zero);
        }
        if (backCharacter.onGround && !frontCharacter.onGround && frontCharacter.chargeCharacter)
        {
            backCharacter.SetChargeCharacter(true, frontCharacter.Velocity());
            frontCharacter.SetChargeCharacter(false, Vector3.zero);
        }
        else if (!backCharacter.onGround && frontCharacter.onGround && backCharacter.chargeCharacter)
        {
            frontCharacter.SetChargeCharacter(true, backCharacter.Velocity());
            backCharacter.SetChargeCharacter(false, Vector3.zero);
        }
        else if (!backCharacter.onGround && !frontCharacter.onGround)
        {
            bool playerClose = Physics.Raycast(frontCharacter.transform.position - new Vector3(0f, 0.9f, 0f), Vector3.down, out RaycastHit playerLanding, Mathf.Abs(frontCharacter.Velocity().y) * 1.2f, ground);
            Debug.DrawRay(frontCharacter.transform.position, Vector3.down * Mathf.Abs(frontCharacter.Velocity().y), Color.green);
            bool shadowClose = Physics.Raycast(backCharacter.transform.position - new Vector3(0f, 0.9f, 0f) * backCharacter.transform.localScale.y, Vector3.down, out RaycastHit shadowLanding, Mathf.Abs(backCharacter.Velocity().y) * 1.2f, ground);
            Debug.DrawRay(backCharacter.transform.position, Vector3.down * Mathf.Abs(backCharacter.Velocity().y), Color.black);

            if (playerClose && shadowClose)
            {
                // closest
                if (Vector3.SqrMagnitude(frontCharacter.transform.position - playerLanding.point) > Vector3.SqrMagnitude(backCharacter.transform.position - shadowLanding.point))
                {
                    shadowClose = false;
                }
                else
                {
                    playerClose = false;
                }
            }
            
            if (playerClose && !shadowClose && !frontCharacter.chargeCharacter)
            {
                frontCharacter.SetChargeCharacter(true, backCharacter.Velocity());
                backCharacter.SetChargeCharacter(false, Vector3.zero);
            }
            else if (!playerClose && shadowClose && !backCharacter.chargeCharacter)
            {
                backCharacter.SetChargeCharacter(true, frontCharacter.Velocity());
                frontCharacter.SetChargeCharacter(false, Vector3.zero);
            }
        }
    }

    private bool collisionRight = false;
    private bool collisionLeft = false;
    private bool collisionUp = false;
    private bool collisionDown = false;

    private void FollowCharacterCollisions(Character3DMovement targetChar)
    {
        // Divide size by 2 because it's half extents
        // collisionUp = Physics.Raycast(targetChar.transform.position + playerWidth, Vector3.up, 0.6f * targetChar.transform.localScale.z) ||
        //               Physics.Raycast(targetChar.transform.position - playerWidth, Vector3.up, 0.6f * targetChar.transform.localScale.z);
        collisionUp = Physics.OverlapBox(targetChar.transform.position + new Vector3(0, 0.9f, 0) * targetChar.transform.localScale.y,
                                         Vector3.Scale(new Vector3(0.75f, 0.4f, 0.5f), targetChar.transform.localScale) / 2f,
                                         Quaternion.identity, ground).Length != 0;
        // collision down
        collisionDown = Physics.OverlapBox(targetChar.transform.position - new Vector3(0, 1.2f, 0) * targetChar.transform.localScale.y,
                                           Vector3.Scale(new Vector3(0.75f, 0.4f, 0.5f), targetChar.transform.localScale) / 2f,
                                           Quaternion.identity, ground).Length != 0;

        // collisionRight = Physics.Raycast(targetChar.transform.position, Vector3.right, 0.6f * targetChar.transform.localScale.x);
        collisionRight = Physics.OverlapBox(targetChar.transform.position + Vector3.Scale(new Vector3(0.5f, -0.1f, 0), targetChar.transform.localScale),
                                            Vector3.Scale(new Vector3(0.2f, 1.2f, 0.5f), targetChar.transform.localScale) / 2f,
                                            Quaternion.identity, ground).Length != 0;

        // collisionLeft = Physics.Raycast(targetChar.transform.position, Vector3.left, 0.6f * targetChar.transform.localScale.x);
        collisionLeft = Physics.OverlapBox(targetChar.transform.position - Vector3.Scale(new Vector3(0.5f, 0.1f, 0), targetChar.transform.localScale),
                                        Vector3.Scale(new Vector3(0.2f, 1.2f, 0.5f), targetChar.transform.localScale) / 2f,
                                        Quaternion.identity, ground).Length != 0;

        // Debug.DrawRay(targetChar.transform.position, Vector3.up * 0.6f * targetChar.transform.localScale.z, Color.red);
    }

    private void SetShadowState(bool enabled)
    {
        backCharacter.SetVisualEnabled(enabled);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(sun.transform.position, 0.5f);

        if (leadCharacter == null || followCharacter == null)
        {
            leadCharacter = frontCharacter.chargeCharacter ? frontCharacter : backCharacter;
            followCharacter = frontCharacter.chargeCharacter ? backCharacter : frontCharacter;
        }

        Gizmos.color = Color.gray;
        // top
        Gizmos.DrawCube(followCharacter.transform.position + new Vector3(0, 0.9f, 0) * followCharacter.transform.localScale.y,
            Vector3.Scale(new Vector3(0.75f, 0.4f, 0.5f), followCharacter.transform.localScale));

        // bottom
        Gizmos.DrawCube(followCharacter.transform.position - new Vector3(0, 1.2f, 0) * followCharacter.transform.localScale.y,
            Vector3.Scale(new Vector3(0.75f, 0.4f, 0.5f), followCharacter.transform.localScale));

        // right
        Gizmos.color = Color.green;
        Gizmos.DrawCube(followCharacter.transform.position + Vector3.Scale(new Vector3(0.5f, -0.1f, 0), followCharacter.transform.localScale),
            Vector3.Scale(new Vector3(0.2f, 1.2f, 0.5f), followCharacter.transform.localScale));

        // left
        Gizmos.color = Color.red;
        Gizmos.DrawCube(followCharacter.transform.position - Vector3.Scale(new Vector3(0.5f, 0.1f, 0), followCharacter.transform.localScale),
            Vector3.Scale(new Vector3(0.2f, 1.2f, 0.5f), followCharacter.transform.localScale));
    }
}
