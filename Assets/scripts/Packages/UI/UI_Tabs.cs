using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// A modular class for a tabs system (like google chrome tabs)

// -- SETUP: -- //
// Get the CanvasUtils script into the project (same folder)
public class UI_Tabs : MonoBehaviour
{
    [Header("General Settings")]
    public GameObject tabPrefab;
    public GameObject[] tabObjects;
    public int selectedTab;
    public int buttonToLookFor; // left, right, or middle click

    [Space(6)]
    [Header("Color Settings")]
    public bool colorSwitch; // whether to switch colors when pressed
    public Color defaultColor;
    public Color hoverColor;
    public Color selectedColor;

    public void InitializeTabs(int count, float spacing) {
        tabObjects = new GameObject[count];

        for (int i = 0; i < count; i++) {
            GameObject newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity);
            newTab.transform.SetParent(transform);
            newTab.transform.localPosition = Vector3.right * i * spacing;

            tabObjects[i] = newTab;
        }
    }

    public void HandleInteract() {
        for (int i = 0; i < tabObjects.Length; i++) {
            bool isHover = CanvasUtils.IsCursorInteract(tabObjects[i], true);
            bool isPressed = Input.GetMouseButtonDown(buttonToLookFor) && isHover;

            tabObjects[i].GetComponent<Image>().color = isHover ? hoverColor : defaultColor;

            if (isPressed) { // is the button pressed
                selectedTab = i;
            }
        }

        tabObjects[selectedTab].GetComponent<Image>().color = selectedColor;
    }
}
