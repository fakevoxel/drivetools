using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// DATA TYPES:
// 0 = string (InputField)
// 1 = float (Slider)
// 2 = bool (checkbox)
public class UI_SettingsPanel : MonoBehaviour
{
    // PREFABS
    public GameObject inputFieldPrefab;
    public GameObject sliderPrefab;
    public GameObject checkBoxPrefab;
    public GameObject textPrefab;
    public GameObject titlePrefab;
    // -- //

    public bool useTitle;
    public bool isBlocked;
    public GameObject blocker;
    public List<SettingsPanel> panels;
    private int activePanelId;

    public Transform panelContainer;

    public float elementSpacing = 90;

    void FixedUpdate() {
        if (blocker != null) {
            blocker.SetActive(isBlocked);
        }
    }

    // ADD A CLEAR FUNCTION

    // Load a given settings panel
    public void InitializePanel(int _panelId) {
        activePanelId = _panelId;
        CanvasUtils.DestroyChildren(panelContainer.gameObject);
        
        if (useTitle) {
            GameObject panelTitle = Instantiate(titlePrefab, Vector3.zero, Quaternion.identity);
            panelTitle.GetComponent<TextMeshProUGUI>().text = panels[_panelId].name;

            panelTitle.transform.SetParent(panelContainer);
            panelTitle.transform.localPosition = Vector3.zero;
        }

        // Spawn all the objects in the panel
        for (int i = 0; i < panels[_panelId].elements.Count; i++) {
            // Create a title for the element
            GameObject elementTitle = Instantiate(textPrefab, Vector3.zero, Quaternion.identity);
            elementTitle.GetComponent<TextMeshProUGUI>().text = panels[_panelId].elements[i].name;

            elementTitle.transform.SetParent(panelContainer);
            elementTitle.transform.localPosition = new Vector3(0, -1, 0) * (((i + 1) * 2 - 1) * elementSpacing + 90);

            // If the data type is 3, then it's just a text object 
            // Otherwise we need to make more stuff
            if (panels[_panelId].elements[i].dataType != 3) {
                GameObject elementPrefab = GetPrefab(panels[_panelId].elements[i].dataType);
            
                // Make the element object
                GameObject newElement = Instantiate(elementPrefab, Vector3.zero, Quaternion.identity);
                // Set the parent, localposition, etc.
                newElement.transform.SetParent(panelContainer);
                newElement.transform.localPosition = new Vector3(0, -1, 0) * ((i + 1) * 2 * elementSpacing + 90);

                panels[_panelId].elements[i].reference = newElement;
            }
            else {
                // In the case that the element is just text
                elementTitle.GetComponent<TextMeshProUGUI>().text = elementTitle.GetComponent<TextMeshProUGUI>().text + ":";
                panels[_panelId].elements[i].reference = elementTitle;
            }
        }
    }
    
    // Make a new settings panel from scratch
    public void CreateNewPanel(SettingsElement[] _elements, string _title) {
        panels.Add(new SettingsPanel(_title));
        // Create a new set of SettingsElement objects and add them to the panels array
        for (int i = 0; i < _elements.Length; i++) {
            // Keep in mind the reference variable will stay null
            panels[panels.Count - 1].elements.Add(_elements[i]);
        }
    }

    // Get the current value (what the player has inputted) from an element
    // the value is a string because strings can be numbers, not the other way around
    public string GetValueFromElement(string _elementName) {
        for (int i = 0; i < panels[activePanelId].elements.Count; i++) {
            if (panels[activePanelId].elements[i].name.Equals(_elementName)) {
                return panels[activePanelId].elements[i].reference.GetComponent<UI_Value>().value;
            }
        }

        return "";
    }

    // Get the prefab associated with a given data type
    GameObject GetPrefab(int _dataType) {
        if (_dataType == 0) { // inputfield
            return inputFieldPrefab;
        }
        else if (_dataType == 1) { // slider
            return sliderPrefab;
        }
        else if (_dataType == 2) { // checkbox
            return checkBoxPrefab;
        }
        // 3 is just text

        Debug.Log("UI_SettingsPanel: DATA TYPE DOES NOT EXIST");
        return null;
    }

    public void PassValueToElement(string _elementName, string _value) {
        for (int i = 0; i < panels[activePanelId].elements.Count; i++) {
            if (panels[activePanelId].elements[i].name.Equals(_elementName)) {
                panels[activePanelId].elements[i].reference.GetComponent<UI_Value>().setValue = _value;
            }
        }
    }
}

[System.Serializable]
public class SettingsElement {
    public string name;
    public int dataType;
    public GameObject reference;

    public SettingsElement(string _name, int _type) {
        name = _name;
        dataType = _type;
    }
}

[System.Serializable]
public class SettingsPanel {
    public string name;
    public List<SettingsElement> elements;

    public SettingsPanel(string _name) {
        elements = new List<SettingsElement>();
        name = _name;
    }
}
