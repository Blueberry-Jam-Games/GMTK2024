#define RPGTalk_TMP

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

    [SerializeField]
    private SpinningWheel wheel;

    float[] secondsDialogue = new float[] {4f, 3.25f, 0f, 2.75f, 2f, 4.75f, 3.5f, 3.25f};

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
            if (hitEvent == 1)
            {
                BJ.Coroutines.DoInSeconds(3f, () => PlayRPGTalk(hitEvent));
            }
            else
            {
                PlayRPGTalk(hitEvent);
            }
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
        wheel.spin = true;
    }

    public void PreloadVideo()
    {
        cutsceneVideo.Prepare();
    }

    public void showVideo()
    {
        cutsceneVideo.GetComponent<SpriteRenderer>().sortingOrder = -1;
    }

    public void disableControls()
    {
    TutorialToggles.DEPTH_WALKING = false;
    TutorialToggles.LEFT_RIGHT = false;
    }

    public void enableControls()
    {
    TutorialToggles.DEPTH_WALKING = true;
    TutorialToggles.LEFT_RIGHT = true;
    }
}
