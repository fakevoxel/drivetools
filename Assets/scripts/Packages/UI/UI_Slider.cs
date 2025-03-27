using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// SETUP:
// you need a gameobject with this class and an image class
// you need the 0th child to have an image class
// for this object anchor AND pivot have to be middle-left
// for the child (handle) they have to be middle-center
public class UI_Slider : MonoBehaviour
{   
    // The resulting value of the slider
    public float value;
    public int buttonToLookFor;

    [Space(6)]
    [Header("Color Settings")]
    public bool lerpColor;
    public Color zeroColor;
    public Color oneColor;

    public UnityEvent onUpdateSlider;
    // invoked ONCE when you let go of the slider
    public UnityEvent onEndInteraction;

    private Vector2 defaultScale;
    private Transform handleTransform;
    private bool isHandleHeld;

    void Awake() {
        if (GetComponent<UI_Value>() != null) {GetComponent<UI_Value>().value = value.ToString();}

        defaultScale = GetComponent<RectTransform>().sizeDelta;
        handleTransform = transform.GetChild(0);
    }

    void Update() {
        if (GetComponent<UI_Value>() != null) {
            if (!string.IsNullOrEmpty(GetComponent<UI_Value>().setValue)) {
                SetValue(GetComponent<UI_Value>().setValue);
                GetComponent<UI_Value>().setValue = "";
            }
        }
        if (CanvasUtils.IsCursorInteract(handleTransform.gameObject, true) && Input.GetMouseButtonDown(buttonToLookFor)) {
            isHandleHeld = true;
        }
        
        if (Input.GetMouseButton(buttonToLookFor) && isHandleHeld) {
            Vector3 newPosition = 
            Vector3.Lerp(transform.position, 
            transform.position + transform.right * defaultScale.x, 
            Mathf.Clamp(Vector3.Project((Vector3)Input.mousePosition - transform.position, transform.right).magnitude / defaultScale.x, 0, 1));

            if (Vector3.Dot(Vector3.Project((Vector3)Input.mousePosition - transform.position, transform.right), transform.right) < 0) {
                newPosition = transform.position;
            }

            handleTransform.position = newPosition;

            onUpdateSlider.Invoke();
        }
        else {
            if (isHandleHeld) {
                onEndInteraction.Invoke();
            }
            isHandleHeld = false;
        }
        value = (handleTransform.position - transform.position).magnitude / defaultScale.x;
        if (GetComponent<UI_Value>() != null) {GetComponent<UI_Value>().value = value.ToString();}

        if (lerpColor) {
            GetComponent<Image>().color = Color.Lerp(zeroColor, oneColor, Mathf.Min(value, 1));
        }
    }

    public void SetValue(float _input) {

        value = _input;

        Vector3 newPosition = 
        Vector3.Lerp(transform.position, 
        transform.position + transform.right * defaultScale.x, 
        Mathf.Clamp(_input, 0f, 1f));

        handleTransform.position = newPosition;

        // not invoking onUpdateSlider here because the user didn't move the handle
    }

    void SetValue(string _value) {
        float trueValue = float.Parse(_value);

        handleTransform.position = new Vector3(trueValue * defaultScale.x + transform.position.x, handleTransform.position.y, handleTransform.position.z);
    }
}
