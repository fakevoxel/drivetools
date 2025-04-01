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

// deals with ui interactions with nodes, like snapping and re-sizing
// this script is present on every node prefab
public class NodeInteractionHandler : MonoBehaviour
{
    // each node type has it's own index, made easier by the NodeType enum
    public int nodeType;

    // the minimum width and height that this node can ocuppy
    // can I just say this was so annoying to get working
    public float minWidth;
    public float minHeight;

    // the transform object of the background image,
    // which is the blank image scaled to the size of the node
    public Transform backgroundTransform;

    // how close do you have to be to another node to snap on to it?
    // and yes, the nodes have a snapping system similar to KSP,
    // just instead of snapping points you have snapping edges
    public double snappingTolerance;

    // how far do you have to move the cursor for the node to un-snap itself
    public double stickyTolerance;

    // very rarely do I write comments the first time, usually its bc something is a pain to remember
    // as explained somewhere below this defines how the user is interacting with a node

    // here's the key:
        // 0 = not touching the node (obviously)
        // 1 = node is held (dragging it around)

        // 2 = left edge
        // 3 = right edge
        // 4 = bottom edge
        // 5 = top edge

        // 6 = top left corner
        // 7 = bottom left corner
        // 8 = top right corner
        // 9 = bottom right corner
    // ------------
    public int holdType;

    // whether the user's cursor is over the node at the moment
    public bool isHoveringOverNode;

    // whether the user can drop the node just by letting go of left mouse,
    // (like when dragging nodes around, value is false),
    // or whether they have to click to place it,
    // (like when first placing a node, value is true)
    public bool isHoldLocked;

    // when the node is being held (dragged around),
    // this defines where the center was compared to the cursor whent the user picked up the node
    private Vector3 offsetFromCursor;
    // I actually don't know how this is different 
    // TODO: know
    private Vector3 offsetFromCursorRaw;


    // bottom left and top right points of the node,
    // these (I think) represent the pixel (screen) positions of the four corners
    public Vector3 leftEdge, rightEdge, bottomEdge, topEdge;

    // there are physical objects for all edges and corners,
    // both so they can be seen by the user (think google slides),
    // and because they're positions are actually referenced in the backend here
    // basically THEY ARE NECESSARY

    // transform objects for all four edges
    public Transform leftEdgeTransform, rightEdgeTransform, bottomEdgeTransform, topEdgeTransform;
    // transform objects for all four corners
    public Transform topLeftTransform, bottomLeftTransform, topRightTransform, bottomRightTransform;

    // the name of the node, obviously
    public string nodeName;

    // a boolean that turns to true if there is no NT data available, 
    // and false if there is
    // it updates IMMEDIATELY
    private bool connectionLoss;
    // same thing as above, but it only becomes true after a solid time of no connection has passed
    // THIS IS THE ONE TO USE if you want to show the user when a node isn't receiving data
    private bool confirmedConnectionLoss;
    // the program time (Time.time) when connection was first lost (first boolean set to true)
    // this resets every time the boolean goes from true to false and back again
    private double timeWhenConnectionLost;
    // how long exactly before connection loss is confirmed and the second boolean is set to true
    private double timeToConfirmedLoss = 1.5;

    // whether or not the node is flagged for recording
    // being 'tracked' means when the users hits record,
    // this node will start logging data
    public bool isNodeTracked;

    void Awake() {
        // instead of defining stuff in the inspector, it's quicker just to leverage the SearchChildrenForName() function
        // after all, there are many instances of this script, one on every node prefab
        backgroundTransform = CanvasUtils.SearchChildrenForName(gameObject, "bg");

        // edges
        leftEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "leftEdge");
        rightEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "rightEdge");
        bottomEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomEdge");
        topEdgeTransform = CanvasUtils.SearchChildrenForName(gameObject, "topEdge");

        // corners
        topLeftTransform = CanvasUtils.SearchChildrenForName(gameObject, "topLeftCorner");
        bottomLeftTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomLeftCorner");
        topRightTransform = CanvasUtils.SearchChildrenForName(gameObject, "topRightCorner");
        bottomRightTransform = CanvasUtils.SearchChildrenForName(gameObject, "bottomRightCorner");
    }

    // this is a public function both so I can insert logic here,
    // and so that I can assign buttons to it in the inspector

    // when a node is 'tracked' it's essentally flagged for data recording
    // if the user hits record, all tracked nodes will start logging data
    public void ToggleTrack() {
        isNodeTracked = !isNodeTracked;
    }

    // this function has no references, 
    // but its still probably being called in the inspector!
    // (BEWARE AND DONT DELETE!!)
    public void SetName(TMP_InputField input) {
        nodeName = input.text;
    }

    // sort of a misc. function for all periodic logic

    // when I started this project I gave the UIManager the only call to Update(), 
    // and everything else was called from the UIManager's Update() function
    // this kept the order of logic consistent and easy to debug

    // since then I have given most nodes calls to the Update() function
    public void Refresh() {
        // the recttransform component of the node, so I don't have to call GetComponent() 500 times
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

        // the holdType variable is just an easy way of knowing how the player is interacting with this node
        // like, are they messing with the top-left corner? the left edge?

        // see the variable definition for the glossary

        // all these if statements define what the hold type is
        // there are no else statements because that made more sense to me and I'm lazy
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

            // make sure the node adopts the proper colors
            AppData.Instance.RefreshObjectColors();
        }

        // when held, drag the node along with the cursor
        if (holdType == 1) {
            HandlePosition();
        }
        else if (holdType != 0) { // not 0, not 1, so re-sizing
            HandleScale();
        }

        // calling the PositionUI() function for all nodes that have it
        // (i may have missed one or two)

        // it's called here, not in the Update() function for the respective nodes,
        // and i have no idea why i decided to do this

        // the order is arbitrary, else statements because you can't be 2 nodes at once
        if (GetComponent<Node_TextDisplay>() != null) {
            GetComponent<Node_TextDisplay>().PositionUI();
        }
        else if (GetComponent<Node_Compass>() != null) {
            GetComponent<Node_Compass>().PositionUI();
        }
        else if (GetComponent<Node_ImageDisplay>() != null) {
            GetComponent<Node_ImageDisplay>().PositionUI();
        }

        // the timer logic for confirming a connection loss (no NT data for a certain amount of time)
        if (connectionLoss && timeWhenConnectionLost < Time.time - timeToConfirmedLoss) {
            // TODO: show to the user when connection has been lost
            confirmedConnectionLoss = true;
        }

        // making sure the parent object is the same as the background object
        // this is done because the background object is the one actually being dealth with in code (for some reason),
        // and I want to keep the parent synced
        GetComponent<RectTransform>().sizeDelta = backgroundTransform.GetComponent<RectTransform>().sizeDelta;
    }

    // called when a hold type of 1 is triggered (the user starts dragging around the node)
    public void Grab() {
        holdType = 1;
        CanvasUtils.SetTransparencyOfChildren(gameObject, 0.5f, false);
    }

    // this variable is explained at its definition, 
    // but basically this is called when first placing a node,
    // so that you have to click to place it instead of just letting go
    public void Lock() {
        isHoldLocked = true;
    }

    // setting the edge object positions to what they should be
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

        backgroundTransform.GetComponent<Image>().color = isNodeTracked ? AppData.Instance.colorPalette[2] : new Color(1,1,1,0.1f);
    }

    // handling the scale of all edges and the background object
    // basically depening on the type of hold the Input.mousePosition variable is used to move around different edges and such
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

        // once everything is scaled make sure the positions are right as well
        // (very important)
        RefreshEdges();
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

    // I have no clue what these next three functions do, 
    // just that they are VERY IMPORTANT AND SHOULD NOT BE MESSED WITH
    // ----------------------
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

        // got rid of this debug cuz it wasn't rlly doing anything and i didnt like it
        //Debug.Log("ERROR: When clamping an attempted edge move, supplied edge index does not exist! Needs to be [0..4]");

        return attemptedPoint;
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

    // ----------------------
}