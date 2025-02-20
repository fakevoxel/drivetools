using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

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

    private float scaleFactor;

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

        RectTransform backgroundImage = transform.GetChild(0).GetComponent<RectTransform>();

        scaleFactor = 1;
        pointContainer.localPosition = new Vector3(backgroundImage.sizeDelta.x / -2, backgroundImage.sizeDelta.y / -2, 0);
    }

    // takes in a vector2 representing (x, y) and plots it to the graph
    public void PlotPoint(Vector2 point) {
        points.Add(point);

        if (points.Count > 1) {
            scaleFactor = points[points.Count - 1].x - points[0].x;
        }

        for (int i = 0; i < pointContainer.childCount; i++) {
            Destroy(pointContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < points.Count; i++) {
            GameObject newPoint = Instantiate(pointPrefab, Vector3.zero, Quaternion.identity);

            newPoint.transform.SetParent(pointContainer.transform);

            // gotta multiply this by a scale factor
            newPoint.transform.localPosition =
            new Vector3((points[i].x - points[0].x) / scaleFactor * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, points[i].y, 0);
        }

        RefreshLineRenderer();
    }

    // updates the line renderer component so it connects all the points
    public void RefreshLineRenderer() {
        lineRenderer.Points = new Vector2[points.Count];

        // loop through all the points, and set the position on the line renderer
        for (int i = 0; i < points.Count; i++) {
            lineRenderer.Points[i] = new Vector3(Screen.width / 2, Screen.height / 2) + new Vector3(transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x / -2, transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y / -2) +
            new Vector3((points[i].x - points[0].x) / scaleFactor * transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, points[i].y, 0);
        }
    }
}
