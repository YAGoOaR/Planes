using UnityEngine;

//My utilities for math
public static class MathUtils
{
    public static float clampPlusMinus(float variable, float value)
    {
        return Mathf.Clamp(variable, -value, value);
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
