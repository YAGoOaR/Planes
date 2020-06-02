﻿using UnityEngine;

public class MathUtils : MonoBehaviour
{
    public static float clamp(float variable, float value)
    {
        if (variable > value) return value;
        if (variable < -value) return -value;
        return variable;
    }
}