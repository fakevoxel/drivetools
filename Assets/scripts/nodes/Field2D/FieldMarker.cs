using UnityEngine;

[System.Serializable]
public class FieldMarker
{   public float xPos, yPos;
    public float rot;

    public string xSource, ySource;
    public string rotSource;

    public float size;

    public string imageName;

    public FieldMarker(){
    }

    // getting updated data values from NT, and assigning the variables
    public void UpdateData() {
        xPos = (float)NetworkManager.Instance.FetchNTDouble(xSource);
        yPos = (float)NetworkManager.Instance.FetchNTDouble(ySource);
        rot = (float)NetworkManager.Instance.FetchNTDouble(rotSource);
    }
}
