using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    private AudioSource localSound;

    private static BGMManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    }

    private void Start()
    {
        localSound = GetComponent<AudioSource>();

        if (SceneManager.GetActiveScene().name != "TutorialLevel")
        {
            localSound.Play();
        }
    }

    private void Update()
    {
        
    }
}
