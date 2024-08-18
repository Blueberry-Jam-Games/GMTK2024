using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraPlayer : MonoBehaviour
{
    [Tooltip("Put the playerobject here")]
    public Rigidbody body;
    //public Character3DGround player;
    public CinemachineVirtualCamera cam;
    
    private Cinemachine3rdPersonFollow camComponent;
   
    // Start is called before the first frame update
    void Start()
    {
        camComponent = cam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        // if(body.velocity.x >0){
        //    camComponent.ShoulderOffset.x = 1; 
        // }else if (body.velocity.x <0){
        //     camComponent.ShoulderOffset.x = -1;
        // }else{
        //     camComponent.ShoulderOffset.x = 0;
        // }
        //if(player.GetOnGround())
        if(camComponent.ShoulderOffset.x>(body.velocity.x/3.5)+0.5){
            camComponent.ShoulderOffset.x -= (float)0.01;
        }else if (camComponent.ShoulderOffset.x<(body.velocity.x/3.5)-0.5){
            camComponent.ShoulderOffset.x += (float)0.01;
        }
        
    }
}
