using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// data class for storing general app settings, like target frame rate, the team number
[System.Serializable]
public class PersistentSettings
{
    public ushort teamNumber; // this data type will work until we get 65000 FRC teams
    public ushort targetFrameRate;

    public PersistentSettings(ushort teamNumber, ushort targetFrameRate) {
        this.teamNumber = teamNumber;
        this.targetFrameRate = targetFrameRate;
    }
}
