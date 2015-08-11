using UnityEngine;
using System.Collections;

public class TrailingFocusPoint : FocusPoint
{

    public Transform OriginPoint;
    public NadineController Controller;
    public float Multiplier;


    private Vector3 OriginPointPreviousPosition;
    public void Update()
    {
        transform.position = OriginPoint.position - Controller.Velocity * Multiplier;
        OriginPointPreviousPosition = OriginPoint.position;
    }
}
