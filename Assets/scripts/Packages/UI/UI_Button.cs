using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// A modular button class (the default unity button but better)

// -- SETUP: -- //
// Get the CanvasUtils script into the project (same folder)
public class UI_Button : MonoBehaviour
{
    private bool isPressed; // Determine if the button is pressed
    public int buttonToLookFor; // left, right, or middle click

    [Space(6)]
    [Header("Interactions")]
    public UnityEvent onPress; // Click (runs once)
    public UnityEvent onDrag; // Click and then move mouse away (runs once)
    public UnityEvent whilePress; // Hold click (repeats)

    [Space(6)]
    [Header("Color Settings")]
    public bool colorSwitch; // whether to switch colors when pressed
    public Color defaultColor;
    public Color hoverColor;
    public Color pressedColor;

    void Update() {
        if (Input.GetMouseButtonDown(buttonToLookFor) && CanvasUtils.IsCursorInteract(gameObject, true)) { // Use CanvasUtils to check if the button is being pressed
            isPressed = true;
        }

        if (isPressed) { // is the button pressed
            whilePress.Invoke(); // Invoke the event that runs when the button is held

             if (colorSwitch) { GetComponent<Image>().color = pressedColor; }
        }
        else if (CanvasUtils.IsCursorInteract(gameObject, true)) { // hovering over the button but not pressing it
            if (colorSwitch) { GetComponent<Image>().color = hoverColor; }
        }
        else {
            if (colorSwitch) { GetComponent<Image>().color = defaultColor; } // default color (not interacting at all)
        }   

        if (!Input.GetMouseButton(buttonToLookFor)) {
            if (isPressed && CanvasUtils.IsCursorInBounds(gameObject, true)) { onPress.Invoke(); } // when you release the cursor trigger the onPressed event
            isPressed = false;

            // one issue is that when holding down the button both onPressed and whilePressed are called, there may be a better way to do this
        }

        if (isPressed && !CanvasUtils.IsCursorInteract(gameObject, true)) { // if you press and hold then the cursor leaves the bounds of the button
            onDrag.Invoke(); // Invoke the unity event that runs when you click and drag the button
            isPressed = false;
        }
    }
}
