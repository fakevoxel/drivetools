using System;
using System.Collections;
using System.Collections.Generic;
using NetworkTables;
using TMPro;
using UnityEngine;

public enum NodeType {
    Double,
    String,
    Compass,
    Field2D,
    Graph,
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
    public GameObject nodeOptionsWidget;

    // container object
    public Transform canvasTransform;
    public Transform layoutContainer;
    public GameObject layoutTabsObject;

    public int activeLayoutIndex;
    public List<GameObject> activeNodes;
    public Transform activeNodeContainer;

    public GameObject previewNode;

    void Start() {
        CloseRightClickMenu();
    }

    // don't usually use the update function in this project,
    // because I want to be able to control when logic is run
    // I do this so that I can manually tell nodes to refresh or not
    void Update() {
        // update all DATA (coming from robot) for nodes in the active layout
        UpdateActiveLayout();

        // handle node interations for the active layout
        layoutTabs.HandleInteract();

        canvasTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 
        canvasTransform.GetComponent<RectTransform>().sizeDelta * 1.25f;

        layoutTabsObject.transform.GetChild(0).position = new Vector3(
            Screen.width/2, Screen.height, 0
        );
        layoutTabsObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = 
        new Vector2(canvasTransform.GetComponent<RectTransform>().sizeDelta.x * 1.25f, 100);
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
    }

    // going through EVERY SINGLE active noded and populating its respective NodeData class
    void PopulateNodeData() {
        AppData.Instance.layouts[activeLayoutIndex].doubleNodes = new NodeData_Double[]{};
        for (int i = 0; i < activeNodes.Count; i++) {
            if (activeNodes[i] == null) {continue;}

            GameObject currentNode = activeNodes[i];

            if (currentNode.GetComponent<Node_Double>() != null) {
                List<NodeData_Double> doubleNodeList = new List<NodeData_Double>();
                for (int j = 0; j < AppData.Instance.layouts[activeLayoutIndex].doubleNodes.Length; j++) {
                    doubleNodeList.Add(AppData.Instance.layouts[activeLayoutIndex].doubleNodes[j]);
                }

                currentNode.GetComponent<Node_Double>().PopulateDataClass();
                doubleNodeList.Add(currentNode.GetComponent<Node_Double>().data);

                AppData.Instance.layouts[activeLayoutIndex].doubleNodes = doubleNodeList.ToArray();
            }
        }
    }

    // opens a little widget that allows you to select options for the node,
    // usually "delete" "edit" and "track"
    public void OpenRightClickMenu(NodeInteractionHandler node) {
        nodeOptionsWidget.SetActive(true);

        nodeOptionsWidget.transform.position = Input.mousePosition;

        if (node.nodeType == (int)NodeType.Double) {
            // if its a double type, fill out the options menu with double stuff
            node.GetComponent<Node_Double>().PopulateRightClickMenu();
        }
        else if (node.nodeType == (int)NodeType.Graph) {
            // if its a double type, fill out the options menu with double stuff
            node.GetComponent<Node_Graph>().PopulateRightClickMenu();
        }
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

        if (type == (int)NodeType.Double) {
            newNode.GetComponent<Node_Double>().SetSourceString(sourceString);
        }

        newNode.transform.SetParent(activeNodeContainer);
        AddNodeToList(newNode);

        newNode.transform.position = new Vector3(Screen.width/2, Screen.height/2, 0);
    }

    // Spawwning and placing a node programatically
    // this AVOIDS placing it on the cursor like would happen when you spawn it manually
    // this version of the function in particular is used when loading nodes from disk, because all the info is there
    public void SpawnAndPlaceNewNode(int type, Vector2 pos, Vector2 size, string sourceString, GameObject parent) {
        GameObject newNode = null;

        newNode = Instantiate(AppData.Instance.GetPrefabObject(type), Vector3.zero, Quaternion.identity);

        if (type == (int)NodeType.Double) {
            newNode.GetComponent<Node_Double>().SetSourceString(sourceString);
        }

        newNode.transform.SetParent(parent.transform.GetChild(0));
        AddNodeToList(newNode);

        newNode.transform.position = pos;

        newNode.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = size;
    }

    // Checking if a node exists with a certain source string
    // among other things this is used when grabbing nodes from SmartDashboard, to avoid duplicates
    public bool HasActiveNodeWithSource(int type, string sourceString) {
        if (type == (int)NodeType.Double) {
            for (int i = 0; i < activeNodes.Count; i++) {
                if (activeNodes[i].GetComponent<Node_Double>().sourceString == sourceString) {
                    return true;
                }
            }
        }

        return false;
    }

    // opening and setting up the configuration UI for a given node
    public void OpenNodeConfig(NodeInteractionHandler comp) {
        CloseRightClickMenu();

        configWindow.SetActive(true);
        if (comp.nodeType == (int)NodeType.Double) {
            // double nodes are simple, they only have a source string and a title
            // TODO: add the title

            // clear all the children in the config window so we don't duplicate UI objects
            CanvasUtils.DestroyChildren(configWindow.transform.GetChild(4).gameObject);

            // create an input field for the source string
            GameObject sourceInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);

            // parent it to the window
            sourceInput.transform.SetParent(configWindow.transform.GetChild(4));
            // zero it
            sourceInput.transform.localPosition = new Vector3(0, 0, 0);
            
            // we set the text of the input field to show the user the current source string
            sourceInput.GetComponent<TMP_InputField>().text = comp.GetComponent<Node_Double>().sourceString;
            // when the user finishes editing, change the source string of the double node
            sourceInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                comp.GetComponent<Node_Double>().SetSourceString); // this syntax is interesting, I supply the function without () and unity knows to give it the final string as a parameter

        } else if (comp.nodeType == (int)NodeType.Graph) {
            CanvasUtils.DestroyChildren(configWindow.transform.GetChild(4).gameObject);

            GameObject testModeButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
            testModeButton.transform.SetParent(configWindow.transform.GetChild(4));
            testModeButton.transform.localPosition = new Vector3(0, 0, 0);
            testModeButton.GetComponent<UI_Button>().onPress.AddListener(
                    () => comp.GetComponent<Node_Graph>().ToggleTestMode()
                );
            
        } else if (comp.nodeType == (int)NodeType.Field2D) {
            CanvasUtils.DestroyChildren(configWindow.transform.GetChild(4).gameObject);
            
            GameObject addRobotButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
            addRobotButton.transform.SetParent(configWindow.transform.GetChild(4));
            addRobotButton.transform.localPosition = new Vector3(0, 0, 0);
            addRobotButton.GetComponent<UI_Button>().onPress.AddListener(
                    () => comp.GetComponent<Node_Field2D>().AddTrackedRobot()
                );

            for (int i = 0; i < comp.GetComponent<Node_Field2D>().robots.Count; i++) {
                GameObject xInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
                xInput.transform.SetParent(configWindow.transform.GetChild(4));
                xInput.transform.localPosition = new Vector3(0, 0, 0);
                xInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                    (value) => comp.GetComponent<Node_Field2D>().SetX(value, 0)
                );
                GameObject yInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
                yInput.transform.SetParent(configWindow.transform.GetChild(4));
                yInput.transform.localPosition = new Vector3(0, -100, 0);
                yInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                    (value) => comp.GetComponent<Node_Field2D>().SetY(value, 0)
                );
                GameObject zInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
                zInput.transform.SetParent(configWindow.transform.GetChild(4));
                zInput.transform.localPosition = new Vector3(0, -200, 0);
                zInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                    (value) => comp.GetComponent<Node_Field2D>().SetRot(value, 0)
                );
            }
        }
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

        layoutTabs.onChangeTabs = SwitchToLayout; 

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

            for (int i = 0; i < AppData.Instance.layouts[layoutIndex].doubleNodes.Length; i++) {
                NodeData_Double dataClass = AppData.Instance.layouts[layoutIndex].doubleNodes[i];
                SpawnAndPlaceNewNode((int)NodeType.Double, dataClass.generic.GetPosition(), dataClass.generic.GetSize(), dataClass.sourceString, layoutObject);
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

    public void ToggleSettingsWindow() {
        settingsWindow.SetActive(!settingsWindow.activeSelf);
    }

    public void ToggleNewNodeWindow() {
        nodeWindow.SetActive(!nodeWindow.activeSelf);
    }

    public void ToggleConfigWindow() {
        configWindow.SetActive(!configWindow.activeSelf);
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
