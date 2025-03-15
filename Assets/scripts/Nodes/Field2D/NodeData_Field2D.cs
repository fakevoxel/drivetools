using System.Collections.Generic;

[System.Serializable]
public class NodeData_Field2D
{
    // the generic data, like position and size
    public GenericNodeData generic;
    public List<TrackedRobot> robots;
    public List<FieldMarker> markers;
}
