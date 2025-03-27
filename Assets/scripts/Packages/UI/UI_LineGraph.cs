using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Linq.Expressions;

// this requires a LineRenderer component
// this only plots in 2D, obviously
public class UI_LineGraph : MonoBehaviour
{
    public Transform pointContainer;
    public UILineRenderer lineRenderer;

    public List<Vector2> points;

    // what value should the graph start at
    // if using time on the x-axis for example, you might not want to start at t = 0
    public float startValue;

    private float scaleFactorX;
    private float scaleFactorY;
    private int tallestPointIndex;
    private int shortestPointIndex;

    public GameObject pointDataWidget;

    [Header("Settings")]
    // whether the individual points show up
    public bool pointsVisible;
    // whether the points are connected by lines
    public bool lineVisible;

    // test
    public GameObject pointPrefab;
    //--

    void Start() {
        points = new List<Vector2>();

        scaleFactorX = 1;
        scaleFactorY = 1;

        tallestPointIndex = -1;
        shortestPointIndex = -1;
    }

    void Update() {
        RectTransform backgroundImage = transform.GetChild(0).GetComponent<RectTransform>();
        pointContainer.localPosition = new Vector3(backgroundImage.sizeDelta.x / -2, backgroundImage.sizeDelta.y / -2, 0);
    }

    // takes in a vector2 representing (x, y) and plots it to the graph
    public void PlotPoint(Vector2 point) {
        points.Add(point);

        if (tallestPointIndex == -1) {
            tallestPointIndex = points.Count - 1;
        } else if (point.y > points[tallestPointIndex].y) {
            tallestPointIndex = points.Count - 1;
        }
        if (shortestPointIndex == -1) {
            shortestPointIndex = points.Count - 1;
        } else if (point.y < points[shortestPointIndex].y) {
            shortestPointIndex = points.Count - 1;
        }

        if (points.Count > 1) {
            scaleFactorY = points[tallestPointIndex].y - points[shortestPointIndex].y;
            scaleFactorX = points[points.Count - 1].x - points[0].x;
        }

        // re-draw the points
        RefreshPoints();

        // re-draw the line
        RefreshLineRenderer();
    }

    // display the coordinates of whatever point they're hovering over to the user
    public void ShowPointData(int i) {
        pointDataWidget.SetActive(true);
        
        Transform pointTransform = pointContainer.GetChild(i);

        pointDataWidget.transform.position = new Vector3(pointTransform.position.x, pointTransform.position.y, 0);

        pointDataWidget.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = 
        "(" + points[i].x.ToString() + ", " + points[i].y.ToString() + ")";
    }
    
    public void HidePointData() {
        pointDataWidget.SetActive(false);
    }

    // re-draws all the points on the graph
    public void RefreshPoints() {
        for (int i = 0; i < pointContainer.childCount; i++) {
            Destroy(pointContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < points.Count; i++) {
            GameObject newPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);

            newPoint.transform.SetParent(pointContainer.transform);
            
            newPoint.GetComponent<UI_GraphPoint>().parentGraph = this;
            newPoint.GetComponent<UI_GraphPoint>().pointIndex = i;

            // gotta multiply this by a scale factor
            newPoint.transform.position = pointContainer.position +
            new Vector3((points[i].x - points[0].x) / scaleFactorX * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, 
            (points[i].y - points[shortestPointIndex].y) / scaleFactorY * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y, 0);
        }
    }

    // updates the line renderer component so it connects all the points
    public void RefreshLineRenderer() {
        lineRenderer.Points = new Vector2[points.Count];

        // loop through all the points, and set the position on the line renderer
        for (int i = 0; i < points.Count; i++) {
            lineRenderer.Points[i] = new Vector3(Screen.width / 2, Screen.height / 2) + new Vector3(transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / -2, transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / -2) +
            new Vector3((points[i].x - points[0].x) / scaleFactorX * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, 
            (points[i].y - points[shortestPointIndex].y) / scaleFactorY * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y, 0);
        }
    }
}
