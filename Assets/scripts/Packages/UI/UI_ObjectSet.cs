using UnityEngine;

[System.Serializable]
public class UI_ObjectSet
{
    public GameObject[] objects;

    public void Enable() {
        for (int i = 0; i < objects.Length; i++) {
            objects[i].SetActive(true);
        }
    }

    public void Disable() {
        for (int i = 0; i < objects.Length; i++) {
            objects[i].SetActive(false);
        }
    }

    public UI_ObjectSet(GameObject[] objs) {
        this.objects = objs;
    }
}
