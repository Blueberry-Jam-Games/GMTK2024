using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character3DGround : MonoBehaviour
{
    private bool onGround;

    [Header("Collider Settings")]
    [SerializeField][Tooltip("Length of the ground-checking collider")] private float GroundLength = 0.95f;
    [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 ColliderOffset;

    [Header("Layer Masks")]
    [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask GroundLayer;

    private void FixedUpdate()
    {
        onGround = Physics.Raycast(transform.position + ColliderOffset, Vector3.down, GroundLength, GroundLayer) || Physics.Raycast(transform.position - ColliderOffset, Vector3.down, GroundLength, GroundLayer);
    }

    //Send ground detection to other scripts
    public bool GetOnGround() { return onGround; }
}
