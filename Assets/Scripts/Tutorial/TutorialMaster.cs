using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMaster : MonoBehaviour
{
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
            case 1:
            // stuff
            break;

            case 2:
            // stuff
            break;

            case 3:
            // stuff
            break;

            case 4:
            // stuff
            break;

            case 5:
            // stuff
            break;

            case 6:
            // stuff
            break;

            case 7:
            // stuff
            break;

            case 8:
            // stuff
            break;

            default:
            TutorialToggles.DEPTH_WALKING = true;
            break;
        }
    }
}
