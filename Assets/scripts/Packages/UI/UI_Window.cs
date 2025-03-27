using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Window : MonoBehaviour
{
    public Transform toolbarTransform;

    private bool isDragging;
    private Vector2 offsetFromMouse;
    public float timeWhenClosed;

    void Update() {
        if (isDragging) {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) + offsetFromMouse;
        }

        if (CanvasUtils.IsCursorInteract(toolbarTransform.gameObject, true) && Input.GetMouseButtonDown(0)) {
            offsetFromMouse = transform.position - Input.mousePosition;
            isDragging = true;
        }

        if (!Input.GetMouseButton(0) || !CanvasUtils.IsCursorInteract(toolbarTransform.gameObject, true)) {
            isDragging = false;
        }
    }

    public void Close() {
        timeWhenClosed = Time.time;
    }
}
