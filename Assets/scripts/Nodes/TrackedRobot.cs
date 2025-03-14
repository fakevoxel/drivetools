using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// position, rotation, scale of a robot on the field
// having multiple can be useful when comparing pose estimation to odometry

[System.Serializable]
public class TrackedRobot
{
    // x and y position of the bot, in meters
    public float xPos, yPos;
    // rotation of the robot, in radians
    public float rot;

    // meters
    public float bumperWidth;
    // meters
    public float bumperThickness;

    // networktables locations for all the data
    public string xSource, ySource;
    public string rotSource;

    // used when you want to define the variables later
    public TrackedRobot() {
    }

    // sometimes this is easier to use
    public TrackedRobot(float bumperWidth, float bumperThickness) {
        this.bumperWidth = bumperWidth;
        this.bumperThickness = bumperThickness;
    }

    // creating a class without the source strings (useful if adding them in another part of the logic)
    public TrackedRobot(float xPos, float yPos, float rot, float bumperWidth, float bumperThickness) {
        this.xPos = xPos;
        this.yPos = yPos;
        this.rot = rot;
        this.bumperWidth = bumperWidth;
        this.bumperThickness = bumperThickness;
    }

    // creating the class with the source strings as well
    public TrackedRobot(float xPos, float yPos, float rot, float bumperWidth, float bumperThickness, string xSource, string ySource, string rotSource) {
        this.xPos = xPos;
        this.yPos = yPos;
        this.rot = rot;
        this.bumperWidth = bumperWidth;
        this.bumperThickness = bumperThickness;

        this.xSource = xSource;
        this.ySource = ySource;
        this.rotSource = rotSource;
    }

    // getting updated data values from NT, and assigning the variables
    public void UpdateData() {
        xPos = (float)NetworkManager.Instance.FetchNTDouble(xSource);
        yPos = (float)NetworkManager.Instance.FetchNTDouble(ySource);
        rot = (float)NetworkManager.Instance.FetchNTDouble(rotSource);
    }
}
