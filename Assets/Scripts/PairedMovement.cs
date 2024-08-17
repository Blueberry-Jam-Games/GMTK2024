using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PairedMovement : MonoBehaviour
{
    [Header("Key References")]
    [SerializeField]
    public Character3DMovement frontCharacter;
    [SerializeField]
    public Character3DMovement backCharacter;

    // Probably needs more, tbd
    [SerializeField]
    public GameObject sun;

    private Plane backPlane = new Plane(Vector3.back, new Vector3(0, 0, 16));
    private Plane frontPlane = new Plane(Vector3.back, Vector3.zero);

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        Character3DMovement leadCharacter = frontCharacter.chargeCharacter ? frontCharacter : backCharacter;
        Character3DMovement followCharacter = frontCharacter.chargeCharacter ? backCharacter : frontCharacter;

        if (!leadCharacter.onGround && followCharacter.onGround)
        {
            leadCharacter.SetChargeCharacter(!leadCharacter.chargeCharacter);
            followCharacter.SetChargeCharacter(!followCharacter.chargeCharacter);
        }

        leadCharacter = frontCharacter.chargeCharacter ? frontCharacter : backCharacter;
        followCharacter = frontCharacter.chargeCharacter ? backCharacter : frontCharacter;

        Vector3 sunPos = sun.transform.position;
        Vector3 direction = (leadCharacter.transform.position - sun.transform.position).normalized;

        // Debug.DrawRay(sunPos, direction, Color.yellow, 1);

        float along = 0f;

        if (followCharacter == backCharacter)
        {
            backPlane.Raycast(new Ray(sunPos, direction), out along);
        }
        else
        {
            frontPlane.Raycast(new Ray(sunPos, direction), out along);
        }

        followCharacter.transform.position = sunPos + direction * along;

        CheckCollisionRight(followCharacter);
        CheckCollisionLeft(followCharacter);
        
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
    }

    [SerializeField][Tooltip("Distance between the left and right checking colliders")] private UnityEngine.Vector3 ColliderOffset;

    private bool collisionRight = false;
    private bool collisionLeft = false;

    private void CheckCollisionRight(Character3DMovement followCharacter)
    {
        collisionRight = Physics.Raycast(followCharacter.transform.position, Vector3.right, 0.6f);
    }

    private void CheckCollisionLeft(Character3DMovement followCharacter)
    {
        collisionLeft = Physics.Raycast(followCharacter.transform.position, Vector3.left, 0.6f);
    }
}


