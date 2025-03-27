using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeData_String
{
    // the generic data, like position and size
    public GenericNodeData generic;
    private double value;
    
    public void SetValue(double value) {
        this.value = value;
    }
    public double GetValue() {
        return this.value;
    }
}
