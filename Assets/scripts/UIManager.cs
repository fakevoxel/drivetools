using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum NodeType {
    TextDisplay,
    Compass,
    Field2D,
    Graph, // unfinished
    ImageDisplay,
    Boolean, // modified image display node, still uses NodeType.imageDisplay, the unique type is only for spawning it in
    Dropdown,
    Double, // similar to boolean nodes, double and string nodes are modified TextDisplay, the type is only for spawning
    String,
}

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public static UIManager Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("Duplicate UIManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake() {
        Instance = this;
    }

    public UI_Tabs layoutTabs;

    // window objects (references, NOT PREFABS)
    public GameObject settingsWindow; // the window for changing settings like FRC team number
    public GameObject nodeWindow; // the window for spawning new nodes
    public GameObject configWindow; // the window for configuring a node's data, like source string
    public GameObject loadFileWindow;
    public GameObject nodeOptionsWidget;
    public GameObject assetManagerWindow;
    public GameObject typeSelectionWindow;

    // container object
    public Transform canvasTransform;
    public Transform layoutContainer;
    public GameObject layoutTabsObject;

    public int activeLayoutIndex;
    public List<GameObject> activeNodes;
    public Transform activeNodeContainer;

    public GameObject previewNode;

    private UnityAction<string> toRunWithLoadedFile;
    private UnityAction<string> toRunWithLoadedAsset;
    private bool isLocked; // if true, user can't interact with nodes

    void Start() {
        CloseRightClickMenu();
    }

    // don't usually use the update function in this project,
    // because I want to be able to control when logic is run
    // I do this so that I can manually tell nodes to refresh or not
    void Update() {
        // handle switching layouts (the UI_Tabs class)
        layoutTabs.HandleInteract();

        // handle node interations for the active layout
        if (!isLocked) {
            // handle node interactions for the active layout
            UpdateActiveLayout();
        }

        canvasTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 
        canvasTransform.GetComponent<RectTransform>().sizeDelta * 1.25f;

        layoutTabsObject.transform.GetChild(0).position = new Vector3(
            Screen.width/2, Screen.height, 0
        );
        layoutTabsObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 
        new Vector2(canvasTransform.GetComponent<RectTransform>().sizeDelta.x * 1.25f, 100);
    }

    public void ToggleLock() {
        isLocked = !isLocked;
    }

    public void LoadFile(UnityAction<string> toRunAfterLoad) {
        loadFileWindow.SetActive(true);
        toRunWithLoadedFile = toRunAfterLoad;
    }

    public void PassLoadedFile(TMP_InputField input) {
        string text = input.text;
        
        loadFileWindow.SetActive(false);
        toRunWithLoadedFile.Invoke(text);
    }
    public void PassLoadedFile(string input) {
        string text = input;
        
        loadFileWindow.SetActive(false);
        toRunWithLoadedFile.Invoke(text);
    }
    
    // I'm actually why not sure why this function is wrapped into another function
    public void SwitchActiveLayout() {
        SwitchToLayout(layoutTabs.selectedTab);
    }

    // logic for when the user quits the program
    public void OnApplicationQuit() {
        // move all relavant data into the NodeData classes for each node
        PopulateNodeData();
        // save all of the layouts to disk, to be loaded when the user opens the program again
        SaveUtils.SaveLayouts(AppData.Instance.layouts.ToArray());

        NetworkManager.Instance.CloseNetworkTables();
    }

    // opens the menu that shows all the available nodes currently on NT
    // this is the function that the "select from NT" button calls
    public void ShowAvailableNodes() {
        typeSelectionWindow.SetActive(true);
        NetworkManager.Instance.PopulateNodeSelectionMenu(new bool[]{true, true, true, true});
    }

    // going through EVERY SINGLE active noded and populating its respective NodeData class
    void PopulateNodeData() {
        AppData.Instance.layouts[activeLayoutIndex].textDisplayNodes = new NodeData_TextDisplay[]{};
        AppData.Instance.layouts[activeLayoutIndex].compassNodes = new NodeData_Compass[]{};
        AppData.Instance.layouts[activeLayoutIndex].graphNodes = new NodeData_Graph[]{};
        AppData.Instance.layouts[activeLayoutIndex].imageDisplayNodes = new NodeData_ImageDisplay[]{};
        AppData.Instance.layouts[activeLayoutIndex].field2DNodes = new NodeData_Field2D[]{};
        AppData.Instance.layouts[activeLayoutIndex].dropdownNodes = new NodeData_Dropdown[]{};

        for (int i = 0; i < activeNodes.Count; i++) {
            if (activeNodes[i] == null) {continue;}

            GameObject currentNode = activeNodes[i];

            if (currentNode.GetComponent<Node_TextDisplay>() != null) {
                List<NodeData_TextDisplay> doubleNodeList = new List<NodeData_TextDisplay>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].textDisplayNodes.Length; j++) {
                    doubleNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].textDisplayNodes[j]);
                }

                currentNode.GetComponent<Node_TextDisplay>().PopulateDataClass();
                doubleNodeList.Add(currentNode.GetComponent<Node_TextDisplay>().data);

                AppData.Instance.layouts[activeLayoutIndex].textDisplayNodes = doubleNodeList.ToArray();
            }
            else if (currentNode.GetComponent<Node_Compass>() != null) {
                List<NodeData_Compass> compassNodeList = new List<NodeData_Compass>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].compassNodes.Length; j++) {
                    compassNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].compassNodes[j]);
                }

                currentNode.GetComponent<Node_Compass>().PopulateDataClass();
                compassNodeList.Add(currentNode.GetComponent<Node_Compass>().data);

                AppData.Instance.layouts[activeLayoutIndex].compassNodes = compassNodeList.ToArray();
            }
            else if (currentNode.GetComponent<Node_ImageDisplay>() != null) {
                List<NodeData_ImageDisplay> imageDisplayNodeList = new List<NodeData_ImageDisplay>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].imageDisplayNodes.Length; j++) {
                    imageDisplayNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].imageDisplayNodes[j]);
                }

                currentNode.GetComponent<Node_ImageDisplay>().PopulateDataClass();
                imageDisplayNodeList.Add(currentNode.GetComponent<Node_ImageDisplay>().data);

                AppData.Instance.layouts[activeLayoutIndex].imageDisplayNodes = imageDisplayNodeList.ToArray();
            }
            else if (currentNode.GetComponent<Node_Field2D>() != null) {
                List<NodeData_Field2D> field2DNodeList = new List<NodeData_Field2D>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].field2DNodes.Length; j++) {
                    field2DNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].field2DNodes[j]);
                }

                currentNode.GetComponent<Node_Field2D>().PopulateDataClass();
                field2DNodeList.Add(currentNode.GetComponent<Node_Field2D>().data);

                AppData.Instance.layouts[activeLayoutIndex].field2DNodes = field2DNodeList.ToArray();
            }
            else if (currentNode.GetComponent<Node_Dropdown>() != null) {
                List<NodeData_Dropdown> dropdownNodeList = new List<NodeData_Dropdown>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].dropdownNodes.Length; j++) {
                    dropdownNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].dropdownNodes[j]);
                }

                currentNode.GetComponent<Node_Dropdown>().PopulateDataClass();
                dropdownNodeList.Add(currentNode.GetComponent<Node_Dropdown>().data);

                AppData.Instance.layouts[activeLayoutIndex].dropdownNodes = dropdownNodeList.ToArray();
            }
        }
    }

    public void DeleteAllNodes() {
        for (int i = 0; i < activeNodes.Count; i++) {
            DeleteNode(activeNodes[i]);
        }
    }

    // opens a little widget that allows you to select options for the node,
    // usually "delete" "edit" and "track"
    public void OpenRightClickMenu(NodeInteractionHandler node) {
        nodeOptionsWidget.SetActive(true);

        nodeOptionsWidget.transform.position = Input.mousePosition;

        PopulateRightClickMenu(node);
    }

    // populating the right-click widget with options for this node
    public void PopulateRightClickMenu(NodeInteractionHandler interact) {
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
            () => UIManager.Instance.OpenNodeConfig(interact)
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
            () => UIManager.Instance.DeleteNode(interact.gameObject)
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

    public void CloseRightClickMenu() {
        nodeOptionsWidget.SetActive(false);
    }

    // Adding a new node to the active nodes list
    // the .Add() call here is wrapped into another function so future logic can be easily added
    public void AddNodeToList(GameObject nodeToAdd) {
        activeNodes.Add(nodeToAdd);
    }

    public void DeleteNode(GameObject obj) {
        activeNodes.Remove(obj);
        Destroy(obj);

        RefreshAppData();

        CloseRightClickMenu();
    }

    public void TrackAllNodes() {
        for (int i = 0; i < activeNodes.Count; i++) {
            if (!activeNodes[i].GetComponent<NodeInteractionHandler>().isNodeTracked) {
                activeNodes[i].GetComponent<NodeInteractionHandler>().ToggleTrack();
            }
        }
    }

    // Spawning a new node manually, no data is available except the node's type
    // IMPORTANT - this "grabs" the new node, placing it on the cursor for the user to position
    public void SpawnNewNode(int type) {
        GameObject newNode = null;

        newNode = Instantiate(AppData.Instance.GetPrefabObject(type), Vector3.zero, Quaternion.identity);

        newNode.transform.SetParent(activeNodeContainer);
        newNode.GetComponent<NodeInteractionHandler>().Grab();
        newNode.GetComponent<NodeInteractionHandler>().Lock();

        AddNodeToList(newNode);
    }

    // Spawwning and placing a node programatically
    // this AVOIDS placing it on the cursor like would happen when you spawn it manually
    // this version in particular is used when loading nodes from SmartDashboard, where only the sourceString is available
    public void SpawnAndPlaceNewNode(int type, string sourceString) {
        GameObject newNode = null;

        newNode = Instantiate(AppData.Instance.GetPrefabObject(type), Vector3.zero, Quaternion.identity);

        if (type == (int)NodeType.TextDisplay) {
            newNode.GetComponent<Node_TextDisplay>().SetSourceString(sourceString);
        }

        newNode.transform.SetParent(activeNodeContainer);
        AddNodeToList(newNode);

        newNode.transform.position = new Vector3(Screen.width/2, Screen.height/2, 0);
    }

    // Spawwning and placing a node programatically
    // this AVOIDS placing it on the cursor like would happen when you spawn it manually
    // this version of the function in particular is used when loading nodes from disk, because all the info is there
    public GameObject SpawnAndPlaceNewNode(int type, Vector2 pos, Vector2 size, bool isTracked, GameObject parent) {
        GameObject newNode = null;

        newNode = Instantiate(AppData.Instance.GetPrefabObject(type), Vector3.zero, Quaternion.identity);

        newNode.transform.SetParent(parent.transform.GetChild(0));
        AddNodeToList(newNode);

        newNode.transform.position = pos;

        newNode.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = size;

        newNode.GetComponent<NodeInteractionHandler>().isNodeTracked = isTracked;

        return newNode;
    }

    // opening and setting up the configuration UI for a given node
    public void OpenNodeConfig(NodeInteractionHandler comp) {
        CloseRightClickMenu();

        // clear all the children in the config window so we don't duplicate UI objects
        CanvasUtils.DestroyChildren(configWindow.transform.GetChild(4).gameObject);

        configWindow.SetActive(true);
        if (comp.nodeType == (int)NodeType.TextDisplay) {
            comp.GetComponent<Node_TextDisplay>().PopulateConfigMenu(configWindow);
        } else if (comp.nodeType == (int)NodeType.Compass) {
            // compass nodes are simple, they only have a source string

            // create an input field for the source string
            GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);

            // parent it to the window
            sourceInput.transform.SetParent(configWindow.transform.GetChild(4));
            // zero it
            sourceInput.transform.localPosition = new Vector3(0, 0, 0);
            
            // we set the text of the input field to show the user the current source string
            sourceInput.GetComponent<TMP_InputField>().text = comp.GetComponent<Node_Compass>().sourceString;
            // when the user finishes editing, change the source string of the double node
            sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
            comp.GetComponent<Node_Compass>().SetSourceString); // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter
        } else if (comp.nodeType == (int)NodeType.Field2D) {
            comp.GetComponent<Node_Field2D>().PopulateConfigMenu(configWindow);
        } else if (comp.nodeType == (int)NodeType.Graph) {
            GameObject testModeButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
            testModeButton.transform.SetParent(configWindow.transform.GetChild(4));
            testModeButton.transform.localPosition = new Vector3(0, 0, 0);
            testModeButton.GetComponent<UI_Button>().onPress.AddListener(
                    () => comp.GetComponent<Node_Graph>().ToggleTestMode()
                );
            
        } else if (comp.nodeType == (int)NodeType.ImageDisplay) {
            comp.GetComponent<Node_ImageDisplay>().PopulateConfigMenu(configWindow);
        }
    }

    public void ReturnAssetName(string gotAssetName) {
        toRunWithLoadedAsset.Invoke(gotAssetName);
    }

    public void GrabAssetName(UnityAction<string> toRunWhenFinished) {
        toRunWithLoadedAsset = toRunWhenFinished;

        assetManagerWindow.SetActive(true);
    }

    public void UpdateActiveLayout() {
        for (int i = 0; i < activeNodes.Count; i++) {
            if (activeNodes[i] != null) {
                activeNodes[i].GetComponent<NodeInteractionHandler>().Refresh();
            }
        }
    }

    public void CreateNewLayout() {
        AppData.Instance.layouts.Add(new DashboardLayout());
        RefreshAppData();
    }

    public void RefreshAppData() {
        // setting up the layout tabs
        layoutTabs.InitializeTabs(AppData.Instance.layouts.Count, AppData.layoutTabSpacing);

        layoutTabs.onChangeTabs.AddListener(SwitchToLayout); 

        LoadAllLayouts();

        Application.targetFrameRate = AppData.Instance.targetFrameRate;
    }

    // re-loading ONLY NEW layouts from the appdata class, existing layouts stay
    public void LoadAllLayouts() {
        for (int i = 0; i < AppData.Instance.layouts.Count; i++) {
            if (AppData.Instance.layouts[i].isNew) { // checking for a new layout
                InitializeLayoutOfIndex(i);

                AppData.Instance.layouts[i].isNew = false;
            }
        }
        // making sure there is only one active layout at a time
        SwitchToLayout(activeLayoutIndex);
    }

    // re-loading all layouts from the appata class
    public void ResetAndLoadAllLayouts() {
        for (int i = 0; i < AppData.Instance.layouts.Count; i++) {
            InitializeLayoutOfIndex(i);
            AppData.Instance.layouts[i].isNew = false;
        }
        // making sure there is only one active layout at a time
        SwitchToLayout(activeLayoutIndex);
    }

    public void InitializeLayoutOfIndex(int layoutIndex) {
        if (AppData.Instance.layouts.Count <= layoutIndex) {
            Debug.Log("ERROR: Tried to initialize a layout with index greater than available layouts.");
        }
        else {
            DashboardLayout layout = AppData.Instance.layouts[layoutIndex];

            // create a container object for the layout with that layout's name
            GameObject layoutObject = Instantiate(UIPrefabs.Instance.layoutPrefab, Vector3.zero, Quaternion.identity);
            // set the parent to the canvas
            layoutObject.transform.SetParent(layoutContainer);
            // set the name
            layoutObject.name = layout.name;

            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].textDisplayNodes.Length; i++) {
                NodeData_TextDisplay dataClass = AppData.Instance.layouts[layoutIndex].textDisplayNodes[i];

                GameObject node = SpawnAndPlaceNewNode((int)NodeType.TextDisplay, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.generic.isTracked, layoutObject);
                node.GetComponent<Node_TextDisplay>().sourceString = dataClass.sourceString;
                node.GetComponent<Node_TextDisplay>().titleString = dataClass.titleString;
                node.GetComponent<Node_TextDisplay>().mode = dataClass.mode;
            }
            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].compassNodes.Length; i++) {
                NodeData_Compass dataClass = AppData.Instance.layouts[layoutIndex].compassNodes[i];

                GameObject node = SpawnAndPlaceNewNode((int)NodeType.Compass, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.generic.isTracked, layoutObject);
                node.GetComponent<Node_Compass>().sourceString = dataClass.sourceString;
            }
            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].imageDisplayNodes.Length; i++) {
                NodeData_ImageDisplay dataClass = AppData.Instance.layouts[layoutIndex].imageDisplayNodes[i];

                GameObject node = SpawnAndPlaceNewNode((int)NodeType.ImageDisplay, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.generic.isTracked, layoutObject);
                node.GetComponent<Node_ImageDisplay>().sourceString = dataClass.sourceString;
                node.GetComponent<Node_ImageDisplay>().layers = dataClass.layers.ToList();
                node.GetComponent<Node_ImageDisplay>().mode = dataClass.mode;
            }
            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].field2DNodes.Length; i++) {
                NodeData_Field2D dataClass = AppData.Instance.layouts[layoutIndex].field2DNodes[i];

                GameObject node = SpawnAndPlaceNewNode((int)NodeType.Field2D, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.generic.isTracked, layoutObject);
                node.GetComponent<Node_Field2D>().robots = dataClass.robots;
                node.GetComponent<Node_Field2D>().markers = dataClass.markers;
            }
            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].dropdownNodes.Length; i++) {
                NodeData_Dropdown dataClass = AppData.Instance.layouts[layoutIndex].dropdownNodes[i];

                GameObject node = SpawnAndPlaceNewNode((int)NodeType.Dropdown, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.generic.isTracked, layoutObject);
                node.GetComponent<Node_Dropdown>().sourceString = dataClass.sourceString;
                node.GetComponent<Node_Dropdown>().titleString = dataClass.titleString;
            }
        }
    }

    public void SwitchToLayout(int layoutIndex) {
        if (AppData.Instance.layouts.Count <= layoutIndex) {
            Debug.Log("ERROR: Tried to switch to a layout with index greater than available layouts.");
        }
        else {
            activeNodes.Clear();

            GameObject layoutObject = null;
            for (int i = 0; i < AppData.Instance.layouts.Count; i++) {
                if (i != layoutIndex) {
                    layoutContainer.GetChild(i).gameObject.SetActive(false);
                }
                else {
                    layoutContainer.GetChild(i).gameObject.SetActive(true);
                    layoutObject = layoutContainer.GetChild(i).gameObject;
                }
            }

            activeNodeContainer = CanvasUtils.SearchChildrenForName(layoutObject, "nodes");

            for (int i = 0; i < activeNodeContainer.transform.childCount; i++) {
                activeNodes.Add(activeNodeContainer.transform.GetChild(i).gameObject);
            }
        }
    }

    // -- Node Stuff -- // 

    public Vector3[] GetEdgePoints(int edgeType) {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < activeNodes.Count; i++) {
            if (activeNodes[i] == null) {continue;}
            if (edgeType == 0) {
                points.Add(activeNodes[i].GetComponent<NodeInteractionHandler>().leftEdge);
            }
            else if (edgeType == 1) {
                points.Add(activeNodes[i].GetComponent<NodeInteractionHandler>().rightEdge);
            }
            else if (edgeType == 2) {
                points.Add(activeNodes[i].GetComponent<NodeInteractionHandler>().bottomEdge);
            }
            else if (edgeType == 3) {
                points.Add(activeNodes[i].GetComponent<NodeInteractionHandler>().topEdge);
            }
        }
        return points.ToArray();
    }

    public Transform[] GetAlignedEdgeObjects(int edgeType, Vector3 edgePos) {
        List<Transform> objects = new List<Transform>();
        for (int i = 0; i < activeNodes.Count; i++) {
            if (activeNodes[i] == null) {continue;}
            NodeInteractionHandler component = activeNodes[i].GetComponent<NodeInteractionHandler>();

            if (edgeType == 0) {
                if (Mathf.Abs(edgePos.x - component.leftEdge.x) < 10) {
                    objects.Add(activeNodes[i].transform);
                }
            } else if (edgeType == 1) {
                if (Mathf.Abs(edgePos.x - component.rightEdge.x) < 10) {
                    objects.Add(activeNodes[i].transform);
                }
            } else if (edgeType == 2) {
                if (Mathf.Abs(edgePos.y - component.bottomEdge.y) < 10) {
                    objects.Add(activeNodes[i].transform);
                }
            } else if (edgeType == 3) {
                if (Mathf.Abs(edgePos.y - component.topEdge.y) < 10) {
                    objects.Add(activeNodes[i].transform);
                }
            }
        }

        return objects.ToArray();
    }

    public void MoveAlignedEdges(int edgeType, Vector3 edgePos, Vector3 newPos, Vector3 centerPos) {
        Transform[] oppositeEdges = GetAlignedEdgeObjects((int)NodeUtils.GetOppositeEdgeType((int)edgeType), edgePos);
        Transform[] parallelEdges = GetAlignedEdgeObjects(edgeType, edgePos);

        if (edgeType == 0) {
            newPos = new Vector3(newPos.x, centerPos.y, 0);
        } else if (edgeType == 1) {
            newPos = new Vector3(newPos.x, centerPos.y, 0);
        } else if (edgeType == 2) {
            newPos = new Vector3(centerPos.x, newPos.y, 0);
        } else if (edgeType == 3) {
            newPos = new Vector3(centerPos.x, newPos.y, 0);
        }

        Vector3 limitPoint = newPos;

        bool isShrinking = false;
        if (edgeType == 0 && newPos.x > edgePos.x ||
        edgeType == 1 && newPos.x < edgePos.x ||
        edgeType == 2 && newPos.y > edgePos.y ||
        edgeType == 3 && newPos.y < edgePos.y) {
            isShrinking = true;
        }

        if (!isShrinking) {
            for (int i = 0; i < oppositeEdges.Length; i++) {
                Vector3 newLimitPoint = oppositeEdges[i].GetComponent<NodeInteractionHandler>().ClampAttemptedMove(newPos, (int)NodeUtils.GetOppositeEdgeType((int)edgeType));

                bool isMoreLimiting = false;
                if (edgeType == 0 && newLimitPoint.x > limitPoint.x ||
                edgeType == 1 && newLimitPoint.x < limitPoint.x ||
                edgeType == 2 && newLimitPoint.y > limitPoint.y ||
                edgeType == 3 && newLimitPoint.y < limitPoint.y) {
                    isMoreLimiting = true;
                }

                if (isMoreLimiting) {
                    limitPoint = newLimitPoint;
                }
            }
            
        } else {
            for (int i = 0; i < parallelEdges.Length; i++) {
                Vector3 newLimitPoint = parallelEdges[i].GetComponent<NodeInteractionHandler>().ClampAttemptedMove(newPos, edgeType);

                bool isMoreLimiting = false;
                if (edgeType == 0 && newLimitPoint.x < limitPoint.x ||
                edgeType == 1 && newLimitPoint.x > limitPoint.x ||
                edgeType == 2 && newLimitPoint.y < limitPoint.y ||
                edgeType == 3 && newLimitPoint.y > limitPoint.y) {
                    isMoreLimiting = true;
                }

                if (isMoreLimiting) {
                    limitPoint = newLimitPoint;
                }
            }
        }

        for (int i = 0; i < oppositeEdges.Length; i++) {
            oppositeEdges[i].gameObject.GetComponent<NodeInteractionHandler>().MoveEdge((int)NodeUtils.GetOppositeEdgeType((int)edgeType), limitPoint);
        }
        for (int i = 0; i < parallelEdges.Length; i++) {
            parallelEdges[i].gameObject.GetComponent<NodeInteractionHandler>().MoveEdge(edgeType, limitPoint);
        }
    }

    public void DrawPreviewNode(Vector3 pos, Vector2 size) {
        if (previewNode == null) {
            previewNode = Instantiate(UIPrefabs.Instance.previewNodePrefab, Vector3.zero, Quaternion.identity);
            previewNode.transform.SetParent(activeNodeContainer.parent);

            previewNode.transform.SetSiblingIndex(0);
        }

        previewNode.transform.position = pos;
        CanvasUtils.SearchChildrenForName(previewNode, "bg").GetComponent<RectTransform>().sizeDelta = size;
    }

    public void ClearPreviewNode() {
        Destroy(previewNode);
    }
}
