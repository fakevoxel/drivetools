using System.Reflection;
using TMPro;
using UnityEngine;

public class Node_Double : MonoBehaviour
{
    public GenericNodeData genericData;
    public NodeData_Double data;
    
    public float xPadding;
    public float yPadding;

    public double dataValue;
    public TextMeshProUGUI dataDisplay;
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
        interact.nodeType = (int)NodeType.Double;
        
        dataDisplay = CanvasUtils.SearchChildrenForName(gameObject, "data").GetComponent<TextMeshProUGUI>();
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

    public void SetData(double value) {
        dataValue = value;
        dataDisplay.text = value.ToString();
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

    public void PopulateConfigMenu(GameObject window) {
        // double nodes are simple, they only have a source string and a title

        // create an input field for the source string
        GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);

        // parent it to the window
        sourceInput.transform.SetParent(window.transform.GetChild(4));
        // zero it
        sourceInput.transform.localPosition = new Vector3(0, 0, 0);
        
        // we set the text of the input field to show the user the current source string
        sourceInput.GetComponent<TMP_InputField>().text = sourceString;
        // when the user finishes editing, change the source string of the double node
        sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        SetSourceString);

        // create an input field for the title string
        GameObject titleInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
        // parent it to the window
        titleInput.transform.SetParent(window.transform.GetChild(4));
        // zero it
        titleInput.transform.localPosition = new Vector3(0, -100, 0);
        // we set the text of the input field to show the user the current source string
        titleInput.GetComponent<TMP_InputField>().text = titleString;
            // when the user finishes editing, change the source string of the double node
        titleInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        SetTitleString);
    }

    public void UpdateData() {
        SetData(NetworkManager.Instance.FetchNTDouble(sourceString));
    }
}
