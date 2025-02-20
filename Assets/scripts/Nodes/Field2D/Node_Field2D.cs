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
    public float robotWidth;
    public float bumperWidth;
    public Color robotColor = Color.yellow;
    public Sprite fieldTexture;

    public NodeInteractionHandler interact;
    public Image fieldImage;
    public Image fieldBG;   

    // list of data classes for robot data
    public List<TrackedRobot> robots;
    // all robot visual objects are children of this
    public Transform robotVisualContainer;
    // all robot readouts (text displays of data) are children of this
    public Transform robotReadoutContainer;

    public void Awake() {
        // getting the interactionHandler component
        interact = GetComponent<NodeInteractionHandler>();

        interact.nodeName = name;
        interact.nodeType = (int)NodeType.Field2D;

        CanvasUtils.SearchChildrenForName(gameObject, "edit button").GetComponent<UI_Button>().onPress.AddListener(
            () => UIManager.Instance.OpenNodeConfig(interact)
        );

        fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2(fieldTexture.texture.width, fieldTexture.texture.height);
    }

    void Update() {
        if ((interact.leftEdge - interact.rightEdge).magnitude < (interact.topEdge - interact.bottomEdge).magnitude * 2.181043663471778f) {
            fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.leftEdge - interact.rightEdge).magnitude, (interact.leftEdge - interact.rightEdge).magnitude * 0.45849609375f);
            fieldBG.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.leftEdge - interact.rightEdge).magnitude, (interact.leftEdge - interact.rightEdge).magnitude * 0.45849609375f);
        } else {
            fieldImage.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.topEdge - interact.bottomEdge).magnitude * 2.181043663471778f, (interact.topEdge - interact.bottomEdge).magnitude);
            fieldBG.GetComponent<RectTransform>().sizeDelta = new Vector2((interact.topEdge - interact.bottomEdge).magnitude * 2.181043663471778f, (interact.topEdge - interact.bottomEdge).magnitude);
        }

        CanvasUtils.DestroyChildren(robotReadoutContainer.gameObject);
        for (int i = 0; i < robots.Count; i++) {
            if (!string.IsNullOrEmpty(robots[i].xSource) && !string.IsNullOrEmpty(robots[i].ySource) && !string.IsNullOrEmpty(robots[i].rotSource)) {
                robots[i].UpdateData();
            
                DrawRobot(i);

                // FINISH THIS CODE FOR CREATING READOUTS

                // x axis (display in meters)
                TextMeshProUGUI xReadout = Instantiate(
                    UIManager.Instance.textPrefab, 
                    Vector3.zero, 
                    Quaternion.identity).GetComponent<TextMeshProUGUI>();
                xReadout.transform.position = interact.bottomLeftTransform.position + 
                new Vector3(200, (robots.Count * 3 - (i * 3 + 0)) * 50, 0);
                xReadout.text = MathUtils.ToDegrees(robots[i].xPos).ToString();

                // y axis (display in meters)
                TextMeshProUGUI yReadout = Instantiate(
                    UIManager.Instance.textPrefab, 
                    Vector3.zero, 
                    Quaternion.identity).GetComponent<TextMeshProUGUI>();
                yReadout.transform.position = interact.bottomLeftTransform.position + 
                new Vector3(200, (robots.Count * 3 - (i * 3 + 1)) * 50, 0);
                yReadout.text = MathUtils.ToDegrees(robots[i].yPos).ToString();

                // rotation (display in degrees)
                TextMeshProUGUI rotReadout = Instantiate(
                    UIManager.Instance.textPrefab, 
                    Vector3.zero, 
                    Quaternion.identity).GetComponent<TextMeshProUGUI>();
                rotReadout.transform.position = interact.bottomLeftTransform.position + 
                new Vector3(200, (robots.Count * 3 - (i * 3 + 2)) * 50, 0);
                rotReadout.text = MathUtils.ToDegrees(robots[i].rot).ToString();
            }
        }
    }

    // adds a container for tracked robot data
    public void AddTrackedRobot() {

    }

    public void SetX(string input, int robotIndex) {
        robots[robotIndex].xSource = input;
    }
    public void SetX(TMP_InputField input, int robotIndex) {
        SetX(input.text, robotIndex);
    }
    public void SetY(string input, int robotIndex) {
        robots[robotIndex].ySource = input;
    }
    public void SetY(TMP_InputField input, int robotIndex) {
        SetY(input.text, robotIndex);
    }
    public void SetRot(string input, int robotIndex) {
        robots[robotIndex].rotSource = input;
    }
    public void SetRot(TMP_InputField input, int robotIndex) {
        SetRot(input.text, robotIndex);
    }

    float MetersToPixels(float input) {
        return input / (8.0518f / (fieldImage.GetComponent<RectTransform>().sizeDelta.y));
    }

    // update every single tracked robot
    public void UpdateAllRobots() {
        for (int i = 0; i < robots.Count; i++) {
            DrawRobot(i);
        }
    }

    // draw a given tracked robot
    public void DrawRobot(int index) {
        Transform robotVisual = robotVisualContainer.GetChild(index);
        TrackedRobot robotData = robots[index];

        RectTransform rect = fieldImage.GetComponent<RectTransform>();

        robotVisual.position = fieldImage.transform.position - new Vector3(rect.sizeDelta.x/2f, rect.sizeDelta.y/2f, 0)
        + new Vector3(MetersToPixels(robotData.xPos), MetersToPixels(robotData.yPos), 0);
        robotVisual.rotation = Quaternion.Euler(new Vector3(0, 0, robotData.rot));
        
        for (int i = 0; i < 4; i++) {
            RectTransform component = robotVisual.GetChild(i).GetComponent<RectTransform>();

            component.sizeDelta = new Vector2(MetersToPixels(robotData.bumperWidth), MetersToPixels(robotData.bumperThickness));
            component.GetComponent<Image>().color = robotColor;

            if (i==0) {
                component.localPosition = new Vector3(0, MetersToPixels(robotData.bumperWidth/2 - robotData.bumperThickness), 0);
                component.localRotation = Quaternion.Euler(0, 0, 0);
            } else if (i==1) {
                component.localPosition = new Vector3(0, MetersToPixels(-robotData.bumperWidth/2 + robotData.bumperThickness), 0);
                component.localRotation = Quaternion.Euler(0, 0, 0);
            } else if (i==2) {
                component.localPosition = new Vector3(MetersToPixels(robotData.bumperWidth/2 - robotData.bumperThickness), 0, 0);
                component.localRotation = Quaternion.Euler(0, 0, 90);
            } else if (i==3) {
                component.localPosition = new Vector3(MetersToPixels(-robotData.bumperWidth/2 +  - robotData.bumperThickness), 0, 0);
                component.localRotation = Quaternion.Euler(0, 0, 90);
            }
        }
    }
}
