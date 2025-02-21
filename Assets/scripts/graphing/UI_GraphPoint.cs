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
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
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
