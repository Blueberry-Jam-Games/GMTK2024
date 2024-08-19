using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField]
    private int triggerEvent;
    private bool eventTriggered = false;

    private void OnTriggerEnter()
    {
        GameObject tutorialMaster = GameObject.FindWithTag("TutorialMaster");
        TutorialMaster tm = tutorialMaster.GetComponent<TutorialMaster>();

        if (!eventTriggered)
        {
            eventTriggered = true;
            Debug.Log($"Trigger entered for {name}, firing event {triggerEvent}");
            BJ.Coroutines.DoNextFrame(() =>
            {
                tm.NotifyEventTrigger(triggerEvent);
            });
        }
    }
}
