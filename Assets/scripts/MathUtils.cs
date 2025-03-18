using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtils
{
    public static float ToRadians(float a) {
        return a * Mathf.PI / 180;
    }

    public static float ToDegrees(float a) {
        return a * 180 / Mathf.PI;
    }
}
