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
}
