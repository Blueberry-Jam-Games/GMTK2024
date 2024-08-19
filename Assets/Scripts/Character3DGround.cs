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
        // onGround = Physics.Raycast(transform.position + ColliderOffset, Vector3.down, GroundLength * transform.localScale.y, GroundLayer) || Physics.Raycast(transform.position - ColliderOffset, Vector3.down, GroundLength * transform.localScale.y, GroundLayer);
        // /2 because it's half extent for Physics.OverlapBox
        onGround = Physics.OverlapBox(transform.position - new Vector3(0, 1f, 0) * transform.localScale.y,
                                    Vector3.Scale(new Vector3(0.75f, 0.25f, 0.25f), transform.localScale) / 2f,
                                    Quaternion.identity, GroundLayer).Length != 0;
    }

    public bool AcceleratedGroundCheck(float sunYVelocity, out Vector3 hit)
    {
        // /2 because it's half extent for Physics.OverlapBox
        Collider[] targets = Physics.OverlapBox(transform.position - new Vector3(0, 1f, 0) * transform.localScale.y - new Vector3(0, Mathf.Abs(sunYVelocity), 0),
                            Vector3.Scale(new Vector3(0.75f, 0.25f, 0.25f), transform.localScale) / 2f,
                            Quaternion.identity, GroundLayer);
        
        onGround = targets.Length != 0;
        hit = Vector3.zero;
        if (onGround)
        {
            hit = targets[0].bounds.max;
        }
        // if (Physics.Raycast(transform.position + ColliderOffset, Vector3.down, out hit, HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity), GroundLayer))
        // {
        //     onGround = true;
        // }
        // else if (Physics.Raycast(transform.position - ColliderOffset, Vector3.down, out hit, HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity), GroundLayer))
        // {
        //     onGround = true;
        // }
        // else
        // {
        //     onGround = false;
        // }

        // Debug.DrawRay(transform.position + ColliderOffset, Vector3.down * (HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity)), Color.blue);
        // Debug.DrawRay(transform.position - ColliderOffset, Vector3.down * (HalfHeight * transform.localScale.y + Mathf.Abs(sunYVelocity)), Color.gray);
        return onGround;
    }

    //Send ground detection to other scripts
    public bool GetOnGround() { return onGround; }
}
