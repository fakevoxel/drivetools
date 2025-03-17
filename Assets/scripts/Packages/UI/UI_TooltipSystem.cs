using UnityEngine;

// a central script for managing tooltips for UI components
// the idea is the dev defines the tooltip message over on the components side, 
// and thanks to the singleton implementation they can talk to this script without needing a reference
public class UI_TooltipSystem : MonoBehaviour
{
    private static UI_TooltipSystem _instance;

    public static UI_TooltipSystem Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("Duplicate UIManager instance in scene!");
                Destroy(value);
            }
        }
    }

    private void Awake() {
        Instance = this;
    }
}
