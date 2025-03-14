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

    public UnityEvent<int> onChangeTabs;

    public bool runOnUpdate;

    [Space(6)]
    [Header("Color Settings")]
    public bool colorSwitch; // whether to switch colors when pressed
    public Color defaultColor;
    public Color hoverColor;
    public Color selectedColor;

    void Start() {
        // we call this event here in case anything has been done in the editor,
        // this will return the tabs to their default state
        if (onChangeTabs != null) {
            onChangeTabs.Invoke(selectedTab);
        }
    }

    void Update() {
        if (runOnUpdate) {
            HandleInteract();
        }
    }

    public void InitializeTabs(int count, float spacing) {
        CanvasUtils.DestroyChildren(gameObject);

        tabObjects = new GameObject[count];

        for (int i = 0; i < count; i++) {
            GameObject newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity);
            newTab.transform.SetParent(transform);
            newTab.transform.localPosition = Vector3.right * i * spacing;

            tabObjects[i] = newTab;

            newTab.transform.localScale = Vector3.one;
        }
    }

    public void HandleInteract() {
        for (int i = 0; i < tabObjects.Length; i++) {
            bool isHover = CanvasUtils.IsCursorInteract(tabObjects[i], true);
            bool isPressed = Input.GetMouseButtonDown(buttonToLookFor) && isHover;

            tabObjects[i].GetComponent<Image>().color = isHover ? hoverColor : defaultColor;

            if (isPressed) { // is the button pressed
                selectedTab = i;

                if (onChangeTabs != null) {
                    onChangeTabs.Invoke(i);
                }
            }
        }

        tabObjects[selectedTab].GetComponent<Image>().color = selectedColor;
    }
}
