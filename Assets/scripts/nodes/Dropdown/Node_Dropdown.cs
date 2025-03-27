using System.Reflection;
using TMPro;
using UnityEngine;

// WIP
public class Node_Dropdown : MonoBehaviour
{
    public GenericNodeData genericData;
    public NodeData_Dropdown data;
    
    public float xPadding;
    public float yPadding;

    public double dataValue;
    public TMP_Dropdown dataDisplay;
     public TextMeshProUGUI titleDisplay;

    public NodeInteractionHandler interact;

    // the networktables name to use when fetching data
    public string sourceString;
    // the name of the node, seen by the user
    public string titleString;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Dropdown;
        
        dataDisplay = CanvasUtils.SearchChildrenForName(gameObject, "data").GetComponent<TMP_Dropdown>();
        dataDisplay.transform.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        titleDisplay = CanvasUtils.SearchChildrenForName(gameObject, "title").GetComponent<TextMeshProUGUI>();
        titleDisplay.transform.SetParent(transform);
        titleDisplay.gameObject.name = "title";

        xPadding = 30;
        yPadding = 30;
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

    public void PositionUI() {
        // move the name of the node
        titleDisplay.transform.position = interact.transform.position + new Vector3(0, yPadding, 0);
        dataDisplay.transform.position = interact.transform.position + new Vector3(0, -yPadding, 0);

        titleDisplay.text = titleString;
    }

    public void PopulateDataClass() {
        data.sourceString = sourceString;
        data.titleString = titleString;

        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }
}