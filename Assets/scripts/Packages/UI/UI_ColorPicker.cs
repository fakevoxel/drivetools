using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

// A modular UI element for picking colors

// This requires an image to represent the slider area, and another image to represent the handle
// Like this: https://i.stack.imgur.com/86N5L.png

// -- SETUP: -- //
// Get the CanvasUtils script into the project (same folder)
// Attach this script to the color picker image
// The handle needs to be the first child (index 0)

// THIS ASSET NEEDS TO BE SUPPLIED A HUE VALUE, IT ONLY HANDLES S and V
// NO SUPPORT FOR RGB AS OF NOW
public class UI_ColorPicker : MonoBehaviour
{   
    // the h, s and v values that the GetColor() method returns
    private float h;
    private float s;
    private float v;

    // the action to call when the user lets go of the handle, presumably after moving it
    public UnityEvent<Color> onEndInteraction;

    private Transform handleTransform;
    private bool isHandleHeld;

    // split this process into two separate functions so that I can force an initialization
    // --------------
    void Awake()
    {
        Initialize();
    }
    public void Initialize() {
        handleTransform = transform.GetChild(0);
    }
    // --------------

    void Update() {
        // If the cursor is interacting with the object the handle should follow it
        // This uses the CanvasUtils support script at C:\Users\maxim\Desktop\ASSETS\UI Suite\CanvasUtils.cs
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
            if (Input.GetMouseButtonDown(0)) {
                isHandleHeld = true;
            }
        }
        if (!Input.GetMouseButton(0)) {
            if (isHandleHeld) {
                onEndInteraction.Invoke(GetColor());
            }
            
            isHandleHeld = false;
        }

        if (isHandleHeld) {
            handleTransform.position = new Vector3(
                Mathf.Clamp(Input.mousePosition.x, transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2, 
                transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2),

                Mathf.Clamp(Input.mousePosition.y, transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2, 
                transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2),

                0);

            UpdateColorBasedOnHandle();
        }

        GetComponent<Image>().color = GetColor();
    }

    // set the hue of the color picker.
    // since the handle is only 2-axis, hue cannot be set using just this script
    // a colorpicker + slider combo could work, in which case the slider sets the hue here
    public void SetHue(float _h) {
        h = _h;
    }
    
    public void SetHue(UI_Slider inputSlider) {
        h = inputSlider.value;
    }

    // update the h, s, and v values using the position of the handle
    public void UpdateColorBasedOnHandle() {
        // maximum x and y of the bounding box
        Vector2 min = new Vector2(transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2, transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2);
        // minimum x and y of the bounding box
        Vector2 max = new Vector2(transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2, transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2);

        // Figure out how far the cursor is between min and max to determine s and v (x is s, y is v)
        s = (handleTransform.position.x - min.x) / (max.x - min.x);
        v = (handleTransform.position.y - min.y) / (max.y - min.y);
    }

    public void UpdateHandleBasedOnColor() {
        // maximum x and y of the bounding box
        Vector2 min = new Vector2((transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2));
        // minimum x and y of the bounding box
        Vector2 max = new Vector2((transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2));

        // Use linear interpolation to determine new handle position
        handleTransform.position = new Vector3(Mathf.Lerp(min.x, max.x, s), Mathf.Lerp(min.y, max.y, v), 0);
    }
    
    // Returns a color using the h, s, and v values
    public Color GetColor() {
        return Color.HSVToRGB(h, s, v);
    }

    public void SetColor(Color color) {
        Color.RGBToHSV(color, out h, out s, out v);

        UpdateHandleBasedOnColor();
    }

    // Set the position of the handle to represent a specific color [0 - 1]
    public void SetColor01(float _h, float _s, float _v) {
        h = _h;
        s = _s;
        v = _v;

        UpdateHandleBasedOnColor();
    }

    // Set the position of the handle to represent a specific color [0 - 255]
    public void SetColor255(float _h, float _s, float _v) {
        h = _h / 255; // divide by 255 to convert to [0 - 1] range
        s = _s / 255;
        v = _v / 255;

        UpdateHandleBasedOnColor();
    }
}
