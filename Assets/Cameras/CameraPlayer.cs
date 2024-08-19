using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraPlayer : MonoBehaviour
{
    //[Tooltip("Put the playerobject here")]
    
    //public Rigidbody body;
    //public Character3DGround player;
    public CinemachineVirtualCamera cam;
    
    private Cinemachine3rdPersonFollow camComponent;

    public float lookAhead;
   
    public float stepSize;
   
    // Start is called before the first frame update
    void Start()
    {
        camComponent = cam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    }

    private float GetLeftRightFromStickX()
    {
        Gamepad gamepad = Gamepad.current;
        return gamepad.leftStick.ReadValue().x;
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
        
        float horizontal = GetLeftRight();
        // if(horizontal > 0 && camComponent.ShoulderOffset.x < lookAhead)
        if (horizontal > 0)
        {
            // camComponent.ShoulderOffset.x += stepSize;
            camComponent.ShoulderOffset.x = Mathf.MoveTowards(camComponent.ShoulderOffset.x, lookAhead, stepSize);
        }
        // if (horizontal < 0 && camComponent.ShoulderOffset.x > -lookAhead)
        if (horizontal < 0)
        {
            // camComponent.ShoulderOffset.x -= stepSize;
            camComponent.ShoulderOffset.x = Mathf.MoveTowards(camComponent.ShoulderOffset.x, -lookAhead, stepSize);
        }
        if (horizontal==0)
        {
            // if (camComponent.ShoulderOffset.x < 0)
            //     camComponent.ShoulderOffset.x += stepSize;
            // else if (camComponent.ShoulderOffset.x > 0)
            //     camComponent.ShoulderOffset.x -= stepSize;
            camComponent.ShoulderOffset.x = Mathf.MoveTowards(camComponent.ShoulderOffset.x, 0, stepSize);
        }
        
    }
}
