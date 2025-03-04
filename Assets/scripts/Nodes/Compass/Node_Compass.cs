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

    public void PopulateRightClickMenu() {
        CanvasUtils.DestroyChildren(UIManager.Instance.nodeOptionsWidget.transform.GetChild(0).gameObject);

        RectTransform bgTransform = UIManager.Instance.nodeOptionsWidget.transform.GetChild(0).GetComponent<RectTransform>();

        // this could be more modular, but I don't care rn
        // TODO: ?

        // we're using a size of 200 x 60 for each option, so with two options (right now) we have 200 x 120
        bgTransform.sizeDelta = new Vector2(200, 180);

        // button to open the config menu for this node
        // ----------------------------------

        GameObject editButton = Instantiate(UIPrefabs.Instance.textButtonPrefab, Vector3.zero, Quaternion.identity);

        editButton.transform.SetParent(bgTransform);

        editButton.GetComponent<TextMeshProUGUI>().text = "edit";

        // moving the button to the right spot on the widget
        editButton.transform.localPosition = 
        new Vector3(bgTransform.sizeDelta.x / 2, 0, 0) // the top-centre of the element
        + new Vector3(0, -bgTransform.sizeDelta.y / 3 * 0, 0); // the offset vector

        editButton.transform.GetChild(0).GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.OpenNodeConfig(GetComponent<NodeInteractionHandler>())
        );

        // button to delete this node
        // ----------------------------------

        GameObject deleteButton = Instantiate(UIPrefabs.Instance.textButtonPrefab, Vector3.zero, Quaternion.identity);

        deleteButton.transform.SetParent(bgTransform);

        deleteButton.GetComponent<TextMeshProUGUI>().text = "delete";

        // moving the button to the right spot on the widget
        deleteButton.transform.localPosition = 
        new Vector3(bgTransform.sizeDelta.x / 2, 0, 0) // the top-centre of the element
        + new Vector3(0, -bgTransform.sizeDelta.y / 3 * 1, 0); // the offset vector

        deleteButton.transform.GetChild(0).GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.DeleteNode(gameObject)
        );

        // button to toggle tracking of this node
        // ----------------------------------

        GameObject trackButton = Instantiate(UIPrefabs.Instance.textButtonPrefab, Vector3.zero, Quaternion.identity);

        trackButton.transform.SetParent(bgTransform);

        trackButton.GetComponent<TextMeshProUGUI>().text = "track";

        // moving the button to the right spot on the widget
        trackButton.transform.localPosition = 
        new Vector3(bgTransform.sizeDelta.x / 2, 0, 0) // the top-centre of the element
        + new Vector3(0, -bgTransform.sizeDelta.y / 3 * 2, 0); // the offset vector

        trackButton.transform.GetChild(0).GetComponent<UI_Button>().onPress.AddListener(
            () => interact.ToggleTrack()
        );
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
