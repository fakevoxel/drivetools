using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// a class for other UI classes to post to, allowing scripts to easily source info
public class UI_Value : MonoBehaviour
{
    public string setValue;
    public string value;

    void Update() {
        if (GetComponent<TMP_InputField>() != null) {
            value = GetComponent<TMP_InputField>().text;
        }
        if (!string.IsNullOrEmpty(setValue)) {
            if (GetComponent<TMP_InputField>() != null) {
                GetComponent<TMP_InputField>().text = setValue;
                setValue = "";
            }
        }
    }
}
