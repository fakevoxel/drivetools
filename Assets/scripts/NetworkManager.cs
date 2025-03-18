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
    string defaultString = "null";
    double defaultDouble = 0;
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

            if (comp.nodeType == (int)NodeType.Double) {
                comp.GetComponent<Node_Double>().UpdateData();
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

    public void InitializeNetworkTablesClient() {
        Debug.Log("Starting networktables...");
        NetworkTableInstance.Default.StartClientTeam(2386);
    }

    public void CloseNetworkTables() {
        Debug.Log("Stopping networktables...");
        NetworkTableInstance.Default.StopClient();
    }

    // spawn all the necessary UI objects in the menu that allows you to select nodes from NetworkTables
    public void PopulateNodeSelectionMenu() {
        if (!gotTable) {return;}

        string[] availableDoubleEntries = table.GetKeys(NtType.Double).ToArray();
        string[] availableStringEntries = table.GetKeys(NtType.String).ToArray();
        string[] availableBooleanEntries = table.GetKeys(NtType.Boolean).ToArray(); // will be represented using image display nodes
    }
}
