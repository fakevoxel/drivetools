using UnityEngine;

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
    // The hue to be used when returning the color
    public float h;
    private Transform handleTransform;

    void Start() {
        handleTransform = transform.GetChild(0);
    }

    void Update() {
        // If the cursor is interacting with the object the handle should follow it
        // This uses the CanvasUtils support script at C:\Users\maxim\Desktop\ASSETS\UI Suite\CanvasUtils.cs
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
            if (Input.GetMouseButton(0)) {
                handleTransform.position = Input.mousePosition;
            }
        }
    }
    
    // Returns a color using the hue value and the position of the handle relative to the bounds of the obj
    public Color GetColor() {
        // maximum x and y of the bounding box
        Vector2 min = new Vector2((transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2));
        // minimum x and y of the bounding box
        Vector2 max = new Vector2((transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2));

        // Figure out how far the cursor is between min and max to determine s and v (x is s, y is v)
        float s = (handleTransform.position.x - min.x) / (max.x - min.x);
        float v = (handleTransform.position.y - min.y) / (max.y - min.y);

        return Color.HSVToRGB(h, s, v);
    }

    // Set the position of the handle to represent a specific color [0 - 1]
    public void SetColor01(float _h, float _s, float _v) {
        // maximum x and y of the bounding box
        Vector2 min = new Vector2((transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2));
        // minimum x and y of the bounding box
        Vector2 max = new Vector2((transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2));

        h = _h;

        // Use linear interpolation to determine new handle position
        handleTransform.position = new Vector3(Mathf.Lerp(min.x, max.x, _s), Mathf.Lerp(min.y, max.y, _v), 0);
    }

    // Set the position of the handle to represent a specific color [0 - 255]
    public void SetColor255(float _h, float _s, float _v) {
        // maximum x and y of the bounding box
        Vector2 min = new Vector2((transform.position.x - GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y - GetComponent<RectTransform>().sizeDelta.y / 2));
        // minimum x and y of the bounding box
        Vector2 max = new Vector2((transform.position.x + GetComponent<RectTransform>().sizeDelta.x / 2), (transform.position.y + GetComponent<RectTransform>().sizeDelta.y / 2));

        h = _h / 255; // divide by 255 to convert to [0 - 1] range

        // Use linear interpolation to determine new handle position
        handleTransform.position = new Vector3(Mathf.Lerp(min.x, max.x, _s / 255), Mathf.Lerp(min.y, max.y, _v / 255), 0);
    }
}
