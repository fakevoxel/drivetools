using UnityEngine;

// version of the Color class for use when writing to disk
[System.Serializable]
public class SerializableColor
{   
    // ALL OF THESE ARE 0-1
    public float r;
    public float g;
    public float b;
    public float a;

    // basic r, g, b (alpha assumed as 1)
    public SerializableColor(float r, float g, float b) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 1;
    }

    // RGBA
    public SerializableColor(float r, float g, float b, float a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public SerializableColor(Color color) {
        this.r = color.r;
        this.g = color.g;
        this.b = color.b;
        this.a = color.a;
    }

    // returning the RGBA data as a unity Color class
    public Color GetColor() {
        return new Color(r, g, b, a);
    }

    // method for creating arrays
    public static SerializableColor[] CreateArray(Color[] colors) {
        SerializableColor[] toReturn = new SerializableColor[colors.Length];

        for (int i = 0; i < colors.Length; i++) {
            toReturn[i] = new SerializableColor(colors[i]);
        }

        return toReturn;
    }

    public static Color[] CreateColorArray(SerializableColor[] colors) {
        Color[] toReturn = new Color[colors.Length];

        for (int i = 0; i < colors.Length; i++) {
            toReturn[i] = colors[i].GetColor();
        }

        return toReturn;
    }
}
