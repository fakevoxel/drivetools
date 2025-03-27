using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Node_Field2D : MonoBehaviour
{
    // Data classes, unused rn but soon to be used when writing to disk
    public GenericNodeData genericData;
    public NodeData_Field2D data;

    // settings 
    public Color robotColor = Color.yellow;
    public Sprite fieldTexture;

    public NodeInteractionHandler interact;
    public Image fieldImage;
    public Image fieldBG;   

    // list of data classes for robot data
    public List<TrackedRobot> robots;
    public List<FieldMarker> markers;
    // all robot visual objects are children of this
    public Transform robotVisualContainer;
    public Transform markerVisualContainer;

    public float defaultBumperWidth;
    public float defaultBumperThickness;

    public void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Field2D;

        fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2(fieldTexture.texture.width, fieldTexture.texture.height);

        robots = new List<TrackedRobot>();
        AddTrackedRobot();

        markers = new List<FieldMarker>();
        AddFieldMarker();
    }

    void Update() {
        if ((interact.leftEdge - interact.rightEdge).magnitude < (interact.topEdge - interact.bottomEdge).magnitude * 2.17433234421365f) {
            fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.leftEdge - interact.rightEdge).magnitude, (interact.leftEdge - interact.rightEdge).magnitude * 0.4599112930740362f);
            fieldBG.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.leftEdge - interact.rightEdge).magnitude, (interact.leftEdge - interact.rightEdge).magnitude * 0.4599112930740362f);
        } else {
            fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.topEdge - interact.bottomEdge).magnitude * 2.17433234421365f, (interact.topEdge - interact.bottomEdge).magnitude);
            fieldBG.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.topEdge - interact.bottomEdge).magnitude * 2.17433234421365f, (interact.topEdge - interact.bottomEdge).magnitude);
        }

        if (robotVisualContainer.childCount != robots.Count) {
            CanvasUtils.DestroyChildren(robotVisualContainer.gameObject);

            for (int i = 0; i < robots.Count; i++) {
                GameObject newRobotVisual = Instantiate(UIPrefabs.Instance.robotPrefab, Vector3.zero, Quaternion.identity);

                newRobotVisual.transform.SetParent(robotVisualContainer);
            }
        }
        if (markerVisualContainer.childCount != markers.Count) {
            CanvasUtils.DestroyChildren(markerVisualContainer.gameObject);

            for (int i = 0; i < markers.Count; i++) {
                GameObject newMarkerVisual = Instantiate(UIPrefabs.Instance.imagePrefab, Vector3.zero, Quaternion.identity);

                newMarkerVisual.transform.SetParent(markerVisualContainer);
            }
        }

        for (int i = 0; i < robots.Count; i++) {
            if (!string.IsNullOrEmpty(robots[i].xSource) && !string.IsNullOrEmpty(robots[i].ySource) && !string.IsNullOrEmpty(robots[i].rotSource)) {
                robots[i].UpdateData();
            
                DrawRobot(i);
            }
        }
        for (int i = 0; i < markers.Count; i++) {
            if (!string.IsNullOrEmpty(markers[i].xSource) && !string.IsNullOrEmpty(markers[i].ySource) && !string.IsNullOrEmpty(markers[i].rotSource)) {
                markers[i].UpdateData();
            
                DrawMarker(i);
            }
        }
    }

    public void PopulateConfigMenu(GameObject window) {
        // button to add a new robot
        GameObject addRobotButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
        addRobotButton.transform.SetParent(window.transform.GetChild(4));
        addRobotButton.transform.localPosition = new Vector3(0, 200, 0);

        // we set up the button so that it adds a new robot, then re-initializes the menu
        addRobotButton.GetComponent<UI_Button>().onPress.AddListener(
                () => {
                    AddTrackedRobot();
                    CanvasUtils.DestroyChildren(window.transform.GetChild(4).gameObject);
                    this.PopulateConfigMenu(window);
                }
            );

        // tabs to manage what robot you're editing
        GameObject robotTabs = Instantiate(UIPrefabs.Instance.tabsPrefab, Vector3.zero, Quaternion.identity);
        robotTabs.transform.SetParent(window.transform.GetChild(4));
        robotTabs.transform.localPosition = Vector3.zero;
        
        // set up the tabs so there's a tab for each robot
        robotTabs.GetComponent<UI_Tabs>().InitializeTabs(robots.Count, 125);

        // object to manage which input fields (for robot data) are shown
        GameObject robotSetManager = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        robotSetManager.transform.SetParent(window.transform.GetChild(4));

        // we set up the set manager to work with 
        robotSetManager.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[robots.Count];
        robotSetManager.GetComponent<UI_ObjectSetManager>().controllingTabs = robotTabs.GetComponent<UI_Tabs>();
        robotSetManager.GetComponent<UI_ObjectSetManager>().Initialize();

        // loop through all the robots and spawn some input fields
        // five fields for each: (xSource, ySource, rotSource, bumperWidth, bumperThickness)
        for (int i = 0; i < robots.Count; i++) {
            int j = i;

            // the xSource input
            GameObject xInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            xInput.transform.SetParent(window.transform.GetChild(4));
            xInput.transform.localPosition = new Vector3(0, -75, 0);
            xInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetRobotX(value, j)
            );

            // the ySource input
            GameObject yInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            yInput.transform.SetParent(window.transform.GetChild(4));
            yInput.transform.localPosition = new Vector3(0, -150, 0);
            yInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetRobotY(value, j)
            );

            // the rotSource input
            GameObject zInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            zInput.transform.SetParent(window.transform.GetChild(4));
            zInput.transform.localPosition = new Vector3(0, -225, 0);
            zInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetRobotRot(value, j)
            );

            // the rotSource input
            GameObject widthInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            widthInput.transform.SetParent(window.transform.GetChild(4));
            widthInput.transform.localPosition = new Vector3(0, -300, 0);
            widthInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetRobotWidth(float.Parse(value), j)
            );

            // the rotSource input
            GameObject thicknessInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            thicknessInput.transform.SetParent(window.transform.GetChild(4));
            thicknessInput.transform.localPosition = new Vector3(0, -375, 0);
            thicknessInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetRobotThickness(float.Parse(value), j)
            );

            robotSetManager.GetComponent<UI_ObjectSetManager>().sets[j] = new UI_ObjectSet(new GameObject[]{
                xInput, yInput, zInput, widthInput, thicknessInput
            });
        }

        // button to add a new robot
        GameObject addMarkerButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
        addMarkerButton.transform.SetParent(window.transform.GetChild(4));
        addMarkerButton.transform.localPosition = new Vector3(200, 200, 0);

        // we set up the button so that it adds a new robot, then re-initializes the menu
        addMarkerButton.GetComponent<UI_Button>().onPress.AddListener(
                () => {
                    AddFieldMarker();
                    CanvasUtils.DestroyChildren(window.transform.GetChild(4).gameObject);
                    this.PopulateConfigMenu(window);
                }
            );

        // another set of tabs for field markers (highlighted positions on the field)
        GameObject markerTabs = Instantiate(UIPrefabs.Instance.tabsPrefab, Vector3.zero, Quaternion.identity);
        markerTabs.transform.SetParent(window.transform.GetChild(4));
        markerTabs.transform.localPosition = new Vector3(0, -400, 0);;
        
        // set up the tabs so there's a tab for each robot
        markerTabs.GetComponent<UI_Tabs>().InitializeTabs(markers.Count, 125);

        // object to manage which input fields (for robot data) are shown
        GameObject markerSetManager = Instantiate(UIPrefabs.Instance.objectSetPrefab, Vector3.zero, Quaternion.identity);
        markerSetManager.transform.SetParent(window.transform.GetChild(4));

        // we set up the set manager to work with 
        markerSetManager.GetComponent<UI_ObjectSetManager>().sets = new UI_ObjectSet[markers.Count];
        markerSetManager.GetComponent<UI_ObjectSetManager>().controllingTabs = markerTabs.GetComponent<UI_Tabs>();
        markerSetManager.GetComponent<UI_ObjectSetManager>().Initialize();

        // loop through all the markers and spawn some input fields and an image select button
        // three fields for each: (xSource, ySource, rotSource, size)
        for (int i = 0; i < markers.Count; i++) {
            int j = i;

            // the xSource input
            GameObject xInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            xInput.transform.SetParent(window.transform.GetChild(4));
            xInput.transform.localPosition = new Vector3(0, -450, 0);
            xInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetMarkerX(value, j)
            );

            // the ySource input
            GameObject yInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            yInput.transform.SetParent(window.transform.GetChild(4));
            yInput.transform.localPosition = new Vector3(0, -500, 0);
            yInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetMarkerY(value, j)
            );

            // the rotSource input
            GameObject zInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            zInput.transform.SetParent(window.transform.GetChild(4));
            zInput.transform.localPosition = new Vector3(0, -550, 0);
            zInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetMarkerRot(value, j)
            );

            // the rotSource input
            GameObject sizeInput = Instantiate(UIPrefabs.Instance.inputFieldPrefab, Vector3.zero, Quaternion.identity);
            sizeInput.transform.SetParent(window.transform.GetChild(4));
            sizeInput.transform.localPosition = new Vector3(0, -600, 0);
            sizeInput.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => SetMarkerSize(float.Parse(value), j)
            );

            // a button to select what image to use for the marker
            GameObject imageButton = Instantiate(UIPrefabs.Instance.buttonPrefab, Vector3.zero, Quaternion.identity);
            imageButton.transform.SetParent(window.transform.GetChild(4));
            imageButton.transform.localPosition = new Vector3(200, -600, 0);
            imageButton.GetComponent<UI_Button>().onPress.AddListener(
                () => UIManager.Instance.GrabAssetName(
                    (imageName) => SetMarkerImage(imageName, j)
                )
            );

            markerSetManager.GetComponent<UI_ObjectSetManager>().sets[j] = new UI_ObjectSet(new GameObject[]{
                xInput, yInput, zInput, sizeInput, imageButton
            });
        }
    }

    public void AddFieldMarker() {
        markers.Add(new FieldMarker());
    }

    // adds a container for tracked robot data
    public void AddTrackedRobot() {
        robots.Add(new TrackedRobot(defaultBumperWidth, defaultBumperThickness));
        
        GameObject newRobotVisual = Instantiate(UIPrefabs.Instance.robotPrefab, Vector3.zero, Quaternion.identity);

        newRobotVisual.transform.SetParent(robotVisualContainer);
    }

    public void SetMarkerX(string input, int markerIndex) {
        markers[markerIndex].xSource = input;
    }
    public void SetMarkerY(string input, int markerIndex) {
        markers[markerIndex].ySource = input;
    }
    public void SetMarkerRot(string input, int markerIndex) {
        markers[markerIndex].rotSource = input;
    }
    public void SetMarkerSize(float markerSize, int markerIndex) {
        markers[markerIndex].size = markerSize;
    }
    public void SetMarkerImage(string imageName, int markerIndex) {
        markers[markerIndex].imageName = imageName;
        UIManager.Instance.assetManagerWindow.SetActive(false);
    }

    // setting data for a robot
    public void SetRobotX(string input, int robotIndex) {
        robots[robotIndex].xSource = input;
    }
    public void SetRobotY(string input, int robotIndex) {
        robots[robotIndex].ySource = input;
    }
    public void SetRobotRot(string input, int robotIndex) {
        robots[robotIndex].rotSource = input;
    }
    // change the actual size of a robot
    public void SetRobotWidth(float bumperWidth, int robotIndex) {
        robots[robotIndex].bumperWidth = bumperWidth;
    }
    // change the line thickness (like brush width) of a robot
    public void SetRobotThickness(float bumperThickness, int robotIndex) {
        robots[robotIndex].bumperThickness = bumperThickness;
    }

    float MetersToPixels(float input) {
        return input / (8.0518f / (fieldImage.GetComponent<RectTransform>().sizeDelta.y));
    }

    // draw a given tracked robot
    public void DrawRobot(int index) {
        Transform robotVisual = robotVisualContainer.GetChild(index);
        TrackedRobot robotData = robots[index];

        RectTransform rect = fieldImage.GetComponent<RectTransform>();

        robotVisual.position = fieldImage.transform.position - new Vector3(rect.sizeDelta.x/2f, rect.sizeDelta.y/2f, 0)
        + new Vector3(MetersToPixels(robotData.xPos), MetersToPixels(robotData.yPos), 0);
        robotVisual.rotation = Quaternion.Euler(new Vector3(0, 0, robotData.rot));
        
        for (int i = 0; i < 5; i++) {
            RectTransform component = robotVisual.GetChild(i).GetComponent<RectTransform>();

            if (i == 4) {
                component.sizeDelta = new Vector2(20, 20);
            } else {
                component.sizeDelta = new Vector2(MetersToPixels(robotData.bumperWidth), MetersToPixels(robotData.bumperThickness));
            }
            component.GetComponent<Image>().color = robotColor;

            if (i==0) {
                component.localPosition = new Vector3(0, MetersToPixels(robotData.bumperWidth/2 - robotData.bumperThickness/2), 0);
                component.localRotation = Quaternion.Euler(0, 0, 0);
            } else if (i==1) {
                component.localPosition = new Vector3(0, MetersToPixels(-robotData.bumperWidth/2 + robotData.bumperThickness/2), 0);
                component.localRotation = Quaternion.Euler(0, 0, 0);
            } else if (i==2) {
                component.localPosition = new Vector3(MetersToPixels(robotData.bumperWidth/2 - robotData.bumperThickness/2), 0, 0);
                component.localRotation = Quaternion.Euler(0, 0, 90);
            } else if (i==3) {
                component.localPosition = new Vector3(MetersToPixels(-robotData.bumperWidth/2 + robotData.bumperThickness/2), 0, 0);
                component.localRotation = Quaternion.Euler(0, 0, 90);
            } else if (i==4) {
                component.localPosition = new Vector3(MetersToPixels(robotData.bumperWidth/2 - robotData.bumperThickness/2), 0, 0);
                component.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public void DrawMarker(int index) {
        Transform markerVisual = markerVisualContainer.GetChild(index);
        FieldMarker markerData = markers[index];

        RectTransform rect = fieldImage.GetComponent<RectTransform>();

        markerVisual.position = fieldImage.transform.position - new Vector3(rect.sizeDelta.x/2f, rect.sizeDelta.y/2f, 0)
        + new Vector3(MetersToPixels(markerData.xPos), MetersToPixels(markerData.yPos), 0);
        markerVisual.rotation = Quaternion.Euler(new Vector3(0, 0, markerData.rot));
        
        markerVisual.GetComponent<RectTransform>().sizeDelta = Vector2.one * MetersToPixels(markerData.size);
        markerVisual.GetComponent<RawImage>().texture = AssetManager.Instance.GetImageOfName(markerData.imageName);
    }

    public void PopulateDataClass() {
        // robot data
        data.robots = robots;
        data.markers = markers;

        data.generic = new GenericNodeData(
            transform.position.x,
            transform.position.y,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x,
            transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y,
            interact.isNodeTracked
        );
    }
}
