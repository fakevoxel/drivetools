using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// a list of input fields that can be added and removed from
public class UI_InputList : MonoBehaviour
{
    List<string> values;

    public Transform inputContainer;

    public GameObject inputPrefab;
    public UI_Button addButton;

    // called when an element is pressed, if its a button
    public UnityEvent onPressElement;
    // if its an input field
    public UnityEvent<string> onEditElement;
    // called when a new element is added
    public UnityEvent onAddNewElement;

    public float spacing;
    public bool isHorizontal;
    public int limit;

    public int pressedElementIndex;

    void Start() {
        values = new List<string>();

        if (addButton != null) {
            addButton.onPress.AddListener(
                AddNewElement
            );
        }
    }

    public void RefreshValues() {
        values.Clear();

        // loop through all the input fields
        for (int i = 0; i < inputContainer.childCount; i++) {
            // grab the text inputted by the user
            values.Add(inputContainer.GetChild(i).GetComponent<TMP_InputField>().text);
        }
    }

    // return an array containing all the inputted values as they are
    public string[] GetValues() {
        RefreshValues();
        return values.ToArray();
    }

    // adds a new input field to the list
    public void AddNewElement() {
        GameObject newElement = Instantiate(inputPrefab, Vector3.zero, Quaternion.identity);

        if (newElement.GetComponent<UI_Button>() != null) {
            newElement.GetComponent<UI_Button>().onPress.AddListener(
                () => {
                    pressedElementIndex = newElement.transform.GetSiblingIndex();
                    onPressElement.Invoke();
                }
            );
        }
        else if (newElement.GetComponent<TMP_InputField>() != null) {
            newElement.GetComponent<TMP_InputField>().onEndEdit.AddListener(
                (value) => {
                    pressedElementIndex = newElement.transform.GetSiblingIndex();
                    onEditElement.Invoke(value);
                }
            );
        }

        newElement.transform.SetParent(inputContainer);

        int index = inputContainer.childCount - 1;
        int x = index % limit;
        int y = Mathf.FloorToInt((float)index / limit);

        Vector3 pos;
        Vector3 buttonPos;
        if (!isHorizontal) {
            pos = new Vector3(y * spacing, -x * spacing, 0);
            buttonPos = new Vector3(y * spacing, -(x+1) * spacing, 0);
        }
        else {
            pos = new Vector3(x * spacing, -y * spacing, 0);
            buttonPos = new Vector3((x+1) * spacing, -y * spacing, 0);
        }

        newElement.transform.localPosition = pos;
        addButton.transform.localPosition = buttonPos;

        onAddNewElement.Invoke();
    }
}
