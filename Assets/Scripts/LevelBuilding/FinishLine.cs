using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private string NextScene;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Finish Line");
        // Probably need to disable Player input here
        BJ.SceneTransitionManager.LoadNewScene(NextScene);
    }
}
