using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;

// utility class, contains functions for modifying files/file paths
public class FileUtils : MonoBehaviour
{
    // replaces one line of a text file with a string
    //returns null if goes right, return error message if goes wrong
    public static string ModifyTxtWithNoSurprises(string file, int lineIndex, string newText)
    {
        try
        {
            var lines = File.ReadAllLines(file);
            lines[lineIndex] = newText;
            File.WriteAllLines(file, lines);
        }
        catch(Exception e)
        {
            return e.Message;
        }
        return null;
    }

    // takes out the last directory of a file path, like going out of a folder
    public static string RemoveLastDirectory(string directory) {
        for (int i = directory.Length - 1; i >= 0; i--) {
            if (directory[i] == '/') {
                return directory.Substring(0, i+1);
            }
        }
        return "";
    }

    // gets the file name, given the file path (removes the extension)
    // c:/users/me/desktop/map.png --> map
    // c:/users/me/documents/file --> file
    public static string GetFileName(string path) {
        int startIndex = 0;
        for (int i = path.Length - 1; i >= 0; i--) {
            if (path[i] == '/' || path[i] == '\\') {
                startIndex = i + 1;
                break;
            }
        }

        int endIndex = path.Length - 1;
        if (path.LastIndexOf(".") > startIndex) {
            UnityEngine.Debug.Log(path);
            
            endIndex = 0;
            for (int i = path.Length - 1; i >= 0; i--) {
                if (path[i] == '.') {
                    endIndex = i;
                    break;
                }
            }
        }

        return path.Substring(startIndex, endIndex - startIndex);
    }

    // sets this application as the default driverstation, by modifying the FRC Driverstation settings file
    public void SetAsDefaultDriverstation() {
        FileUtils.ModifyTxtWithNoSurprises(
            "C:/Users/Public/Documents/FRC/FRC DS Data Storage.ini", 
            3, 
            FileUtils.RemoveLastDirectory("DashboardCmdLine = " + Application.dataPath) + "Drivetools.exe");
    }

    // opens a given directory in the windows file explorer
    public void OpenDirectory(string directory) {
        Process.Start(directory);
    }
}
