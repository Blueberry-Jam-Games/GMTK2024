using System.Collections;
using System.Collections.Generic;
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
    }
}
