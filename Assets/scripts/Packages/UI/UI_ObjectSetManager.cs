using UnityEngine;

public class UI_ObjectSetManager : MonoBehaviour
{
    public UI_ObjectSet[] sets;

    public UI_Tabs controllingTabs;

    void Start() {
        if (controllingTabs != null) {
            controllingTabs.onChangeTabs = EnableSetOfIndex;
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
