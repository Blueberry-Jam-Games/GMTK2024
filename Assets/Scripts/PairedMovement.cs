using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
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
    private Vector3 playerHeight = new Vector3(0f, 0.5f, 0f);
    [SerializeField]
    private Vector3 playerWidth = new Vector3(0.5f, 0f, 0f);

    [Header("Sun")]
    private float sunOffsetY;
    private float sunYVelocity;
    [SerializeField]
    private float sunCoefficient = 0.1f;

    [SerializeField]
    private float sunMaxY = 10f;
    [SerializeField]
    private float sunMinY = -10f;

    private Character3DMovement leadCharacter;
    private Character3DMovement followCharacter;

    [SerializeField]
    private LayerMask ground;

    private void Start()
    {
        leadCharacter = frontCharacter;
        followCharacter = backCharacter;
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        // times negative one because we want left trigger to move sun down
        float left_trigger = -1f * gamepad.leftTrigger.ReadValue();
        float right_trigger = gamepad.rightTrigger.ReadValue();
        float total = left_trigger + right_trigger;

        float right_axis_y = gamepad.rightStick.ReadValue().y;

        float mouseDelta = Input.GetAxis("Mouse Y");

        if (total != 0f)
        {
            sunYVelocity += total * sunCoefficient;
        }
        else if (right_axis_y != 0f)
        {
            sunYVelocity += right_axis_y * sunCoefficient;
        }
        else
        {
            sunYVelocity =+ mouseDelta * sunCoefficient;
        }
    }

    private void LateUpdate()
    {
        sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sun.transform.position.z);
        // TestHandoff();
    }

    private void FixedUpdate()
    {
        float distance1 = Vector3.Distance(sun.transform.position, frontCharacter.transform.position);
        float distance2 = Vector3.Distance(backCharacter.transform.position, frontCharacter.transform.position);
        
        sunYVelocity = Mathf.Clamp(sunYVelocity, -1.2f, 1.2f);

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
            if (followCharacter.AcceleratedGroundCheck(sunAdjustedVelocity, out RaycastHit thingHit))
            {
                Vector3 followCharacterPos = followCharacter.transform.position;
                followCharacter.transform.position = new Vector3(followCharacterPos.x, thingHit.point.y + playerHeight.y * followCharacter.transform.localScale.y, followCharacterPos.z);

                Vector3 newVelocity = leadCharacter.Velocity();
                newVelocity.y += Mathf.Abs(sunYVelocity) * mouseJumpMultiplier;

                followCharacter.SetChargeCharacter(true, newVelocity);
                leadCharacter.SetChargeCharacter(false, Vector3.zero);;
                sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sun.transform.position.z);
            }
        }
        else if (followCharacter == backCharacter && sunAdjustedVelocity > 0)
        {
            if (followCharacter.AcceleratedGroundCheck(sunAdjustedVelocity, out RaycastHit thingHit))
            {
                Vector3 followCharacterPos = followCharacter.transform.position;
                followCharacter.transform.position = new Vector3(followCharacterPos.x, thingHit.point.y + playerHeight.y * followCharacter.transform.localScale.y, followCharacterPos.z);

                Vector3 newVelocity = leadCharacter.Velocity();
                newVelocity.y += Mathf.Abs(sunYVelocity) * mouseJumpMultiplier;

                followCharacter.SetChargeCharacter(true, newVelocity);
                leadCharacter.SetChargeCharacter(false, Vector3.zero);
                sun.transform.position = new Vector3(frontCharacter.transform.position.x, frontCharacter.transform.position.y + sunOffsetY, sun.transform.position.z);
            }
        }

        sunYVelocity = 0;

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

        followCharacter.transform.position = sunPos + direction * along;

        // Shadow Scale
        Vector3 shadowHeightDirection = (frontCharacter.transform.position + playerHeight - sun.transform.position).normalized;
        backPlane.Raycast(new Ray(sunPos, shadowHeightDirection), out float shadowAlong);

        float shadowY = (sunPos + shadowHeightDirection * shadowAlong).y - backCharacter.transform.position.y;
        // Debug.Log($"Shadow Y {(shadowHeightDirection * shadowAlong).y}, centerY {backCharacter.transform.position.y}");
        if (shadowY <= 0)
        {
            Debug.Log("It's broken");
        }

        Vector3 shadowWidthDirection = (frontCharacter.transform.position + playerWidth - sun.transform.position).normalized;
        backPlane.Raycast(new Ray(sunPos, shadowWidthDirection), out float shadowWidthAlong);
        float shadowX = (sunPos + shadowWidthDirection * shadowWidthAlong).x - backCharacter.transform.position.x;

        backCharacter.transform.localScale = new Vector3(shadowX * 2f, shadowY * 2f, 1f);

        Debug.DrawRay(sunPos, direction * along, Color.white);
        Debug.DrawRay(sunPos, shadowHeightDirection * shadowAlong, Color.red);
        Debug.DrawRay(sunPos, shadowWidthDirection * shadowWidthAlong, Color.green);

        CheckCollisionRight(followCharacter);
        CheckCollisionLeft(followCharacter);
        CheckCollisionUp(followCharacter);
        
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
            CheckCollisionUp(backCharacter);
            if (collisionUp)
            {
                frontCharacter.DisableDown();
            }
            else
            {
                frontCharacter.EnableDown();
            }
        }
        else
        {
            frontCharacter.EnableDown();
        }
    }

    private void UpdateSun()
    {
        if (sunYVelocity > 0)
        {
            // shadow down or player up
            Collider[] playerColliders = Physics.OverlapBox(frontCharacter.transform.position + playerHeight.y * frontCharacter.transform.localScale, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, ground);
            Collider[] shadowColliders = Physics.OverlapBox(backCharacter.transform.position - playerHeight.y * backCharacter.transform.localScale, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, ground);
            if (playerColliders.Length > 0 && shadowColliders.Length > 0)
            {
                sunYVelocity = 0;
            }
        }
        else if (sunYVelocity < 0)
        {
            // player down or shadow up
            Collider[] playerColliders = Physics.OverlapBox(frontCharacter.transform.position - playerHeight.y * frontCharacter.transform.localScale, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, ground);
            Debug.DrawRay(frontCharacter.transform.position - playerHeight.y * frontCharacter.transform.localScale, Vector3.down * 0.2f, Color.magenta);
            Collider[] shadowColliders = Physics.OverlapBox(backCharacter.transform.position + playerHeight.y * backCharacter.transform.localScale, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, ground);
            Debug.DrawRay(backCharacter.transform.position + playerHeight.y * backCharacter.transform.localScale, Vector3.up * 0.2f, Color.magenta);
            if (playerColliders.Length > 0 && shadowColliders.Length > 0)
            {
                sunYVelocity = 0;
            }
        }
        sunOffsetY += sunYVelocity;
        sunOffsetY = Mathf.Clamp(sunOffsetY, sunMinY, sunMaxY);
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
    }

    [SerializeField][Tooltip("Distance between the left and right checking colliders")] private UnityEngine.Vector3 ColliderOffset;

    private bool collisionRight = false;
    private bool collisionLeft = false;
    private bool collisionUp = false;

    private void CheckCollisionRight(Character3DMovement followCharacter)
    {
        collisionRight = Physics.Raycast(followCharacter.transform.position, Vector3.right, 0.6f * followCharacter.transform.localScale.x);
    }

    private void CheckCollisionLeft(Character3DMovement followCharacter)
    {
        collisionLeft = Physics.Raycast(followCharacter.transform.position, Vector3.left, 0.6f * followCharacter.transform.localScale.x);
    }

    private void CheckCollisionUp(Character3DMovement followCharacter)
    {
        collisionUp = Physics.Raycast(followCharacter.transform.position + playerWidth, Vector3.up, 0.6f * followCharacter.transform.localScale.z) ||
                      Physics.Raycast(followCharacter.transform.position - playerWidth, Vector3.up, 0.6f * followCharacter.transform.localScale.z);
        Debug.DrawRay(followCharacter.transform.position, Vector3.up * 0.6f * followCharacter.transform.localScale.z, Color.red);
    }
}
