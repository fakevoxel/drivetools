using UnityEngine;

[System.Serializable]
public class NodeData_TextDisplay
{
    // the generic data, like position and size
    public GenericNodeData generic;
    public string sourceString;
    public string titleString;

    // what data type the display node is using, 
    // see TextDisplayMode (Node_TextDisplay.cs) for more detail
    public int mode;
}
