using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSwoop : MonoBehaviour
{
    // Start is called before the first frame update
    
    public GameObject CameraSetup;

    void Start()
    {
        CameraSetup.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
