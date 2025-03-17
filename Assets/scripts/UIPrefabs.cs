using UnityEngine;

// I kept adding prefab objects to the UIManager, and that script gets cluttered fast
// so, I moved them all here
public class UIPrefabs : MonoBehaviour
{
    private static UIPrefabs _instance;

    public static UIPrefabs Instance {
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

    // element prefabs
    public GameObject previewNodePrefab;
    public GameObject layoutPrefab;

    // UI component prefabs
    // these are used as an easy way to set up dynamic UI
    public GameObject textPrefab;
    public GameObject inputFieldPrefab;
    public GameObject buttonPrefab;
    public GameObject graphPointPrefab;
    public GameObject textButtonPrefab;
    public GameObject tabsPrefab;
    public GameObject objectSetPrefab;
    public GameObject elementListPrefab;
    public GameObject imagePrefab;

    // robot prefab for the field2d node
    public GameObject robotPrefab;
}
