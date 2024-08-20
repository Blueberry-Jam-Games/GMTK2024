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
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {

        SceneManager.sceneLoaded += SceneLoaded;
        localSound = GetComponent<AudioSource>();

        if (instance == this && SceneManager.GetActiveScene().name != "Tutorial Level")
        {
            localSound.Play();
        }
    }

    public void PlayBGMSignal()
    {
        localSound.Play();
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main_Menu")
        {
            localSound.Stop();
            Destroy(this.gameObject);
        }
    }
}
