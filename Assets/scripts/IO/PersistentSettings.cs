using UnityEngine;

// data class for storing general app settings, like target frame rate, the team number
[System.Serializable]
public class PersistentSettings
{   
    // all directories for currently loaded image assets
    public string[] imageAssetDirectories;

    // basic settings
    public ushort teamNumber; // this data type will work until we get 65000 FRC teams
    public ushort targetFrameRate;

    // the chosen UI color palette
    // not the unity Color class because its being written to disk
    public SerializableColor[] colorPalette;

    public PersistentSettings(ushort teamNumber, ushort targetFrameRate) {
        this.teamNumber = teamNumber;
        this.targetFrameRate = targetFrameRate;
        imageAssetDirectories = new string[0];
        colorPalette = new SerializableColor[4]; // initializing as length 4 because rn that's how big the palette is (see AppData for info)
    }

    public PersistentSettings(ushort teamNumber, ushort targetFrameRate, string[] imageAssetDirectories) {
        this.teamNumber = teamNumber;
        this.targetFrameRate = targetFrameRate;
        this.imageAssetDirectories = imageAssetDirectories;
        colorPalette = new SerializableColor[4];
    }

    // explicitly defining the color array
    public PersistentSettings(ushort teamNumber, ushort targetFrameRate, string[] imageAssetDirectories, Color[] colorPalette) {
        this.teamNumber = teamNumber;
        this.targetFrameRate = targetFrameRate;
        this.imageAssetDirectories = imageAssetDirectories;

        // handy method for converting the class
        this.colorPalette = SerializableColor.CreateArray(colorPalette);
    }
}
