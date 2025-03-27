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

    private bool isTestMode;
    private float lastUpdateTime;
    public float updateFrequency;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Graph;

        isTestMode = false;
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
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }

    // when "test mode" is active, random values are written to the graph just to see how it looks
    public void ToggleTestMode() {
        isTestMode = !isTestMode;
    }

    void Update() {
        if (isTestMode) {
            PlotNewDataPoint(new Vector2(Time.time, Random.Range(0, 100)));
        }

        if (interact.holdType != 0 && interact.holdType != 1) {
            graph.RefreshPoints();
            graph.RefreshLineRenderer();
        }

        // changing the size of the graph to match the node size
        graph.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 
        new Vector2(transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x - 50, 
        transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y - 50);
    }

    public void UpdateData() {
        //SetData(NetworkManager.Instance.FetchNTDouble(sourceString));
    }

    public void PlotNewDataPoint(Vector2 point) {
        if (Time.time > lastUpdateTime + updateFrequency) {
            graph.PlotPoint(point);
            lastUpdateTime = Time.time;
        }
    }
}
