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

    public Image isConnectedIcon;
    public TextMeshProUGUI ipDisplay;
    public bool isNetworkRunning;
    NetworkTable table;

    // these values get send to the nodes if there is no connection
    string defaultString = "null";
    double defaultDouble = 0;

    void Update() {
        if (!isNetworkRunning) {
            InitializeNetworkTablesClient();
            isNetworkRunning = true;
        }

        // was gonna test a bug that killed photonvision
        //Debug.Log(NetworkTable.Connections().Count);

        // Right now using the smartdashboard table only
        // TODO: add functionality to switch between tables
        if (NetworkTableInstance.Default.GetConnections().ToArray().Length > 0) {
            table = NetworkTableInstance.Default.GetTable("SmartDashboard");

            UpdateAllNodeData(table);
            isConnectedIcon.color = Color.green;
            
            string[] keys = new string[table.GetSubTable("Driver Select").GetKeys().Count];
            table.GetSubTable("Driver Select").GetKeys().CopyTo(keys);

            // if (keys.Length > 0) {
            //     Debug.Log(keys[4]);
            //     Debug.Log(table.GetSubTable("Driver Select").GetString(".type"));
            // }
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

    public void RetrieveAllNodes() {
        string[] doubleKeys = new string[table.GetKeys(NtType.Double).Count];
        table.GetKeys(NtType.Double).CopyTo(doubleKeys);
        
        for (int i = 0; i < doubleKeys.Length; i++) {
            if (!UIManager.Instance.HasActiveNodeWithSource((int)NodeType.Double, doubleKeys[i])) {
                UIManager.Instance.SpawnAndPlaceNewNode((int)NodeType.Double, doubleKeys[i]);
            }
        }
    }

    // get a double off networktables using a name
    public double FetchNTDouble(string key) {
        if (table == null || NetworkTableInstance.Default.GetConnections().ToArray().Length <= 0) {return defaultDouble;}
        double val = table.GetEntry(key).GetDouble(defaultDouble);
        return val;
    }
    // get a string off networktables using a name
    public string FetchNTString(string key) {
        if (table == null || NetworkTableInstance.Default.GetConnections().ToArray().Length <= 0) {return defaultString;}
        string val = table.GetEntry(key).GetString(defaultString);
        return val;
    }

    public void InitializeNetworkTablesClient() {
        InitializeNetworkTablesClient();
    }

    public void CloseNetworkTables() {
        CloseNetworkTables();
    }
}
