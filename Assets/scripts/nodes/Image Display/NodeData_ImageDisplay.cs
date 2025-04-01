[System.Serializable]
public class NodeData_ImageDisplay
{
    // the generic data, like position and size
    public GenericNodeData generic;
    public ImageDisplayLayer[] layers;
    public string sourceString;
    
    // what data type the display node is using, 
    // see TextDisplayMode (Node_TextDisplay.cs) for more detail
    public int mode;
}