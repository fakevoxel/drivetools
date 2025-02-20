using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node_Double : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_Double data;
    
    public float xPadding;
    public float yPadding;

    public double dataValue;
    public TextMeshProUGUI dataDisplay;

    public NodeInteractionHandler interact;

    // the networktables name to use when fetching data
    public string sourceString;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Double;
        
        dataDisplay = CanvasUtils.SearchChildrenForName(gameObject, "data").GetComponent<TextMeshProUGUI>();
        dataDisplay.transform.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        xPadding = 30;
        yPadding = 30;

        CanvasUtils.SearchChildrenForName(gameObject, "edit button").GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.OpenNodeConfig(interact)
        );
    }

    public void SetSourceString(string input) {
        sourceString = input;
    }
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
    }

    public void SetData(double value) {
        dataValue = value;
        dataDisplay.text = value.ToString();
    }

    public void PositionUI() {
        // move the name of the node
        dataDisplay.transform.position = interact.transform.position + new Vector3(0, yPadding, 0);
    }

    public void PopulateDataClass() {
        data.sourceString = sourceString;

        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y
        );
    }

    public void UpdateData() {
        SetData(NetworkManager.Instance.FetchNTDouble(sourceString));
    }
}
