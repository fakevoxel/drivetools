using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A support class with functions for Nodes (positioning, snapping etc.)
// This class can be referenced by any other script in the project
public class NodeUtils : MonoBehaviour
{
    public static EdgeType GetOppositeEdgeType(int edgeType) {
        if (edgeType == (int)EdgeType.Left) {
            return EdgeType.Right;
        } else if (edgeType == (int)EdgeType.Right) {
            return EdgeType.Left;
        } else if (edgeType == (int)EdgeType.Bottom) {
            return EdgeType.Top;
        } else if (edgeType == (int)EdgeType.Top) {
            return EdgeType.Bottom;
        }

        Debug.Log("ERROR: When finding opposite edge type, supplied index does not exist! Needs to be [0..4]");
        return EdgeType.Left;
    }
}
