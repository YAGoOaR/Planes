using UnityEngine;

//My utilities for math
public static class MathUtils
{
    public static void clamp(float variable, float min, float max)
    {
        if (variable > max)
        {
            variable = max;
        }
        if (variable < min)
        {
            variable = min;
        }
    }

    public static float clamped(float variable, float value)
    {
        if (variable > value)
        {
            return value;
        }
        if (variable < -value)
        {
            return -value;
        }
        return variable;
    }

    public static float clamped(float variable, float min, float max)
    {
        if (variable > max)
        {
            return max;
        }
        if (variable < min)
        {
            return min;
        }
        return variable;
    }

    public static float Vector2ToAngle(Vector2 heading)
    {
        Vector3 normalized = heading.normalized;
        return Mathf.Acos(normalized.x) * Mathf.Sign(normalized.y);
    }

    public static float AngleBetweenVectors(Vector2 A, Vector2 B)
    {
        Vector3 normalized = (B - A).normalized;
        return Mathf.Acos(normalized.x) * Mathf.Sign(normalized.y);
    }

    public static Vector2 angleToVector2(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public static float toRadian(float angle)
    {
        return angle / 180 * Mathf.PI;
    }
}
