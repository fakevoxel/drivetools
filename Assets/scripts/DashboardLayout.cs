using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class for storing arrangements of nodes
[System.Serializable]
public class DashboardLayout
{
    public string name;

    public NodeData_Double[] doubleNodes;
    public bool isNew;

    public DashboardLayout() {
        doubleNodes = new NodeData_Double[0];
        name = "New Layout";
        isNew = true;
    }

    public DashboardLayout(string name) {
        doubleNodes = new NodeData_Double[0];
        this.name = name;
        isNew = true;
    }
}
