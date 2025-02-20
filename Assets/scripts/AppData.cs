using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class AppData : MonoBehaviour
{
    private static AppData _instance;

    public static AppData Instance {
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

    // static variables
    public static int layoutTabSpacing = 120;
    public static int hardMinFrameRate = 24;
    public static int hardMaxFrameRate = 128;

    // variables
    public int targetFrameRate;
    public List<DashboardLayout> layouts;
    public int teamNumber;
    public string robotRadioIP;

    public GameObject[] nodePrefabs;

    public TMP_InputField teamNumberInput;
    public TMP_InputField frameRateInput;

    void Start() {
        LoadSettingsFromDisk();
        LoadLayoutFromDisk();
        UIManager.Instance.RefreshAppData();
    }

    public GameObject GetPrefabObject(string key) {
        for (int i = 0; i < nodePrefabs.Length; i++) {
            if (nodePrefabs[i].name == key) {
                return nodePrefabs[i];
            }
        }

        Debug.Log("ERROR: tried to get a prefab object that doesn't exist!");

        return null;
    }

    // grab a node prefab using the index in the array
    public GameObject GetPrefabObject(int key) {
        if (key < nodePrefabs.Length && key >= 0) {
            return nodePrefabs[key];
        }
        else {
            Debug.Log("ERROR: looking for node prefab outside of array!");
            return null;
        }
    }

    // revert all changes made by the user
    // TODO: set color palette here
    public void LoadFactoryDefaults() {
        layouts = new List<DashboardLayout>() {new DashboardLayout("Main")};

        UpdateTargetFrameRate(60);
        SetTeamNumber(2386);
    }

    public void SetTeamNumber(TMP_InputField input) {
        int parsedText;
        if (!int.TryParse(input.text, out parsedText)) {return;}
        SetTeamNumber(int.Parse(input.text));
    }
    public void SetTeamNumber(int teamNumber) {
        this.teamNumber = teamNumber;
        robotRadioIP = "10." + teamNumber.ToString().Substring(0, 2) + "." + teamNumber.ToString().Substring(2, 2) + ".2";

        NetworkManager.Instance.ipDisplay.text = "Robot IP: " + robotRadioIP;

        teamNumberInput.text = teamNumber.ToString();
    }

    public void UpdateTargetFrameRate(TMP_InputField input) {
        int parsedText;
        if (!int.TryParse(input.text, out parsedText)) {return;}
        parsedText = Mathf.Clamp(parsedText, hardMinFrameRate, hardMaxFrameRate);
        UpdateTargetFrameRate(parsedText);
    }

    public void UpdateTargetFrameRate(int updatedValue) {
        updatedValue = Mathf.Clamp(updatedValue, hardMinFrameRate, hardMaxFrameRate);
        targetFrameRate = updatedValue;
        UIManager.Instance.RefreshAppData();

        frameRateInput.text = updatedValue.ToString();
    }

    void OnApplicationQuit() {
        WriteSettingsToDisk();
    }

    // writing application data to disk
    public void WriteSettingsToDisk() {
        PersistentSettings prefs = new PersistentSettings(
            (ushort)teamNumber,
            (ushort)targetFrameRate
        );

        SaveUtils.SavePreferences(prefs);
    }

    // loading the file from disk and setting the variables
    public void LoadSettingsFromDisk() {
        PersistentSettings prefs = SaveUtils.LoadPreferences();
        if (prefs == null) {return;}

        teamNumber = prefs.teamNumber;
        targetFrameRate = prefs.targetFrameRate;
    }

    public void LoadLayoutFromDisk() {
        DashboardLayout[] layoutArray = SaveUtils.LoadLayouts();
        List<DashboardLayout> layoutList = new List<DashboardLayout>();
        if (layoutArray == null) {LoadFactoryDefaults(); return;}
        for (int i = 0; i < layoutArray.Length; i++) {
            layoutList.Add(layoutArray[i]);
        }

        layouts = layoutList;
        UIManager.Instance.ResetAndLoadAllLayouts();
        UIManager.Instance.SwitchToLayout(0);
    }
}
