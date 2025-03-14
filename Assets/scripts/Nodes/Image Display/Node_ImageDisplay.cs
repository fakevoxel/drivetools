using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;

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

    public Transform dataContainer;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.ImageDisplay;

        xPadding = 30;
        yPadding = 30;

        layers = new List<ImageDisplayLayer>();
        layers.Add(new ImageDisplayLayer());
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

    public void PopulateDataClass() {
        data.sourceString = sourceString;
        data.layers = layers.ToArray();
        
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

        GameObject layerTabs = Instantiate(UIPrefabs.Instance.tabsPrefab, Vector3.zero, Quaternion.identity);
        layerTabs.transform.SetParent(window.transform.GetChild(4));
        layerTabs.transform.localPosition = Vector3.zero;

        layerTabs.GetComponent<UI_Tabs>().InitializeTabs(layers.Count, 125);

        GameObject newLayerButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
        newLayerButton.transform.SetParent(window.transform.GetChild(4));
        newLayerButton.transform.localPosition = new Vector3(0, 400, 0);

        newLayerButton.GetComponent<UI_Button>().onPress.AddListener(
            () => {
                layers.Add(new ImageDisplayLayer());
                CanvasUtils.DestroyChildren(UIManager.Instance.configWindow.transform.GetChild(4).gameObject);
                this.PopulateConfigMenu(UIManager.Instance.configWindow);
            }
        );

        GameObject inputSet = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        inputSet.transform.SetParent(window.transform.GetChild(4));
        inputSet.transform.localPosition = Vector3.zero;

        inputSet.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[layers.Count];
        inputSet.GetComponent<UI_ObjectSetManager>().controllingTabs = layerTabs.GetComponent<UI_Tabs>();
        inputSet.GetComponent<UI_ObjectSetManager>().Initialize();

        GameObject imageSet = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        imageSet.transform.SetParent(window.transform.GetChild(4));
        imageSet.transform.localPosition = Vector3.zero;

        imageSet.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[layers.Count];
        imageSet.GetComponent<UI_ObjectSetManager>().controllingTabs = layerTabs.GetComponent<UI_Tabs>();
        imageSet.GetComponent<UI_ObjectSetManager>().Initialize();

        for (int i = 0; i < layers.Count; i++) {
            int j = i;

            GameObject newInputList = Instantiate(UIPrefabs.Instance.elementListPrefab, Vector3.zero, Quaternion.identity);
            newInputList.transform.SetParent(window.transform.GetChild(4));
            newInputList.transform.localPosition = new Vector3(-150, -75, 0);
            
            for (int k = 0; k < layers[i].requiredStates.Count; k++) {
                newInputList.GetComponent<UI_InputList>().AddNewElement();

                newInputList.transform.GetChild(0).GetChild(k).GetComponent<TMP_InputField>().text = layers[i].requiredStates[k];
            }

            newInputList.GetComponent<UI_InputList>().onEditElement.AddListener(
                (value) => {
                    SetState(j, newInputList.GetComponent<UI_InputList>().pressedElementIndex, value);
                }
            );
            newInputList.GetComponent<UI_InputList>().onAddNewElement.AddListener(
                () => AddNewRequiredState(j)
            );

            inputSet.GetComponent<UI_ObjectSetManager>().sets[i] = new UI_ObjectSet(new GameObject[]{newInputList});

            GameObject newImageList = Instantiate(UIPrefabs.Instance.elementListPrefab, Vector3.zero, Quaternion.identity);
            newImageList.transform.SetParent(window.transform.GetChild(4));
            newImageList.transform.localPosition = new Vector3(150, -75, 0);

            newImageList.GetComponent<UI_InputList>().inputPrefab = UIPrefabs.Instance.buttonPrefab;

            for (int k = 0; k < layers[i].images.Count; k++) {
                newImageList.GetComponent<UI_InputList>().AddNewElement();
            }

            newImageList.GetComponent<UI_InputList>().onPressElement.AddListener(
                () => {
                    UIManager.Instance.GrabAssetName((name) =>
                    SetImage(j, newImageList.GetComponent<UI_InputList>().pressedElementIndex, name));
                }
            );
            newImageList.GetComponent<UI_InputList>().onAddNewElement.AddListener(
                () => AddNewImage(j)
            );

            imageSet.GetComponent<UI_ObjectSetManager>().sets[j] = new UI_ObjectSet(new GameObject[]{newImageList});
        }

        layerTabs.GetComponent<UI_Tabs>().onChangeTabs.Invoke(layerTabs.GetComponent<UI_Tabs>().selectedTab);
    }

    public void PositionUI() {
        float minDimensions = (interact.leftEdge - interact.rightEdge).magnitude < 
        (interact.topEdge - interact.bottomEdge).magnitude ? 
        (interact.leftEdge - interact.rightEdge).magnitude : 
        (interact.topEdge - interact.bottomEdge).magnitude;

        if (dataContainer.childCount != layers.Count) {
            // repopulate the images
            CanvasUtils.DestroyChildren(dataContainer.gameObject);

            for (int i = 0; i < layers.Count; i++) {
                GameObject newImage = Instantiate(UIPrefabs.Instance.imagePrefab, Vector3.zero, Quaternion.identity);
                newImage.transform.SetParent(dataContainer);
                newImage.transform.localPosition = Vector3.zero;
            }
        }
    
        for (int i = 0; i < dataContainer.childCount; i++) {
            dataContainer.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(minDimensions, minDimensions);
            
            dataContainer.GetChild(i).GetComponent<RawImage>().texture = null;
            
            // deciding what the image is going to be based off of the NT value
            for (int j = 0; j < layers[i].images.Count; j++) {
                if (dataValue == layers[i].requiredStates[j]) {
                    dataContainer.GetChild(i).GetComponent<RawImage>().texture = AssetManager.Instance.GetImageOfName(layers[i].images[j]);
                }
            }
        }
    }

    public void AddNewRequiredState(int layerIndex) {
        layers[layerIndex].requiredStates.Add(null);
    }
    public void AddNewImage(int layerIndex) {
        layers[layerIndex].images.Add(null);
    }

    public void SetImage(int layerIndex, int stateIndex, string assetName) {
        if (layers[layerIndex].images.Count <= stateIndex) {return;}

        layers[layerIndex].images[stateIndex] = assetName;
        UIManager.Instance.assetManagerWindow.SetActive(false);
    }
    public void SetState(int layerIndex, int stateIndex, string value) {
        if (layers[layerIndex].requiredStates.Count <= stateIndex) {return;}

        layers[layerIndex].requiredStates[stateIndex] = value;
    }

    public void UpdateData() {
        SetData(NetworkManager.Instance.FetchNTDouble(sourceString).ToString());
    }
}