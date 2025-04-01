using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

// TextDisplay nodes are used for handling the basic data types that are viewed as text (double and string)
// booleans are not used here! they are displayed with ImageDisplay nodes

// so basically showing raw data

// --------------
// I could have separate double and string node classes, 
// but the code would be basically copy and pasted
// also we already have a lot of node classes

// so instead you just use  a TextDisplay node,
// and set the mode to either double or string

// its a much cleaner system
// -------------------

// the different modes (types of data) that the node can use
// is this overkill? yeah prolly but I like enums
public enum NodeDisplayMode {
    Double,
    String,
    Boolean, // usually booleans will be displayed using an ImageDisplay node, but TextDisplay works too (it'll just say "true" or "false")
}

public class Node_TextDisplay : MonoBehaviour
{
    // the values that get written to disk
    // -------------
    // boring stuff like position, scale data
    public GenericNodeData genericData;
    // specific stuff like the title and source string
    public NodeData_TextDisplay data;
    // ---------

    // the data value is stored as a string, 
    // because doubles can be strings and not the other way around
    public string dataValue;
    // the mode (rn either Double or Boolean or String, see NodeDisplayMode) that the node is using rn
    public int mode;

    // UI components for displaying the title and data
    public TextMeshProUGUI dataDisplay;
    public TextMeshProUGUI titleDisplay;
    // --------

    // class for handling dragging, re-sizing, etc.
    // (the boring stuff)
    public NodeInteractionHandler interact;

    // the networktables name to use when fetching data
    public string sourceString;
    // the name of the node, seen by the user
    public string titleString;
    // dropdown menu that the user uses to select data mode
    public TMP_Dropdown modeDropdown;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.TextDisplay;
        
        dataDisplay = CanvasUtils.SearchChildrenForName(gameObject, "data").GetComponent<TextMeshProUGUI>();
        dataDisplay.transform.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        titleDisplay = CanvasUtils.SearchChildrenForName(gameObject, "title").GetComponent<TextMeshProUGUI>();
        titleDisplay.transform.SetParent(transform);
        titleDisplay.gameObject.name = "title";

        SetTitleString("New Node");
    }

    public void SetSourceString(string input) {
        sourceString = input;
    }
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
    }
    
    public void SetTitleString(string input) {
        titleString = input;
    }
    public void SetTitleString(TMP_InputField input) {
        SetTitleString(input.text);
    }

    public void SetData(string value) {
        dataValue = value;
        dataDisplay.text = value.ToString();
    }

    public void PositionUI() {
        // move the name of the node
        titleDisplay.transform.position = interact.transform.position + new Vector3(0, 30, 0);
        dataDisplay.transform.position = interact.transform.position + new Vector3(0, -30, 0);

        titleDisplay.text = titleString;

        if (modeDropdown != null) {
            mode = modeDropdown.value;
        }
    }

    public void PopulateDataClass() {
        data.sourceString = sourceString;
        data.titleString = titleString;
        data.mode = mode;

        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }

    public void PopulateConfigMenu(GameObject window) {
        // double nodes are simple, they only have a source string and a title

        window.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Configure Node: Text Display";

        // dropdown for changing the mode (data type to use)
        // --------------
        GameObject modeSelector = Instantiate(UIPrefabs.Instance.dropDownPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        modeSelector.transform.SetParent(window.transform.GetChild(4));
        modeSelector.transform.localPosition = new Vector3(0, 125, 0);

        modeSelector.GetComponent<TMP_Dropdown>().ClearOptions();

        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();

        optionData.Add(new TMP_Dropdown.OptionData("Double"));
        optionData.Add(new TMP_Dropdown.OptionData("String"));
        optionData.Add(new TMP_Dropdown.OptionData("Boolean"));

        modeSelector.GetComponent<TMP_Dropdown>().AddOptions(optionData);

        modeDropdown = modeSelector.GetComponent<TMP_Dropdown>();
        modeDropdown.SetValueWithoutNotify(mode);
        // --------------

        // title string input field
        // =====================
        // label
        // ---------------
        GameObject titleLabel = Instantiate(UIPrefabs.Instance.textPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        titleLabel.transform.SetParent(window.transform.GetChild(4));
        titleLabel.transform.localPosition = new Vector3(0, 50, 0);

        titleLabel.GetComponent<TextMeshProUGUI>().text = "Title:";
        // ---------------
        GameObject titleInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        titleInput.transform.SetParent(window.transform.GetChild(4));
        titleInput.transform.localPosition = new Vector3(0, 0, 0);
        // we set the text of the input field to show the user the current source string
        titleInput.GetComponent<TMP_InputField>().text = titleString;
            // when the user finishes editing, change the source string of the double node
        titleInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        SetTitleString);
        // =====================

        // source string input field
        // =====================
        // label
        // ---------------
        GameObject sourceLabel = Instantiate(UIPrefabs.Instance.textPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        sourceLabel.transform.SetParent(window.transform.GetChild(4));
        sourceLabel.transform.localPosition = new Vector3(0, -100, 0);

        sourceLabel.GetComponent<TextMeshProUGUI>().text = "NT Address:";
        // ---------------
        GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        sourceInput.transform.SetParent(window.transform.GetChild(4));
        sourceInput.transform.localPosition = new Vector3(0, -150, 0);
        
        // we set the text of the input field to show the user the current source string
        sourceInput.GetComponent<TMP_InputField>().text = sourceString;
        // when the user finishes editing, change the source string of the double node
        sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        SetSourceString);
        // =====================
    }

    // functions for getting the data value as its actual type (instead of string)
    public double GetValueAsDouble() {
        double parsedValue;

        if (mode == (int)NodeDisplayMode.Double && double.TryParse(dataValue, out parsedValue)) {
            return parsedValue;
        }
        else {
            return NetworkManager.Instance.defaultDouble;
        }
    }
    // sometimes you want a float, not a double?
    public float GetValueAsFloat() {
        float parsedValue;

        if (mode == (int)NodeDisplayMode.Double && float.TryParse(dataValue, out parsedValue)) {
            return parsedValue;
        }
        else {
            return (float)NetworkManager.Instance.defaultDouble;
        }
    }
    // for boolean mode
    public bool GetValueAsBoolean() {
        bool toReturn = false;

        if (mode == (int)NodeDisplayMode.Boolean) {
            if (dataValue == true.ToString()) {
                toReturn = true;
            }
            else if (dataValue == false.ToString()) {
                toReturn = false;
            }
        }
        
        return toReturn;
    }
    // if the mode is string, we can just return whatever dataValue is, no parsing
    public string GetValueAsString() {
        return dataValue;
    }

    // the function called by NetworkManager to update the data value, based on the mode it looks for different data types
    public void UpdateData() {
        if (mode == (int)NodeDisplayMode.Double) {
            dataValue = NetworkManager.Instance.FetchNTDouble(sourceString).ToString();
        }
        else if (mode == (int)NodeDisplayMode.String) {
            // no need to cast here, it's already a string
            dataValue = NetworkManager.Instance.FetchNTString(sourceString);
        } else if (mode == (int)NodeDisplayMode.Boolean) {
            // no need to cast here, it's already a string
            dataValue = NetworkManager.Instance.FetchNTBoolean(sourceString).ToString();
        }
    }
}
