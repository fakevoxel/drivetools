using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImageDisplayLayer
{   
    // the networktables values needed for each image to show
    public List<string> requiredStates;
    // the image associated with each state
    public List<string> images;

    public ImageDisplayLayer() {
        requiredStates = new List<string>();
        images = new List<string>();
    }
}
