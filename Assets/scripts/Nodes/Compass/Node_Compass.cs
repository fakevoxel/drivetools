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

    public TMP_InputField titleDisplay;
    public double dataValue;
    public RawImage dataDisplay;

    public NodeInteractionHandler interact;

    public void Init(string name, GameObject compassPrefab) {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Double;

        // get the title obj
        titleDisplay = CanvasUtils.SearchChildrenForName(gameObject, "title").GetComponent<TMP_InputField>();
        // not showing the title rn
        //titleDisplay.text = name;

        dataDisplay = Instantiate(compassPrefab, transform.position, Quaternion.identity).transform.GetChild(1).GetComponent<RawImage>();

        dataDisplay.transform.parent.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        xPadding = 30;
        yPadding = 30;
    }

    public void SetData(double value) {
        dataValue = value;
        dataDisplay.transform.eulerAngles = new Vector3(dataDisplay.transform.eulerAngles.x, dataDisplay.transform.eulerAngles.y, (float)value);
    }

    public void PositionUI() {
        // move the name of the node
        titleDisplay.transform.position = interact.topLeftTransform.position + new Vector3(xPadding * 4, -yPadding * 1.5f, 0);
        dataDisplay.transform.position = interact.transform.position;

        float minDimensions = (interact.leftEdge - interact.rightEdge).magnitude < 
        (interact.topEdge - interact.bottomEdge).magnitude ? 
        (interact.leftEdge - interact.rightEdge).magnitude : 
        (interact.topEdge - interact.bottomEdge).magnitude;
        dataDisplay.GetComponent<RectTransform>().sizeDelta = new Vector2(minDimensions, minDimensions);
        dataDisplay.transform.parent.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(minDimensions, minDimensions);
    }
}
