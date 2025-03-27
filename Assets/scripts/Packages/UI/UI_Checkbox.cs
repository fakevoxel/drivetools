using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// A modular checkbox class

// -- SETUP: -- //
// Get the CanvasUtils script into the project (same folder)
public class UI_Checkbox : MonoBehaviour
{
    private bool isPressed; // Determine if the button is pressed
    public int buttonToLookFor; // left, right, or middle click

    [Space(6)]
    [Header("Interactions")]
    public UnityEvent onToggle; // Click (runs once)
    public UnityEvent onTrue; // Click and then move mouse away (runs once)
    public UnityEvent onFalse; // Hold click (repeats)

    [Space(6)]
    [Header("Color Settings")]
    public bool colorSwitch; // whether to switch colors when pressed
    public Color defaultColor;
    public Color hoverColor;

    [Space(6)]
    [Header("Image Settings")]
    public bool imageSwitch; // whether to switch colors when pressed
    public Sprite trueImage;
    public Sprite falseImage;

    void Update() {
        if (Input.GetMouseButtonDown(buttonToLookFor) && CanvasUtils.IsCursorInteract(gameObject, true)) { // Use CanvasUtils to check if the button is being pressed
            isPressed = !isPressed;
            onToggle.Invoke();
            if (isPressed) {
                onTrue.Invoke();
                GetComponent<Image>().sprite = trueImage;
            }
            else {
                onFalse.Invoke();
                GetComponent<Image>().sprite = falseImage;
            }
            if (GetComponent<UI_Value>() != null) {GetComponent<UI_Value>().value = isPressed.ToString();}
        }

        if (CanvasUtils.IsCursorInteract(gameObject, true)) { // hovering over the button but not pressing it
            if (colorSwitch) { GetComponent<Image>().color = hoverColor; }
        }
        else {
            if (colorSwitch) { GetComponent<Image>().color = defaultColor; } // default color (not interacting at all)
        }   
    }
}
