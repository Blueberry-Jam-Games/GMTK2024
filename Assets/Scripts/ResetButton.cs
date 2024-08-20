using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetButton : MonoBehaviour
{
    private Button localButton;

    private void Start()
    {
        localButton = GetComponent<Button>();
        localButton.onClick.AddListener(ButtonPressed);
    }

    private void ButtonPressed()
    {
        GameObject movementRoot = GameObject.FindWithTag("MovementRoot");
        PairedMovement movement = movementRoot.GetComponent<PairedMovement>();
        movement.KillPlayer();
    }
}
