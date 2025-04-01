using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TOOD: make it so you can spawn an image node and tell it to always draw one image, no conditions

public class Node_ImageDisplay : MonoBehaviour
{
    public NodeData_ImageDisplay data;

    // string value been taken from NT
    // represents the 'current state', which is compared to states defined by the user
    public string dataValue;
    // the mode (rn either Double or Boolean or String, see NodeDisplayMode) that the node is using rn
    public int mode;

    // display layers
    // the system is split into layers so you can stack image files on top of each other
    // why did I do it like this? not sure, it probably won't get used
    public List<ImageDisplayLayer> layers;

    // interaction component of this node, handles the boring stuff like positioning
    public NodeInteractionHandler interact;

    // networktables name to use when fetching data
    public string sourceString;

    // the transform object that has all the image objects as its children
    // hence the name 'container'
    public Transform dataContainer;
    // dropdown menu that the user uses to select data mode
    public TMP_Dropdown modeDropdown;

    void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        // setting the name and type
        // type is very important
        interact.nodeName = name;
        interact.nodeType = (int)NodeType.ImageDisplay;

        // we ONLY REINITIALIZE THE ARRAY IF IT HAS A COUNT OF 0, because some prefabs will have pre-defined arrays
        if (layers.Count == 0) {
            // initializing the layers list (or else you get null reference exceptions)
            layers = new List<ImageDisplayLayer>();

            // adding one layer to start with (because what would u do with none?)
            // no the real reason is that the UI_Tabs component used to handle the layers can't deal with 0 tabs
            // TODO: fix that UI_Tabs problem ^^
            layers.Add(new ImageDisplayLayer());
        }
    }

    // tell the node what NT string to use
    public void SetSourceString(string input) {
        sourceString = input;
    }
    // this says 0 references but BEWARE!
    // there might be objects in the scene that call this
    public void SetSourceString(TMP_InputField input) {
        SetSourceString(input.text);
    }

    // why is this a function, this isn't object-oriented programming?
    // i mean if i ever want to run code when the data value is changed...
    // but it's changed in ONE LOCATION
    // maybe there's an object in the scene that uses this?
    public void SetData(string value) {
        dataValue = value;
    }

    // throw all the relevant variables into the NodeData_ImageDisplay class so that it can be written to disk
    // this is called in the UIManager, which controls the writing of nodes to disk
    // it is not called periodically!!
    public void PopulateDataClass() {
        data.sourceString = sourceString;
        data.layers = layers.ToArray();
        data.mode = mode;
        
        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }

    // I found the UIManager getting pretty cluttered with the code that positioned objects on the nodeConfigWindow
    // so, I moved most of the logic for different node types to their respective scripts
    // anyways this is called in UIManager when the user hits 'edit' on this node
    public void PopulateConfigMenu(GameObject window) {

        window.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Configure Node: Image Display";

        // dropdown for changing the mode (data type to use)
        // --------------
        GameObject modeSelector = Instantiate(UIPrefabs.Instance.dropDownPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        modeSelector.transform.SetParent(window.transform.GetChild(4));
        modeSelector.transform.localPosition = new Vector3(0, 200, 0);

        modeSelector.GetComponent<TMP_Dropdown>().ClearOptions();

        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();

        optionData.Add(new TMP_Dropdown.OptionData("Double"));
        optionData.Add(new TMP_Dropdown.OptionData("String"));
        optionData.Add(new TMP_Dropdown.OptionData("Boolean"));

        modeSelector.GetComponent<TMP_Dropdown>().AddOptions(optionData);

        modeDropdown = modeSelector.GetComponent<TMP_Dropdown>();
        modeDropdown.SetValueWithoutNotify(mode);
        // --------------

        GameObject sourceLabel = Instantiate(UIPrefabs.Instance.textPrefab, Vector3.zero, Quaternion.identity);
        // parent, position
        sourceLabel.transform.SetParent(window.transform.GetChild(4));
        sourceLabel.transform.localPosition = new Vector3(0, 125, 0);

        sourceLabel.GetComponent<TextMeshProUGUI>().text = "NT Address:";

        // create an input field for the source string
        GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
        sourceInput.transform.SetParent(window.transform.GetChild(4));
        sourceInput.transform.localPosition = new Vector3(0, 75, 0);
        
        // we set the text of the input field to show the user the current source string
        sourceInput.GetComponent<TMP_InputField>().text = sourceString;

        // when the user finishes editing, change the source string of the double node
        // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
        SetSourceString);

        GameObject layerTabs = Instantiate(UIPrefabs.Instance.tabsPrefab, Vector3.zero, Quaternion.identity);
        layerTabs.transform.SetParent(window.transform.GetChild(4));
        layerTabs.transform.localPosition = Vector3.zero;

        layerTabs.GetComponent<UI_Tabs>().InitializeTabs(layers.Count, 125);
        // multiple layers not supported, so we don't need to see these
        layerTabs.SetActive(false);

        // NO NEW LAYER BUTTON RIGHT NOW, MULTIPLE LAYERS NOT SUPPORTED
        // ===================================
        // GameObject newLayerButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
        // newLayerButton.transform.SetParent(window.transform.GetChild(4));
        // newLayerButton.transform.localPosition = new Vector3(0, 400, 0);

        // newLayerButton.GetComponent<UI_Button>().onPress.AddListener(
        //     () => {
        //         layers.Add(new ImageDisplayLayer());
        //         CanvasUtils.DestroyChildren(window.transform.GetChild(4).gameObject);
        //         this.PopulateConfigMenu(window);
        //     }
        // );
        // ===================================

        // UI_ObjectSetManager is a class to help with turning gameobjects on/off
        // here it's used to toggle the condition and image lists based on what layer tab is selected
        // its nice because you can link it directly to a UI_Tabs class

        // technically there could just be one object set that does both since they're toggled at the same time, but whatever

        // managing the condition (requirement) lists
        // -----------------
        GameObject inputSet = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        inputSet.transform.SetParent(window.transform.GetChild(4));
        inputSet.transform.localPosition = Vector3.zero;

        inputSet.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[layers.Count];
        inputSet.GetComponent<UI_ObjectSetManager>().controllingTabs = layerTabs.GetComponent<UI_Tabs>();
        inputSet.GetComponent<UI_ObjectSetManager>().Initialize();

        // managing the image (result) lists
        // -----------------
        GameObject imageSet = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        imageSet.transform.SetParent(window.transform.GetChild(4));
        imageSet.transform.localPosition = Vector3.zero;

        imageSet.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[layers.Count];
        imageSet.GetComponent<UI_ObjectSetManager>().controllingTabs = layerTabs.GetComponent<UI_Tabs>();
        imageSet.GetComponent<UI_ObjectSetManager>().Initialize();

        // each layer has a list of states, and a list of images
        // so, we go through each layer and define those
        for (int i = 0; i < layers.Count; i++) {
            // unity has this tricky thing where if we supply an indexer variable (the counter in a loop) to a unityaction,
            // it will use the latest value, not the one it was when it was applied,
            // so we have to create a local variable that gets re-defined every time
            // if we didn't do this, everything that uses i would be equal to layers.Count instead of [0, 1, 2, ....]
            int j = i;

            // UI_InputList (also reffered to as element list) is basically a row (or column) of buttons that you can add to
            // they're used here for the required states, and the resulting images

            // this element list is for the required states
            GameObject newInputList = Instantiate(UIPrefabs.Instance.elementListPrefab, Vector3.zero, Quaternion.identity);
            newInputList.transform.SetParent(window.transform.GetChild(4));
            newInputList.transform.localPosition = new Vector3(-150, -50, 0);

            newInputList.GetComponent<UI_InputList>().spacing = 100;
        
            // making sure there is a number of buttons equal to the number of states in this list
            // otherwise when the user added states, then closed and re-opened this menu they wouldn't show up

            // TLDR: this part syncs the UI with what's actually going on
            for (int k = 0; k < layers[i].requiredStates.Count; k++) {
                // first we add the element object, we can let the UI_InputList do the lifting
                newInputList.GetComponent<UI_InputList>().AddNewElement();
                // then set the text value to whatever that state is
                newInputList.transform.GetChild(0).GetChild(k).GetComponent<TMP_InputField>().text = layers[i].requiredStates[k];
            }

            // telling the element list that when we click off of one of the input fields, it should send over the string value
            newInputList.GetComponent<UI_InputList>().onEditElement.AddListener(
                (value) => {
                    SetState(j, newInputList.GetComponent<UI_InputList>().pressedElementIndex, value);
                }
            );
            // when we add a new element to the UI, a new state should be added to the backend too
            newInputList.GetComponent<UI_InputList>().onAddNewElement.AddListener(
                () => AddNewRequiredState(j)
            );

            inputSet.GetComponent<UI_ObjectSetManager>().sets[i] = new UI_ObjectSet(new GameObject[]{newInputList});

            // TODO: make the image buttons actually reflect the image that has been selected, 
            // right now they're always just white and its the worst UX thing ever

            // this element list is for the resulting images
            GameObject newImageList = Instantiate(UIPrefabs.Instance.elementListPrefab, Vector3.zero, Quaternion.identity);
            newImageList.transform.SetParent(window.transform.GetChild(4));
            newImageList.transform.localPosition = new Vector3(150, -50, 0);

            newImageList.GetComponent<UI_InputList>().spacing = 100;

            // normally an element list uses input fields (per the prefab), but we're telling this one to use buttons
            newImageList.GetComponent<UI_InputList>().inputPrefab = UIPrefabs.Instance.buttonPrefab;

            // since (for some reason) we don't actually display the selected image,
            // all we need to do to sync up the UI is create however many buttons we need

            // TLDR: also syncing UI but less work for images
            for (int k = 0; k < layers[i].images.Count; k++) {
                newImageList.GetComponent<UI_InputList>().AddNewElement();
            }

            // like for the required states, when the user edits an element we want get the new value
            // we're using onPressElement instead here bc its buttons

            // method of getting images is unique here, what we're doing is telling the UIManager to open the asset menu with GrabAssetName(),
            // so that the user can pick an image from there,
            // and supplying what we want to do (as a UnityAction) with the name that the user picked
            // in this case what we want to do is SetImage()

            // TLDR: keep backend synced with UI
            newImageList.GetComponent<UI_InputList>().onPressElement.AddListener(
                () => {
                    UIManager.Instance.GrabAssetName((name) =>
                    SetImage(j, newImageList.GetComponent<UI_InputList>().pressedElementIndex, name));
                }
            );
            // same as the required states list, when we add a new element we want the backend to keep up
            newImageList.GetComponent<UI_InputList>().onAddNewElement.AddListener(
                () => AddNewImage(j)
            );

            // make sure the object set has a reference to this element list
            imageSet.GetComponent<UI_ObjectSetManager>().sets[j] = new UI_ObjectSet(new GameObject[]{newImageList});

            Destroy(newInputList.GetComponent<UI_InputList>().addButton.gameObject);
            newInputList.GetComponent<UI_InputList>().addButton = newImageList.GetComponent<UI_InputList>().addButton;
        }

        // we call onChangeTabs to make sure that the object sets only have one set of objects enabled
        // since the object sets have layerTabs as the controlling tab object, this will call EnableSetOfIndex()
        layerTabs.GetComponent<UI_Tabs>().onChangeTabs.Invoke(layerTabs.GetComponent<UI_Tabs>().selectedTab);
    }
    
    // called periodically, used to deal with the image components
    // a PositionUI() function is present on most nodes
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

        if (modeDropdown != null) {
            mode = modeDropdown.value;
        }
    }

    // why are these functions? ig in case I want to add code whenever new stuff is added
    public void AddNewRequiredState(int layerIndex) {
        layers[layerIndex].requiredStates.Add(null);
    }
    public void AddNewImage(int layerIndex) {
        layers[layerIndex].images.Add(null);
    }

    // functions for setting data
    public void SetImage(int layerIndex, int stateIndex, string assetName) {
        if (layers[layerIndex].images.Count <= stateIndex) {return;}

        layers[layerIndex].images[stateIndex] = assetName;
        // make sure to close the asset menu so the user knows they've selected something
        UIManager.Instance.assetManagerWindow.SetActive(false);
    }
    public void SetState(int layerIndex, int stateIndex, string value) {
        if (layers[layerIndex].requiredStates.Count <= stateIndex) {return;}

        layers[layerIndex].requiredStates[stateIndex] = value;
    }

    // sometimes you want a float, not a double?
    public float GetValueAsFloat() {
        float parsedValue;

        if (mode == (int)NodeDisplayMode.Double && float.TryParse(dataValue, out parsedValue)) {
            return parsedValue;
        }
        else {
            return (float)NetworkManager.Instance.defaultDouble;
        }
    }
    // for boolean mode
    public bool GetValueAsBoolean() {
        bool toReturn = false;

        if (mode == (int)NodeDisplayMode.Boolean) {
            if (dataValue == true.ToString()) {
                toReturn = true;
            }
            else if (dataValue == false.ToString()) {
                toReturn = false;
            }
        }
        
        return toReturn;
    }
    // if the mode is string, we can just return whatever dataValue is, no parsing
    public string GetValueAsString() {
        return dataValue;
    }

    // called periodically (inside of NetworkManager) to update the data value from networktables
    // the function called by NetworkManager to update the data value, based on the mode it looks for different data types
    public void UpdateData() {
        if (mode == (int)NodeDisplayMode.Double) {
            dataValue = NetworkManager.Instance.FetchNTDouble(sourceString).ToString();
        }
        else if (mode == (int)NodeDisplayMode.String) {
            // no need to cast here, it's already a string
            dataValue = NetworkManager.Instance.FetchNTString(sourceString);
        } else if (mode == (int)NodeDisplayMode.Boolean) {
            // no need to cast here, it's already a string
            dataValue = NetworkManager.Instance.FetchNTBoolean(sourceString).ToString();
        }
    }
}