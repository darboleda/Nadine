using UnityEngine;
using System.Collections.Generic;

public class FocusPoint : MonoBehaviour
{

    private static List<FocusPoint> focusPoints = new List<FocusPoint>();
    protected static void RegisterFocusPoint(FocusPoint point)
    {
        if (!focusPoints.Contains(point))
        {
            focusPoints.Add(point);
        }
    }

    protected static void UnregisterFocusPoint(FocusPoint point)
    {
        focusPoints.Remove(point);
    }
    
    public static List<FocusPoint> GetFocusPoints()
    {
        return focusPoints;
    }
    
    
    public float Weight;

    public Vector3 Position { get { return transform.position; } }
    public Vector3 WeightedPosition { get { return transform.position * Weight; } }

    public void OnEnable()
    {
        RegisterFocusPoint(this);
    }

    public void OnDisable()
    {
        UnregisterFocusPoint(this);
    }
}
