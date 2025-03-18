using UnityEngine;
using System.IO;
using System;
using System.Diagnostics;

public class FileUtils : MonoBehaviour
{
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

    public static string RemoveLastDirectory(string directory) {
        for (int i = directory.Length - 1; i >= 0; i--) {
            if (directory[i] == '/') {
                return directory.Substring(0, i+1);
            }
        }
        return "";
    }
    
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

    // sets this application as the default driverstation
    public void SetAsDefaultDriverstation() {
        FileUtils.ModifyTxtWithNoSurprises(
            "C:/Users/Public/Documents/FRC/FRC DS Data Storage.ini", 
            3, 
            FileUtils.RemoveLastDirectory("DashboardCmdLine = " + Application.dataPath) + "Drivetools.exe");
    }

    public void OpenDirectory(string directory) {
        Process.Start(directory);
    }
}
