using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningWheel : MonoBehaviour
{
    public bool spin = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(spin)
        {
            Vector3 rotation = new Vector3(0, transform.rotation.y + 41, 0);
            transform.Rotate(rotation);
        }
    }
}
