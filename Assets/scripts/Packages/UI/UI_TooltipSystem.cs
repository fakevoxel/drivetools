using TMPro;
using UnityEngine;
using UnityEngine.Events;

// a central script for managing tooltips for UI components
// the idea is the dev defines the tooltip message over on the components side, 
// and thanks to the singleton implementation they can talk to this script without needing a reference

// create a gameObject with a child object that has a TextMeshProUGUI component (name it text) and whatever else
// MAKE SURE IT'S PUT UNDER THE CANVAS OBJECT 
// then assign tooltipObj to this new gameObject

public class UI_TooltipSystem : MonoBehaviour
{
    private static UI_TooltipSystem _instance;

    public static UI_TooltipSystem Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("Duplicate UIManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake() {
        Instance = this;
    }

    void Start() {
        Setup();

        // configure tooltips to show after 3 seconds of hovering over the thing
        displayDelay = 1;
    }   

    // makes sure the tooltip object has all the necessary components
    void Setup() {
        // dealing with edge cases and logging any necessary warnings
        if (tooltipObj == null) {
            Debug.LogWarning("You forgot to the tooltip object to the tooltip system!");
        }
        else if (CanvasUtils.SearchChildrenForName(tooltipObj.gameObject, "text") == null) {
            Debug.LogWarning("You forgot to add the text object to the tooltip object!");
        }
        else if (CanvasUtils.SearchChildrenForName(tooltipObj.gameObject, "text").GetComponent<TextMeshProUGUI>() == null) {
            Debug.LogWarning("The text object has no text component! Why??");
        }
        else {
            // define the text object
            displayText = CanvasUtils.SearchChildrenForName(tooltipObj.gameObject, "text").GetComponent<TextMeshProUGUI>();
        }
    }

    // reference to the tooltip display, 
    // not using prefabs for this one cuz there's just 1
    public Transform tooltipObj;
    public TextMeshProUGUI displayText;

    public float displayDelay;

    private int numberOfActiveTooltipObjects;

    void Update() {
        // in theory it should never be negative, but just to be safe...
        if (numberOfActiveTooltipObjects <= 0) {
            // the idea here is that once no object wants a tooltip anymore, we close it
            // I like this better than closing it from the component side of things

            HideTooltip();
            displayText.autoSizeTextContainer = false;  
        }
        else {
            displayText.autoSizeTextContainer = true;   
            // move the object to the mouse cursor
            tooltipObj.transform.position = Input.mousePosition + new Vector3(5, -5, 0);
            CanvasUtils.SearchChildrenForName(tooltipObj.gameObject, "bg").GetComponent<RectTransform>().sizeDelta = 
            displayText.GetComponent<RectTransform>().sizeDelta;
        }
    }

    // you should not call this function from any UI_ scripts, there is the GetDisableAction function instead
    public void HideTooltip() {
        tooltipObj.gameObject.SetActive(false);
    }

    // one-stop shop for showing a tooltip to the users
    public void DisplayTooltip(string msg) {
        if (tooltipObj == null || displayText == null) {
            Debug.LogWarning("Cannot display tooltip! You forgot to assign an object to the tooltip system!");
        }

        tooltipObj.gameObject.SetActive(true);

        // TODO: proper scaling and such

        // we don't move the object here, this is done inside of Update()

        // set the text display to, well, display the relevant text
        displayText.text = msg;

        // keep track of this object using a counter
        numberOfActiveTooltipObjects++;
    }

    public UnityAction GetDisableAction() {
        return () => {numberOfActiveTooltipObjects--;};
    }
}
