using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Node_Graph : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_Graph data;

    public double dataValue;

    // the networktables name to use when fetching data
    public string sourceString;
    public UI_LineGraph graph;

    private float lastValueUpdate;

    public NodeInteractionHandler interact;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Double;

        CanvasUtils.SearchChildrenForName(gameObject, "edit button").GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.OpenNodeConfig(interact)
        );
    }

    // this adds the currently read value from networktables, with the current time on the x-axis
    public void PlotCurrentValue() {
        if (graph == null) {return;}

        graph.PlotPoint(new Vector2(Time.time, (float)dataValue));
    }

    public void SetSourceString(string input) {
        sourceString = input;
    }
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
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
}
