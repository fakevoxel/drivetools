using UnityEngine;

public class UI_ObjectSetManager : MonoBehaviour
{
    public UI_ObjectSet[] sets;

    public UI_Tabs controllingTabs;

    void Start() {
        Initialize();
    }

    public void Initialize() {
        if (controllingTabs != null) {
            controllingTabs.onChangeTabs.AddListener(EnableSetOfIndex);
        }
    }

    public void EnableSetOfIndex(int index) {
       
        for (int i = 0; i < sets.Length; i++) {
            if (i == index) {
                sets[i].Enable();
            }
            else {
                sets[i].Disable();
            }
        }
    }
}
