using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeData_Compass
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
