using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// some math functions that are probably native to unity, but I made my own
// this is a utility class, only static variables and functions here
public class MathUtils : MonoBehaviour
{
    public static float ToRadians(float a) {
        return a * Mathf.PI / 180;
    }

    public static float ToDegrees(float a) {
        return a * 180 / Mathf.PI;
    }
}
