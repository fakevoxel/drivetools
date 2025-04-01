using UnityEngine;
using FRC.NetworkTables;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;

    public static NetworkManager Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("Duplicate NetworkManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake() {
        Instance = this;
    }

    public bool devMode; // if true, run w/o NT

    public Image isConnectedIcon;
    public TextMeshProUGUI ipDisplay;
    public bool isNetworkRunning;
    // the dashboard works off of the SmartDashboard networktable, so right now I just have a variable to store that
    private NetworkTable table;

    // these values get send to the nodes if there is no connection
    public string defaultString = "null";
    public double defaultDouble = 0;
    // timer variable for debugging the number of connected clients
    float lastClientLogTime;
    // log interval
    public float clientLogInterval;

    private bool gotTable;

    void Update() {
        if (!isNetworkRunning && !devMode) {
            InitializeNetworkTablesClient();
            isNetworkRunning = true;
        }

        if (Time.time > lastClientLogTime + clientLogInterval) {
            lastClientLogTime = Time.time;
            Debug.Log("Number of networktables connections: " + NetworkTableInstance.Default.GetConnections().Length);
        }

        // Right now using the smartdashboard table only
        // TODO: add functionality to switch between tables
        if (NetworkTableInstance.Default.GetConnections().Length > 0) {
            if (!gotTable) {
                gotTable = true;
                table = NetworkTableInstance.Default.GetTable("SmartDashboard");
            }

            UpdateAllNodeData(table);
            isConnectedIcon.color = Color.green;
        }
        else {
            isConnectedIcon.color = Color.red;
        }
    }

    public void UpdateAllNodeData(NetworkTable sourceTable) {
        for (int i = 0; i < UIManager.Instance.activeNodes.Count; i++) {
            NodeInteractionHandler comp = UIManager.Instance.activeNodes[i].GetComponent<NodeInteractionHandler>();

            if (comp.nodeType == (int)NodeType.TextDisplay) {
                comp.GetComponent<Node_TextDisplay>().UpdateData();
            }
            else if (comp.nodeType == (int)NodeType.Graph) {
                comp.GetComponent<Node_Graph>().UpdateData();
            }
            else if (comp.nodeType == (int)NodeType.Compass) {
                comp.GetComponent<Node_Compass>().UpdateData();
            }
            else if (comp.nodeType == (int)NodeType.ImageDisplay) {
                comp.GetComponent<Node_ImageDisplay>().UpdateData();
            }
        }
    }  

    // functions for grabbing values off of NT
    // --------------

    // get a double off networktables using a name
    public double FetchNTDouble(string key) {
        if (!gotTable) {return defaultDouble;}
        double val = table.GetEntry(key).GetDouble(defaultDouble);
        return val;
    }
    // get a string off networktables using a name
    public string FetchNTString(string key) {
        if (!gotTable) {return defaultString;}
        string val = table.GetEntry(key).GetString(defaultString);
        return val;
    }
    // get a boolean off networktables using a name
    public bool FetchNTBoolean(string key) {
        if (!gotTable) {return false;}
        bool val = table.GetEntry(key).GetBoolean(false);
        return val;
    }
    // --------------

    public void InitializeNetworkTablesClient() {
        Debug.Log("Starting networktables...");
        NetworkTableInstance.Default.StartClientTeam(2386);
    }

    public void CloseNetworkTables() {
        Debug.Log("Stopping networktables...");
        NetworkTableInstance.Default.StopClient();
    }

    // spawn all the necessary UI objects in the menu that allows you to select nodes from NetworkTables
    // the boolean array is for data types:
    // 0 == double?
    // 1 == string?
    // 2 == boolean? 
    // 3 == dropdown? // not implemented yet
    public void PopulateNodeSelectionMenu(bool[] requestedTypes) {
        // don't do anything if there's no connection
        if (!gotTable) {return;}

        string[] availableDoubleEntries = null;
        string[] availableStringEntries = null;
        string[] availableBooleanEntries = null;

        // grab the input list that's gonna be displaying the NT types
        UI_InputList inputList = CanvasUtils.SearchChildrenForName(UIManager.Instance.typeSelectionWindow, "element list").GetComponent<UI_InputList>();

        // make sure any previous elements are gotten rid of
        CanvasUtils.DestroyChildren(inputList.gameObject);

        inputList.inputPrefab = UIPrefabs.Instance.imagePrefab;

        // only grabbing available entries if the relevant type has been requested
        if (requestedTypes[0]) { // doubles
            availableDoubleEntries = table.GetKeys(NtType.Double).ToArray();

            for (int i = 0; i < availableDoubleEntries.Length; i++) {
                inputList.AddNewElement();
            }
        }
        if (requestedTypes[1]) { // strings
            availableStringEntries = table.GetKeys(NtType.String).ToArray();
        }
        if (requestedTypes[2]) { // booleans
            // will be represented using image display nodes
            availableBooleanEntries = table.GetKeys(NtType.Boolean).ToArray();
        }
        if (requestedTypes[3]) { // subtables set up as dropdowns (choosers)
            // soon
        }
    }
}
