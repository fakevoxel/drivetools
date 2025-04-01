using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// this is a modular class for sliders, 
// it's part of my custom UI suite that allows for more in-depth changes by me 
// (which is why I'm not using the default slider class)

// there is some weird integration with the UI_Value class, 
// which to be fair needs to be re-written

// SETUP:
// you need a gameobject with this class and an image class
// you need the 0th child to have an image class
// for this object anchor AND pivot have to be middle-left
// for the child (handle) they have to be middle-center
public class UI_Slider : MonoBehaviour
{   
    [Space(8)]
    [Header("General")]
    // what the slider is currently reading [0..1]
    // not hiding this in inspector because it's useful to see
    public float value;
    // this allows sliders to be manipulated with left, right, or even middle (?) click
    public int buttonToLookFor;

    // TODO: add some sprite-swapping capabilities?
    [Space(8)]
    [Header("Color Settings")]
    public bool lerpColor; // whether to interpolate the color of the slider or not
    public Color zeroColor; // the color to use when the slider is at default end
    public Color oneColor; // the color to use when the slider is at the other end

    [Space(8)]
    [Header("Events")]

    // invoked EVERY FRAME that you hold the handle of the slider
    public UnityEvent whileInteracting;
    // invoked ONCE when you let go of the slider
    public UnityEvent onEndInteraction;

    // private variables
    // ---------------------

    // this is set to RectTransform.sizeDelta on Awake(), used to know how to interpolate
    private Vector2 defaultScale;
    // the transform of the handle object that the user interacts with
    private Transform handleTransform;
    // keeping track of whether the cursor is over the handle and the mouse button is down
    private bool isHandleHeld;

    // all this needs to run on Awake(), 
    // because there can be scripts that run on Start() (after Awake()),
    // that require the handleTransform variable among other things

    // split into 2 functions so I can force-init
    // -----------
    void Awake()
    {
        Initialize();
    }
    public void Initialize() {
        if (GetComponent<UI_Value>() != null) {GetComponent<UI_Value>().value = value.ToString();}

        defaultScale = GetComponent<RectTransform>().sizeDelta;
        handleTransform = transform.GetChild(0);
    }
    // -----------

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

            // invoke this event every frame
            whileInteracting.Invoke();
        }
        else {  
            // boolean check used to make sure the event is only invoked once
            if (isHandleHeld) {
                onEndInteraction.Invoke();
            }
            isHandleHeld = false;
        }

        // updating the value based on where the handle is
        value = (handleTransform.position - transform.position).magnitude / defaultScale.x;

        // weird stuff for the UI_Value class
        if (GetComponent<UI_Value>() != null) {GetComponent<UI_Value>().value = value.ToString();}

        // only update the color if that setting is enabled
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
