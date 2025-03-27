using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Node_Compass : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_Compass data;
    
    public float xPadding;
    public float yPadding;
    public double dataValue;
    public RawImage dataDisplay;

    public NodeInteractionHandler interact;
    // the networktables name to use when fetching data
    public string sourceString;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Compass;

        xPadding = 30;
        yPadding = 30;
    }

    public void Init(string name, GameObject compassPrefab) {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Double;

        dataDisplay = Instantiate(compassPrefab, transform.position, Quaternion.identity).transform.GetChild(1).GetComponent<RawImage>();

        dataDisplay.transform.parent.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        xPadding = 30;
        yPadding = 30;
    }

    public void PopulateDataClass() {
        data.sourceString = sourceString;

        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }

    public void SetData(double value) {
        dataValue = value;
        dataDisplay.transform.eulerAngles = new Vector3(dataDisplay.transform.eulerAngles.x, dataDisplay.transform.eulerAngles.y, (float)value);
    }

    public void SetSourceString(string input) {
        sourceString = input;
    }
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
    }

    public void UpdateData() {
        SetData(NetworkManager.Instance.FetchNTDouble(sourceString));
    }

    public void PositionUI() {
        // move the name of the node
        dataDisplay.transform.position = interact.transform.position;

        float minDimensions = (interact.leftEdge - interact.rightEdge).magnitude < 
        (interact.topEdge - interact.bottomEdge).magnitude ? 
        (interact.leftEdge - interact.rightEdge).magnitude : 
        (interact.topEdge - interact.bottomEdge).magnitude;
        dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(minDimensions, minDimensions);
        dataDisplay.transform.parent.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(minDimensions, minDimensions);
    }
}
