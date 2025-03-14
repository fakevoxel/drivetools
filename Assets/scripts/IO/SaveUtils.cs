using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveUtils : MonoBehaviour
{
    public static string saveDirectory = Application.persistentDataPath;
    public static string buildVersion = "2025.0.1";

    public static void SavePreferences(PersistentSettings _prefs)
    {
        string savePath = saveDirectory + "/" + "v" + buildVersion + "/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "prefs", FileMode.Create);

        formatter.Serialize(stream, _prefs);
        stream.Close();
    }

    public static PersistentSettings LoadPreferences()
    {
        string loadPath = saveDirectory + "/" + "v" + buildVersion + "/";

        if (File.Exists(loadPath + "prefs"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "prefs", FileMode.Open);

            PersistentSettings robot = formatter.Deserialize(stream) as PersistentSettings;
            stream.Close();
            return robot;
        }
        else
        {
            return null;
        }
    }

    public static void SaveLayouts(DashboardLayout[] layouts)
    {
        string savePath = saveDirectory + "/" + "v" + buildVersion + "/layouts/";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        BinaryFormatter formatter = new BinaryFormatter();
        
        for (int i = 0; i < layouts.Length; i++) {
            FileStream stream = new FileStream(savePath + "l" + i, FileMode.Create);
            formatter.Serialize(stream, layouts[i]);
            stream.Close();
        }
    }

    public static DashboardLayout[] LoadLayouts()
    {
        string loadPath = saveDirectory + "/" + "v" + buildVersion + "/layouts/";

        if (File.Exists(loadPath + "l0"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "l0", FileMode.Open);

            DashboardLayout[] robot = new DashboardLayout[1];

            robot [0] = formatter.Deserialize(stream) as DashboardLayout;
            stream.Close();

            return robot;
        }
        else
        {
            return null;
        }
    }

    public static Texture2D LoadPNG(string filePath) {

        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath)) 	{
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
}
