using System.Collections.Generic;
using UnityEngine;

public class UI_GraphPoint : MonoBehaviour
{
    public UI_LineGraph parentGraph;
    public int pointIndex;
    bool hovering;

    void Awake() {
        hovering = false;
    }

    void Update() {
        if (CanvasUtils.IsCursorInteract(gameObject, true, 
        new List<GameObject>(){parentGraph.pointDataWidget,parentGraph.pointDataWidget.transform.GetChild(0).gameObject,
        parentGraph.pointDataWidget.transform.GetChild(1).gameObject})) {
            parentGraph.ShowPointData(pointIndex);
            hovering = true;
        }
        else {
            if (hovering) {
                parentGraph.HidePointData();
            }
            hovering = false;
        }
    }
}
