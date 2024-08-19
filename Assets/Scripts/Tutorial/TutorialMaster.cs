using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMaster : MonoBehaviour
{
    [SerializeField]
    private RPGTalk dialogue;

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
            TutorialToggles.SetShadowState(true);
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

    private void PlayRPGTalk(int evt)
    {
        dialogue.secondsAutoPass = secondsDialogue[evt - 1];
        dialogue.NewTalk($"tut{evt}", $"tut{evt}end");
    }
}
