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
    }

    // populating the right-click widget with options for this node
    public void PopulateRightClickMenu() {
        CanvasUtils.DestroyChildren(UIManager.Instance.nodeOptionsWidget.transform.GetChild(0).gameObject);

        RectTransform bgTransform = UIManager.Instance.nodeOptionsWidget.transform.GetChild(0).GetComponent<RectTransform>();

        // this could be more modular, but I don't care rn
        // TODO: ?

        // we're using a size of 200 x 60 for each option, so with two options (right now) we have 200 x 120
        bgTransform.sizeDelta = new Vector2(200, 120);

        GameObject editButton = Instantiate(UIPrefabs.Instance.textButtonPrefab, Vector3.zero, Quaternion.identity);

        editButton.transform.SetParent(bgTransform);

        editButton.GetComponent<TextMeshProUGUI>().text = "edit";

        // moving the button to the right spot on the widget
        editButton.transform.localPosition = 
        new Vector3(bgTransform.sizeDelta.x / 2, bgTransform.sizeDelta.y / -2, 0) // the centre of the element
        + new Vector3(0, bgTransform.sizeDelta.y / 4, 0); // the offset vector

        editButton.transform.GetChild(0).GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.OpenNodeConfig(GetComponent<NodeInteractionHandler>())
        );

        GameObject deleteButton = Instantiate(UIPrefabs.Instance.textButtonPrefab, Vector3.zero, Quaternion.identity);

        deleteButton.transform.SetParent(bgTransform);

        deleteButton.GetComponent<TextMeshProUGUI>().text = "delete";

        // moving the button to the right spot on the widget
        deleteButton.transform.localPosition = 
        new Vector3(bgTransform.sizeDelta.x / 2, bgTransform.sizeDelta.y / -2, 0) // the centre of the element
        + new Vector3(0, -bgTransform.sizeDelta.y / 4, 0); // the offset vector

        deleteButton.transform.GetChild(0).GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.DeleteNode(gameObject)
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
