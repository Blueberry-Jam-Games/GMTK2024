using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_NEXT_REMOVETHIS : MonoBehaviour
{
    [SerializeField] private string NextScene;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            BJ.SceneTransitionManager.LoadNewScene(NextScene);
        }
    }
}
