using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraPlayer : MonoBehaviour
{
    //[Tooltip("Put the playerobject here")]
    
    //public Rigidbody body;
    //public Character3DGround player;
    public CinemachineVirtualCamera cam;
    
    private Cinemachine3rdPersonFollow camComponent;

    public double lookAhead; 
   
    public float stepSize;
   
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
        
        if((Input.GetAxis("Horizontal") >0)&&
        (camComponent.ShoulderOffset.x<lookAhead)){
            camComponent.ShoulderOffset.x += stepSize;
        }
        if ((Input.GetAxis("Horizontal") <0)&&
        (camComponent.ShoulderOffset.x>(-lookAhead))){
            camComponent.ShoulderOffset.x -= stepSize;
        }
        if(Input.GetAxis("Horizontal")==0){
            if(camComponent.ShoulderOffset.x<0)
                camComponent.ShoulderOffset.x += stepSize;
            else if (camComponent.ShoulderOffset.x>0)
                camComponent.ShoulderOffset.x -= stepSize;

        }
        
    }
}
