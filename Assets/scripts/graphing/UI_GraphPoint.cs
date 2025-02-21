using UnityEngine;

public class UI_GraphPoint : MonoBehaviour
{
    public UI_LineGraph parentGraph;
    public int pointIndex;
    void Update() {
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
            parentGraph.ShowPointData(pointIndex);
        }
    }
}
