using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ConditionalVideo : MonoBehaviour
{
    private VideoPlayer cutsceneVideo;

    // private string GithubURL = "https://github.com/Blueberry-Jam-Games/GMTK2024/raw/main/Assets/Art/BackgroundScreen/GameIntroVideo.mp4";

    private string RelativeURL = "GameIntroVideo.mp4";

    private void Start()
    {
        cutsceneVideo = GetComponent<VideoPlayer>();

// #if UNITY_WEBGL && !UNITY_EDITOR
//         cutsceneVideo.url = RelativeURL;
// #else
//         cutsceneVideo.url = GithubURL;
// #endif

        string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, RelativeURL);
        Debug.Log("Streaming Path");
        cutsceneVideo.url = streamingPath;

        cutsceneVideo.Prepare();
    }
}
