using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data class for stuff every single node has, like position and scale
// keep in mind this is written to disk, so only primitives allowed!
[System.Serializable]
public class GenericNodeData
{
    // BOTTOM LEFT CORNER OF NODE
    public double posX, posY;
    // SIZE OF NODE FROM BOTTOM LEFT CORNER
    public double sizeX, sizeY;
    
    // whether the node is currently set to be tracked
    public bool isTracked;
    
    // we want everything to be defined, so the constructor takes in all variables
    public GenericNodeData(double posX, double posY, double sizeX, double sizeY, bool isTracked) {
        this.posX = posX;
        this.posY = posY;

        this.sizeX = sizeX;
        this.sizeY = sizeY;

        this.isTracked = isTracked;
    }

    // we store position as floats because this class needs to be written to disk,
    // so the variables CAN ONLY BE PRIMITIVES
    // these functions act as a quick way to get back to the vectors unity knows and loves
    public Vector2 GetPosition() {
        return new Vector2((float)posX, (float)posY);
    }
    public Vector2 GetSize() {
        return new Vector2((float)sizeX, (float)sizeY);
    }

    // why can't vector classes just be primitive? there's probably a good reason
}
