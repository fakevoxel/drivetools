using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenericNodeData
{
    // BOTTOM LEFT CORNER OF NODE
    public double posX, posY;
    // SIZE OF NODE FROM BOTTOM LEFT CORNER
    public double sizeX, sizeY;

    public GenericNodeData(double posX, double posY, double sizeX, double sizeY) {
        this.posX = posX;
        this.posY = posY;

        this.sizeX = sizeX;
        this.sizeY = sizeY;
    }

    public Vector2 GetPosition() {
        return new Vector2((float)posX, (float)posY);
    }
    public Vector2 GetSize() {
        return new Vector2((float)sizeX, (float)sizeY);
    }
}
