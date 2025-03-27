using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node_String : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_String data;
    
    public float xPadding;
    public float yPadding;

    public GameObject textPrefab;
    public TMP_InputField titleDisplay;
    public string dataValue;
    public TextMeshProUGUI dataDisplay;

    public NodeInteractionHandler interact;

    public void Init(string name, GameObject textPrefab) {
        this.textPrefab = textPrefab;

        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.String;

        // get the title obj
        titleDisplay = CanvasUtils.SearchChildrenForName(gameObject, "title").GetComponent<TMP_InputField>();
        titleDisplay.text = name;
        
        dataDisplay = Instantiate(textPrefab, titleDisplay.transform.position + new Vector3(xPadding, -yPadding, 0), Quaternion.identity).GetComponent<TextMeshProUGUI>();
        dataDisplay.transform.SetParent(transform);
        dataDisplay.gameObject.name = "data";

        xPadding = 30;
        yPadding = 30;
    }

    public void SetData(string value) {
        dataValue = value;
        dataDisplay.text = value;
    }

    public void PositionUI() {
        // move the name of the node
        titleDisplay.transform.position = interact.transform.position + new Vector3(0, yPadding, 0);
        dataDisplay.transform.position = titleDisplay.transform.position + new Vector3(0, -yPadding - titleDisplay.GetComponent<RectTransform>().sizeDelta.y, 0);
    }
}
