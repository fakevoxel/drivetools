using UnityEngine;
using NetworkTables;
using TMPro;
using UnityEngine.UI;

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
    string defaultString = "connectionloss";
    double defaultDouble = -9999;

    void Update() {
        if (!isNetworkRunning) {
            InitializeNetworkTablesClient();
            isNetworkRunning = true;
        }

        // Right now using the smartdashboard table only
        // TODO: add functionality to switch between tables
        table = NetworkTable.GetTable("SmartDashboard");

        if (table.IsConnected) {
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
        double val = table.GetNumber(key, defaultDouble);
        return val;
    }
    // get a string off networktables using a name
    public string FetchNTString(string key) {
        string val = table.GetString(key, defaultString);
        return val;
    }

    public void InitializeNetworkTablesClient() {
        NetworkTable.SetClientMode();
        NetworkTable.SetTeam(AppData.Instance.teamNumber); 
        NetworkTable.SetIPAddress(AppData.Instance.robotRadioIP);
        NetworkTable.SetDSClientEnabled(true);
        NetworkTable.Initialize();
    }
}
