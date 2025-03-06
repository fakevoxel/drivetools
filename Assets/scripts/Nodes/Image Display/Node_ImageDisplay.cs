using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class Node_ImageDisplay : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_ImageDisplay data;
    
    public float xPadding;
    public float yPadding;

    // the string value been taken from NT
    public string dataValue;
    // the display layers
    // things are split into layers so you can stack image files on top of each other
    public List<ImageDisplayLayer> layers;

    public NodeInteractionHandler interact;

    // the networktables name to use when fetching data
    public string sourceString;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.ImageDisplay;

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

    public void SetSourceString(string input) {
        sourceString = input;
    }
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
    }

    public void SetData(string value) {
        dataValue = value;
    }

    public void PositionUI() {
        
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

    public void PopulateConfigMenu(GameObject window) {
        // double nodes are simple, they only have a source string and a title

        // create an input field for the source string
        GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);

        // parent it to the window
        sourceInput.transform.SetParent(window.transform.GetChild(4));
        // zero it
        sourceInput.transform.localPosition = new Vector3(0, 200, 0);
        
        // we set the text of the input field to show the user the current source string
        sourceInput.GetComponent<TMP_InputField>().text = sourceString;
        // when the user finishes editing, change the source string of the double node
        sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        SetSourceString);
    }

    public void UpdateData() {
        SetData(NetworkManager.Instance.FetchNTString(sourceString));
    }
}