using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SnappingMode {
    X = 0,
    Y = 1,
    XY = 2,
}

public enum EdgeType {
    Left,
    Right,
    Bottom,
    Top,
}

// deals with ui interactions with nodes
public class NodeInteractionHandler : MonoBehaviour
{
    public int nodeType;
    public float minWidth;
    public float minHeight;

    public Transform backgroundTransform;
    public double snappingTolerance;
    public double stickyTolerance;

    public int holdType; // not held, center, left edge, right, bottom, top
    public bool isHoveringOverNode;
    public bool isHoldLocked;
    private Vector3 offsetFromCursor;
    private Vector3 offsetFromCursorRaw;

    // bottom left and top right points of the node
    public Vector3 leftEdge, rightEdge, bottomEdge, topEdge;
    public Transform leftEdgeTransform, rightEdgeTransform, bottomEdgeTransform, topEdgeTransform;
    public Transform topLeftTransform, bottomLeftTransform, topRightTransform, bottomRightTransform;

    public string nodeName;
    private bool connectionLoss;
    private bool confirmedConnectionLoss;
    private double timeWhenConnectionLost;
    private double timeToConfirmedLoss = 1.5;

    // whether or not the node is flagged for recording
    public bool isNodeTracked;

    void Awake() {
        backgroundTransform = CanvasUtils.SearchChildrenForName(gameObject, "bg");

        leftEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "leftEdge");
        rightEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "rightEdge");
        bottomEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomEdge");
        topEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "topEdge");

        topLeftTransform = CanvasUtils.SearchChildrenForName(gameObject, "topLeftCorner");
        bottomLeftTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomLeftCorner");
        topRightTransform = CanvasUtils.SearchChildrenForName(gameObject, "topRightCorner");
        bottomRightTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomRightCorner");
    }

    public void ToggleTrack() {
        isNodeTracked = !isNodeTracked;
    }

    public void SetName(string newName) {
        nodeName = newName;
    }
    public void SetName(TMP_InputField input) {
        nodeName = input.text;
    }

    public void Refresh() {
        // the recttransform component of the node
        RectTransform rect = transform.GetChild(0).GetComponent<RectTransform>();
        
        if (holdType == 1 || holdType == 0) {
            RefreshEdges();
        }   

        List<GameObject> objList = CanvasUtils.GetImageObjectsInChildren(UIManager.Instance.nodeOptionsWidget, true);
        objList.Add(gameObject);

        // being "held" means the node is being dragged around
        if (CanvasUtils.IsCursorInteract(gameObject, true)) {
            isHoveringOverNode = true;

            if (Input.GetMouseButtonDown(0)) {
                Grab();
                // doing this here, not inside of grab() because if the node is grabbed the offsets should just be zero
                offsetFromCursor = transform.position - Input.mousePosition;
                offsetFromCursorRaw = transform.position - Input.mousePosition;
            }
            else if (Input.GetMouseButtonDown(1)) {
                // open a little widget with right-click options,
                // usually "edit" "delete" and "track"

                UIManager.Instance.OpenRightClickMenu(this);
            }
        }
        
        if (!CanvasUtils.IsCursorInteractOR(objList, true)) {
            if (isHoveringOverNode) {
                UIManager.Instance.CloseRightClickMenu();
            }

            isHoveringOverNode = false;
        }

        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(leftEdgeTransform.gameObject, true)) {
            holdType = 2;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(rightEdgeTransform.gameObject, true)) {
            holdType = 3;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(bottomEdgeTransform.gameObject, true)) {
            holdType = 4;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(topEdgeTransform.gameObject, true)) {
            holdType = 5;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(topLeftTransform.gameObject, true)) {
            holdType = 6;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(bottomLeftTransform.gameObject, true)) {
            holdType = 7;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(topRightTransform.gameObject, true)) {
            holdType = 8;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }
        if (Input.GetMouseButtonDown(0) && CanvasUtils.IsCursorInteract(bottomRightTransform.gameObject, true)) {
            holdType = 9;
            CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
        }

        if (!Input.GetMouseButton(0) && !isHoldLocked) {
            if (holdType == 1) {
                transform.position = UIManager.Instance.previewNode.transform.position;
                UIManager.Instance.ClearPreviewNode();
            }
            if (holdType != 0) {
                CanvasUtils.SetTransparencyOfChildren(gameObject, 1, false);
            }
            holdType = 0;
        }

        if (holdType != 0 && isHoldLocked && Input.GetMouseButtonDown(0)) {
            isHoldLocked = false;
        }

        // when held, drag the node along with the cursor
        if (holdType == 1) {
            HandlePosition();
        }
        else if (holdType != 0) { // not 0, not 1, so re-sizing
            HandleScale();
        }

        if (GetComponent<Node_Double>() != null) {
            GetComponent<Node_Double>().PositionUI();
        }
        if (GetComponent<Node_String>() != null) {
            GetComponent<Node_String>().PositionUI();
        }
        if (GetComponent<Node_Compass>() != null) {
            GetComponent<Node_Compass>().PositionUI();
        }
        if (GetComponent<Node_ImageDisplay>() != null) {
            GetComponent<Node_ImageDisplay>().PositionUI();
        }

        if (connectionLoss && timeWhenConnectionLost < Time.time - timeToConfirmedLoss) {
            // TODO: show to the user when connection has been lost
            confirmedConnectionLoss = true;
        }

        GetComponent<RectTransform>().sizeDelta = backgroundTransform.GetComponent<RectTransform>().sizeDelta;
    }

    // called when a hold type of 1 is triggered
    public void Grab() {
        holdType = 1;
        CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
    }

    public void Lock() {
        isHoldLocked = true;
    }

    void RefreshEdges() {
        // the recttransform component of the node
        RectTransform rect = transform.GetChild(0).GetComponent<RectTransform>();

        leftEdge = transform.position - Vector3.right * rect.sizeDelta.x / 2;
        rightEdge = transform.position + Vector3.right * rect.sizeDelta.x / 2;
        bottomEdge = transform.position - Vector3.up * rect.sizeDelta.y / 2;
        topEdge = transform.position + Vector3.up * rect.sizeDelta.y / 2;

        RectTransform edgeRect;
        
        leftEdgeTransform.position = leftEdge;
        edgeRect = leftEdgeTransform.GetComponent<RectTransform>();
        edgeRect.sizeDelta = new Vector2(edgeRect.sizeDelta.x, rect.sizeDelta.y);

        rightEdgeTransform.position = rightEdge;
        edgeRect = rightEdgeTransform.GetComponent<RectTransform>();
        edgeRect.sizeDelta = new Vector2(edgeRect.sizeDelta.x, rect.sizeDelta.y);

        bottomEdgeTransform.position = bottomEdge;
        edgeRect = bottomEdgeTransform.GetComponent<RectTransform>();
        edgeRect.sizeDelta = new Vector2(rect.sizeDelta.x, edgeRect.sizeDelta.y);

        topEdgeTransform.position = topEdge;
        edgeRect = topEdgeTransform.GetComponent<RectTransform>();
        edgeRect.sizeDelta = new Vector2(rect.sizeDelta.x, edgeRect.sizeDelta.y);

        // corners
        topLeftTransform.position = transform.position - Vector3.right * rect.sizeDelta.x / 2 + Vector3.up * rect.sizeDelta.y / 2;
        bottomLeftTransform.position = transform.position - Vector3.right * rect.sizeDelta.x / 2 - Vector3.up * rect.sizeDelta.y / 2;
        topRightTransform.position = transform.position + Vector3.right * rect.sizeDelta.x / 2 + Vector3.up * rect.sizeDelta.y / 2;
        bottomRightTransform.position = transform.position + Vector3.right * rect.sizeDelta.x / 2 - Vector3.up * rect.sizeDelta.y / 2;

        backgroundTransform.GetComponent<Image>().color = isNodeTracked ? new Color(0, 0.34f, 0.84f, 1) : new Color(1,1,1,0.1f);
    }

    void HandleScale() {
        if (holdType == 2) { // holding left
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Left, leftEdge, Input.mousePosition, transform.position);
        } else if (holdType == 3) {  // holding right
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Right, rightEdge, Input.mousePosition, transform.position);
        } else if (holdType == 4) {  // holding bottom
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Bottom, bottomEdge, Input.mousePosition, transform.position);
        } else if (holdType == 5) {  // holding top
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Top, topEdge, Input.mousePosition, transform.position);
        } else if (holdType == 6) {  // holding top left corner
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Top, topEdge, Input.mousePosition, transform.position);
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Left, leftEdge, Input.mousePosition, transform.position);
        } else if (holdType == 7) {  // holding bottom left corner
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Bottom, bottomEdge, Input.mousePosition, transform.position);
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Left, leftEdge, Input.mousePosition, transform.position);
        } else if (holdType == 8) {  // holding top right corner
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Top, topEdge, Input.mousePosition, transform.position);
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Right, rightEdge, Input.mousePosition, transform.position);
        } else if (holdType == 9) {  // holding bottom right corner
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Bottom, bottomEdge, Input.mousePosition, transform.position);
            UIManager.Instance.MoveAlignedEdges((int)EdgeType.Right, rightEdge, Input.mousePosition, transform.position);
        }

        RefreshEdges();
    }

    public void MoveEdge(int edgeType, Vector3 newPos) {
        // the recttransform component of the node
        RectTransform rect = transform.GetChild(0).GetComponent<RectTransform>();

        if (edgeType == (int)EdgeType.Left) {
            transform.position = new Vector3((rightEdge + (newPos - rightEdge) / 2).x, transform.position.y, 0);
            rect.sizeDelta = new Vector2(Mathf.Abs((newPos - rightEdge).x), rect.sizeDelta.y);
        } else if (edgeType == (int)EdgeType.Right) {
            transform.position = new Vector3((leftEdge + (newPos - leftEdge) / 2).x, transform.position.y, 0);
            rect.sizeDelta = new Vector2((newPos - leftEdge).x, rect.sizeDelta.y);
        } else if (edgeType == (int)EdgeType.Bottom) {
            transform.position = new Vector3(transform.position.x, (topEdge + (newPos - topEdge) / 2).y, 0);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Abs((newPos - topEdge).y));
        } else if (edgeType == (int)EdgeType.Top) {
            transform.position = new Vector3(transform.position.x, (bottomEdge + (newPos - bottomEdge) / 2).y, 0);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, (newPos - bottomEdge).y);
        }
    }

    public Vector3 ClampAttemptedMove(Vector3 attemptedPoint, int edgeType) {
        if (edgeType == 0) {
            return rightEdge + new Vector3(-minWidth, 0, 0);
        } else if (edgeType == 1) {
            return leftEdge + new Vector3(minWidth, 0, 0);
        } else if (edgeType == 2) {
            return topEdge + new Vector3(0, -minHeight, 0);
        } else if (edgeType == 3) {
            return bottomEdge + new Vector3(0, minHeight, 0);
        }
        //Debug.Log("ERROR: When clamping an attempted edge move, supplied edge index does not exist! Needs to be [0..4]");
        return attemptedPoint;
    }

    // based on position of other nodes, figure out where this node shoul be 
    // handling the snapping system, in other words
    void HandlePosition() {
        // edge points of all OTHER nodes
        Vector3[] leftEdges = CanvasUtils.RemovePoint(UIManager.Instance.GetEdgePoints((int)EdgeType.Left), leftEdge);
        Vector3[] rightEdges = CanvasUtils.RemovePoint(UIManager.Instance.GetEdgePoints((int)EdgeType.Right), rightEdge);
        Vector3[] bottomEdges = CanvasUtils.RemovePoint(UIManager.Instance.GetEdgePoints((int)EdgeType.Bottom), bottomEdge);
        Vector3[] topEdges = CanvasUtils.RemovePoint(UIManager.Instance.GetEdgePoints((int)EdgeType.Top), topEdge);

        // using a really long up vector as a null value since vectors cannot be null
        Vector3 closestToLeft = Vector3.up * 99999;
        Vector3 closestToRight = Vector3.up * 99999;
        Vector3 closestToBottom = Vector3.up * 99999;
        Vector3 closestToTop = Vector3.up * 99999;
        
        // finding closest edges
        for (int i = 0; i < rightEdges.Length; i++) {
            Vector3 comparisonPoint = Snap(rightEdges[i], leftEdge, SnappingMode.Y);

            // if a point closer than the currently assigned one is found, record it
            // because the 'null vector' is so long, a value of null will always be overwritten
            if (Vector3.Distance(comparisonPoint, leftEdge) < Vector3.Distance(closestToLeft, leftEdge)) {
                closestToLeft = comparisonPoint;
            }
        }
        for (int i = 0; i < leftEdges.Length; i++) {
            Vector3 comparisonPoint = Snap(leftEdges[i], rightEdge, SnappingMode.Y);

            if (Vector3.Distance(comparisonPoint, rightEdge) < Vector3.Distance(closestToRight, rightEdge)) {
                closestToRight = comparisonPoint;
            }
        }
        for (int i = 0; i < topEdges.Length; i++) {
            Vector3 comparisonPoint = Snap(topEdges[i], bottomEdge, SnappingMode.X);

            if (Vector3.Distance(comparisonPoint, bottomEdge) < Vector3.Distance(closestToBottom, bottomEdge)) {
                closestToBottom = comparisonPoint;
            }
        }
        for (int i = 0; i < bottomEdges.Length; i++) {
            Vector3 comparisonPoint = Snap(bottomEdges[i], topEdge, SnappingMode.X);

            if (Vector3.Distance(comparisonPoint, topEdge) < Vector3.Distance(closestToTop, topEdge)) {
                closestToTop = comparisonPoint;
            }
        }

        float[] xDistances = new float[] { 
            Vector3.Distance(closestToLeft, leftEdge),
            Vector3.Distance(closestToRight, rightEdge)};
            float[] yDistances = new float[] { 
            Vector3.Distance(closestToBottom, bottomEdge),
            Vector3.Distance(closestToTop, topEdge)};
        int minX = CanvasUtils.GetIndexOfMinimum(xDistances);
        int minY = CanvasUtils.GetIndexOfMinimum(yDistances);

        // for left and right
        Vector3 snappingOffset = Vector3.zero;
        if (minX == 0 && closestToLeft != Vector3.up * 99999 && xDistances[0] < snappingTolerance) { // left
            offsetFromCursor = new Vector3(offsetFromCursorRaw.x + (closestToLeft - leftEdge).x, offsetFromCursor.y, 0);
        } else if (minX == 1 && closestToRight != Vector3.up * 99999 && xDistances[1] < snappingTolerance) { // left
            offsetFromCursor = new Vector3(offsetFromCursorRaw.x + (closestToRight - rightEdge).x, offsetFromCursor.y, 0);
        } else {
            offsetFromCursor = new Vector3(offsetFromCursorRaw.x, offsetFromCursor.y, 0);
        }
        
        // for top and bottom
        if (minY == 0 && closestToBottom != Vector3.up * 99999 && yDistances[0] < snappingTolerance) { // left
            offsetFromCursor = new Vector3(offsetFromCursor.x, offsetFromCursorRaw.y + (closestToBottom - bottomEdge).y, 0);
        } else if (minY == 1 && closestToTop != Vector3.up * 99999 && yDistances[1] < snappingTolerance) { // left
            offsetFromCursor = new Vector3(offsetFromCursor.x, offsetFromCursorRaw.y + (closestToTop - topEdge).y, 0);
        } else {
            offsetFromCursor = new Vector3(offsetFromCursor.x, offsetFromCursorRaw.y, 0);
        } 

        if (Vector3.Distance(transform.position, Input.mousePosition + offsetFromCursorRaw) > stickyTolerance) {
            offsetFromCursor = offsetFromCursorRaw;
        }

        transform.position = Input.mousePosition + offsetFromCursorRaw;
        UIManager.Instance.DrawPreviewNode(Input.mousePosition + offsetFromCursor, backgroundTransform.GetComponent<RectTransform>().sizeDelta);
    }

    Vector3 Snap(Vector3 input, Vector3 target, SnappingMode mode) {
        if ((int)mode == 0) {
            return new Vector3(target.x, input.y, input.z);
        }
        else if ((int)mode == 1) {
            return new Vector3(input.x, target.y, input.z);
        }
        else if ((int)mode == 2) {
            return new Vector3(target.x, target.y, input.z);
        }

        return input;
    }
}