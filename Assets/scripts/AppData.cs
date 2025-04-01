using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    public List<string> imageAssetDirectories;
    public int teamNumber;
    public string robotRadioIP;

    public GameObject[] nodePrefabs;

    public TMP_InputField teamNumberInput;
    public TMP_InputField frameRateInput;
    // The currently stored colors for the software
    // there are four colors here
    // background color 1 (dashboard background, window background)
    // background color 2 (toolbar background)
    // tracked node color (tracked nodes)
    // interactable color (buttons, node handles)
    public Color[] colorPalette;
    public Color[] oldColorPalette; // when changing color palette, its useful to know the old colors
    // what colors are used in the editor
    public Color[] editorColors = new Color[] {
        Color.white,
        Color.red,
        Color.green,
        Color.blue,
    };

    private float refreshTime;
    private float refreshTimer;

    // the color pickers used to change the theme
    public UI_ColorPicker[] colorPickers;
    public UI_Slider[] colorSliders;

    // sketchy stuff for color palette
    private Image[] imageComponentsInScene;
    private int[] colorIndices;

    void Start() {
        refreshTimer = 0.25f;
        refreshTime = -1;

        SetupColorPickers();
        
        LoadSettingsFromDisk();
        LoadLayoutFromDisk();
        UIManager.Instance.RefreshAppData();

        oldColorPalette = editorColors;
        UpdateTrackedImageComponents();
        ApplyColorPalette();
    }

    void SetupColorPickers() {
        for (int i = 0; i < colorPickers.Length; i++) {
            int j = i;

            colorPickers[i].Initialize();
            colorSliders[i].Initialize();

            colorPickers[i].onEndInteraction.AddListener((color) => SetColorOfIndex(j, color));
            colorSliders[i].onEndInteraction.AddListener(() => SetColorOfIndex(j, colorPickers[j].GetColor()));
        }
    }

    void Update()
    {
        if (refreshTime + refreshTimer < Time.time && refreshTime != -1) {
            refreshTime = -1;
           
            ApplyColorsToNewObjects();
        }
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
        // do SOMETHING with the asset directories
        LoadColorPalette(colorPalette);
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
            (ushort)targetFrameRate,
            imageAssetDirectories.ToArray(),
            colorPalette
        );

        SaveUtils.SavePreferences(prefs);
    }

    // loading the file from disk and setting the variables
    public void LoadSettingsFromDisk() {
        PersistentSettings prefs = SaveUtils.LoadPreferences();
        if (prefs == null) {return;}

        SetTeamNumber(prefs.teamNumber);
        UpdateTargetFrameRate(prefs.targetFrameRate);
        imageAssetDirectories = prefs.imageAssetDirectories.ToList();
        LoadColorPalette(SerializableColor.CreateColorArray(prefs.colorPalette));
       
        AssetManager.Instance.PopulateAssets();
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

    // these two functions are weird
    // basically, when a node is placed this first function is called, which starts a timer
    // the timer allows other code to finish running, then the node's color palette is set to the current palette
    // thats what the second function is for, setting the palette
    public void RefreshObjectColors() {
        refreshTime = Time.time;
    }
    public void ApplyColorsToNewObjects() {
        UpdateTrackedImageComponents();
        ApplyColorPalette();
    }
    // --------------

    // modify one of the colors in the palette
    // this function is called by the color picker components in the settings menu
    public void SetColorOfIndex(int colorIndex, Color newColor) {
        colorPalette[colorIndex] = newColor;

        ApplyColorPalette();
    }

    // setting the color palette array, and properly initializing the sliders and color pickers
    public void LoadColorPalette(Color[] palette) {
        colorPalette = palette;

        for (int i = 0; i < palette.Length; i++) {
            colorPickers[i].SetColor(palette[i]);

            float h;
            float s;
            float v;

            // getting hsv data from the Color class
            Color.RGBToHSV(palette[i], out h, out s, out v);

            colorSliders[i].SetValue(h);
        }
    }
    
    // keeping track of all image components in the scene
    public void UpdateTrackedImageComponents() {
        // first we set all object's colors back to the editor colors
        // since any newly spawned objects will have their editor colors already, this simplifies things
        // it means we just have to look at editor colors to figure out what objects to change
        if (imageComponentsInScene != null) {
            for (int i = 0; i < colorIndices.Length; i++) {
                // sometimes the color index of an object is -1, meaning it should remain unaffected
                // removing this obv causes an index error
                if (colorIndices[i] == -1) {continue;}

                if (imageComponentsInScene[i] == null) {continue;}
                
                // this reverts back to a given editor color
                imageComponentsInScene[i].color = editorColors[colorIndices[i]];
            }
        }

        // grabbing every single unity Image class in the scene
        // it's not amazing, but it does work and it requires no work from me :)
        Image[] allImageComponents = UIManager.Instance.canvasTransform.GetComponentsInChildren<Image>(true);

        imageComponentsInScene = new Image[allImageComponents.Length];
        colorIndices = new int[allImageComponents.Length];

        for (int i = 0; i < allImageComponents.Length; i++) {
            imageComponentsInScene[i] = allImageComponents[i];
            colorIndices[i] = -1;

            for (int j = 0; j < 4; j++) {
                // since we reset everything back to editor colors (see above),
                // we just have to look at the editorColors array
                if (imageComponentsInScene[i].color == editorColors[j]) {
                    colorIndices[i] = j;
                }
            }
        }
    }

    // swap all game objects from one set of colors (oldColors) to another (newColors)
    public void ApplyColorPalette() {
        for (int i = 0; i < imageComponentsInScene.Length; i++) {
            if (colorIndices[i] == -1) {continue;}

            imageComponentsInScene[i].color = colorPalette[colorIndices[i]];

            if (colorIndices[i] == 0 && imageComponentsInScene[i].transform.parent != UIManager.Instance.canvasTransform) {
                // this object is a window
                imageComponentsInScene[i].color = colorPalette[colorIndices[i]] * 1.5f;
            }

            // changing button default colors
            if (imageComponentsInScene[i].GetComponent<UI_Button>() != null) {
                imageComponentsInScene[i].GetComponent<UI_Button>().defaultColor = colorPalette[colorIndices[i]] * 0.6f;
                imageComponentsInScene[i].GetComponent<UI_Button>().hoverColor = colorPalette[colorIndices[i]] * 0.9f;
                imageComponentsInScene[i].GetComponent<UI_Button>().pressedColor = colorPalette[colorIndices[i]] * 1;
            }

            // silly layered if statement
            if (imageComponentsInScene[i].transform.parent != null) {
                if (imageComponentsInScene[i].transform.parent.parent != null) {
                    // this if statement was added specifically to deal with the settings tabs,
                    // but if the parent/child structure is consistent then it will work for all tabs
                    if (imageComponentsInScene[i].transform.parent.parent.GetComponent<UI_Tabs>() != null) {
                        imageComponentsInScene[i].transform.parent.parent.GetComponent<UI_Tabs>().defaultColor = colorPalette[colorIndices[i]] * 0.6f;
                        imageComponentsInScene[i].transform.parent.parent.GetComponent<UI_Tabs>().hoverColor = colorPalette[colorIndices[i]] * 0.7f;
                        imageComponentsInScene[i].transform.parent.parent.GetComponent<UI_Tabs>().selectedColor = colorPalette[colorIndices[i]] * 0.8f;
                    }
                }
            }

            if (imageComponentsInScene[i].gameObject.name == "toolbar") {
                imageComponentsInScene[i].color = colorPalette[colorIndices[i]];
            }
        }

        oldColorPalette = colorPalette;
    }
}
