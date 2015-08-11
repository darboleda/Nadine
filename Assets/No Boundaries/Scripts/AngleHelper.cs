using UnityEngine;
using System.Collections;

public static class AngleHelper
{
    public static float SnapToAngle(this float angle, float snapInterval)
    {
        if (snapInterval <= 0) return angle;
        
        float snappedAngle = (angle + 360) + (snapInterval * 0.5f);
        snappedAngle = (int)(snappedAngle / snapInterval) * snapInterval;

        return snappedAngle;
    }

    public static float GetAngle(this Vector3 direction, Vector3 parallel, Vector3 perpendicular)
    {
        float angle = Vector3.Angle(direction, parallel);
        float sign = Mathf.Sign(Vector3.Dot(direction, perpendicular));
        return angle * sign;
    }
}
