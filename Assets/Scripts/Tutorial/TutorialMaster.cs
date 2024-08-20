using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class TutorialMaster : MonoBehaviour
{
    [SerializeField]
    private RPGTalk dialogue;

    [SerializeField]
    private VideoPlayer cutsceneVideo;

    [SerializeField]
    private PlayableDirector cutscene;

    float[] secondsDialogue = new float[] {3.5f, 3.25f, 0f, 2.75f, 2f, 4.75f, 3.5f, 3.25f};

    private void Start()
    {
        TutorialToggles.LIGHT_HEIGHT = false;
        TutorialToggles.DEPTH_WALKING = false;
        TutorialToggles.SetShadowState?.Invoke(false);
    }

    public void NotifyEventTrigger(int hitEvent)
    {
        switch(hitEvent)
        {
            case 2:
            TutorialToggles.DEPTH_WALKING = true;
            break;

            case 3:

            cutscene.Play();
            break;

            case 7:
            TutorialToggles.LIGHT_HEIGHT = true;
            break;

            default:
            Debug.Log("Uneventful event");
            break;
        }

        if (hitEvent != 3)
        {
            PlayRPGTalk(hitEvent);
        }
    }

    public void ActivateShadow()
    {
        TutorialToggles.SetShadowState(true);
    }

    public void DeactivateShadow()
    {
        TutorialToggles.SetShadowState(false);
    }

    private void PlayRPGTalk(int evt)
    {
        dialogue.secondsAutoPass = secondsDialogue[evt - 1];
        dialogue.NewTalk($"tut{evt}", $"tut{evt}end");
    }

    public void PlayVideo()
    {
        cutsceneVideo.Play();
    }

    public void PreloadVideo()
    {
        cutsceneVideo.Prepare();
    }

    public void showVideo()
    {
        cutsceneVideo.GetComponent<SpriteRenderer>().sortingOrder = 1;
    }
}
