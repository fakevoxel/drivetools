using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// A modular button class (the default unity button but better)

// -- SETUP: -- //
// 1. add the CanvasUtils script into the project
// 2. ONLY IF YOU WANT TO USE THE TOOLTIP FEATURE, add the tooltip system to the project
// 3. profit :)

public class UI_Button : MonoBehaviour
{
    private bool isPressed; // boolean for storing whether the button is pressed
    private bool isHover; // storing whether the cursor is over the button or not

    [Space(8)]
    [Header("Interaction Settings")]
    public string toolTip; // what to display on the tooltip when the button is hovered over
    private bool usingTooltip;
    private float hoverStartTime; // the time when the mouse cursor first entered the button's area
    private UnityAction disableTooltipAction; // the thing to invoke when we don't want the tooltip anymore
    public int buttonToLookFor; // left (0), right (1), or middle click (2)

    [Space(8)]
    [Header("Interaction Events")]
    public UnityEvent onHoverEnter; // invoked upon hovering over the button
    public UnityEvent whileHover; // invoked every frame the cursor is hovering over the button
    public UnityEvent onHoverExit; // invoked when the mouse cursor leaves the button's area
    [Space(4)]
    public UnityEvent onPress; // invoked upon clicking AND THEN RELEASING the button
    public UnityEvent whilePress; // invoked for every frame the button is held down
    [Space(4)]
    public UnityEvent onDrag; // invoked upon clicking, holding, and the mouse cursor leaving the button's area

    [Space(8)]
    [Header("Color Settings")]
    public bool colorSwitch; // whether to switch colors when pressed
    public Color defaultColor;
    public Color hoverColor;
    public Color pressedColor;

    void Start() {
        if (!string.IsNullOrEmpty(toolTip)) {
            usingTooltip = true;
        } else {usingTooltip = false; }
    }

    void Update() {
        HandleInteraction();
    }

    // tell the button object what to do when it doesn't want the tooltip anymore
    public void SetDisableTooltipAction(UnityAction action) {
        disableTooltipAction = action;
    }

    void OnDisable()
    {
        if (disableTooltipAction != null) {
            disableTooltipAction.Invoke();
        }
    }

    // this all runs in its own function, so I can easily change whether the button is active or not
    void HandleInteraction() {
        // update the boolean variables that trigger events
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
            if (!isHover) {
                // if we weren't hovering before, invoke onHoverEnter
                onHoverEnter.Invoke();

                hoverStartTime = Time.time;
            }
            isHover = true; // hover is set to true if the cursor is over the button
            if (Input.GetMouseButtonDown(buttonToLookFor)) {
                isPressed = true; // press is set to true if the relevant mouse button is also pressed
            }
        }
        else {
            if (isHover) {
                // if we were just hovering, invoke onHoverExit
                onHoverExit.Invoke();
                if (disableTooltipAction != null) {
                    disableTooltipAction.Invoke();
                    disableTooltipAction = null;
                }
            }

            isHover = false;
            hoverStartTime = 0;
        }

        if (isPressed) { // is the button pressed
            whilePress.Invoke(); // invoke every frame the boolean is set to true

            if (colorSwitch) { GetComponent<Image>().color = pressedColor; } // change the color if that's enabled

            if (!CanvasUtils.IsCursorInteract(gameObject, true)) {
                // when cursor leaves the button area, invoke onDrag
                onDrag.Invoke(); // Invoke the unity event that runs when you click and drag the button
                isPressed = false;
            }

            if (!Input.GetMouseButton(buttonToLookFor)) {
                // if the button is not pressed, but it was last frame, turn off the boolean and invoke onPress
                // the reason it's invoked on releasing the button is to make sure the user isn't holding the button, just a quick click
                onPress.Invoke();
                isPressed = false;
            }
        }
        else if (isHover) { // hovering over the button but not pressing it
            if (colorSwitch) { GetComponent<Image>().color = hoverColor; } // color if applicable
            whileHover.Invoke();
        }
        else {
            if (colorSwitch) { GetComponent<Image>().color = defaultColor; } // default color (not interacting at all)
        }  

        // tooltip logic
        if (isHover && Time.time > hoverStartTime + UI_TooltipSystem.Instance.displayDelay && disableTooltipAction == null && usingTooltip) {
            UI_TooltipSystem.Instance.DisplayTooltip(toolTip);
            SetDisableTooltipAction(UI_TooltipSystem.Instance.GetDisableAction());
        } 
    }

    // since the boolean is private, here's a function for checking the button state
    // not many applications are going to use this over the events like onPress, but whatever
    public bool IsPressed() {
        return isPressed;
    }
}
