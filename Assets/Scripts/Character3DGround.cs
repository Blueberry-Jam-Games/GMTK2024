using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character3DGround : MonoBehaviour
{
    private bool onGround;

    [Header("Collider Settings")]
    [SerializeField][Tooltip("Length of the ground-checking collider")]
    private float GroundLength = 0.52f;

    [SerializeField][Tooltip("Half Player Length")]
    private float HalfHeight = 0.5f;

    [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 ColliderOffset;

    [Header("Layer Masks")]
    [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask GroundLayer;

    private void FixedUpdate()
    {
        onGround = Physics.Raycast(transform.position + ColliderOffset, Vector3.down, GroundLength * transform.localScale.y, GroundLayer) || Physics.Raycast(transform.position - ColliderOffset, Vector3.down, GroundLength * transform.localScale.y, GroundLayer);
    }

    public bool AcceleratedGroundCheck(float sunYVelocity, out RaycastHit hit)
    {
        if (Physics.Raycast(transform.position + ColliderOffset, Vector3.down, out hit, HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity), GroundLayer))
        {
            onGround = true;
        }
        else if (Physics.Raycast(transform.position - ColliderOffset, Vector3.down, out hit, HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity), GroundLayer))
        {
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        Debug.DrawRay(transform.position + ColliderOffset, Vector3.down * (HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity)), Color.blue);
        Debug.DrawRay(transform.position - ColliderOffset, Vector3.down * (HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity)), Color.gray);
        return onGround;
    }

    //Send ground detection to other scripts
    public bool GetOnGround() { return onGround; }
}
