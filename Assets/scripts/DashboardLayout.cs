using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class for storing arrangements of nodes
[System.Serializable]
public class DashboardLayout
{
    public string name;

    // arrays for all the different types of nodes
    public NodeData_Double[] doubleNodes;
    public NodeData_Compass[] compassNodes;
    public NodeData_String[] stringNodes;
    public NodeData_Graph[] graphNodes;
    public NodeData_ImageDisplay[] imageDisplayNodes;
    public NodeData_Field2D[] field2DNodes;
    public NodeData_Dropdown[] dropdownNodes;

    public bool isNew;

    public DashboardLayout() {
        doubleNodes = new NodeData_Double[0];
        compassNodes = new NodeData_Compass[0];
        stringNodes = new NodeData_String[0];
        graphNodes = new NodeData_Graph[0];
        imageDisplayNodes = new NodeData_ImageDisplay[0];
        field2DNodes = new NodeData_Field2D[0];
        dropdownNodes = new NodeData_Dropdown[0];
        
        name = "New Layout";
        isNew = true;
    }

    public DashboardLayout(string name) {
        doubleNodes = new NodeData_Double[0];
        compassNodes = new NodeData_Compass[0];
        stringNodes = new NodeData_String[0];
        graphNodes = new NodeData_Graph[0];
        imageDisplayNodes = new NodeData_ImageDisplay[0];
        field2DNodes = new NodeData_Field2D[0];
        dropdownNodes = new NodeData_Dropdown[0];
    
        this.name = name;
        isNew = true;
    }
}
