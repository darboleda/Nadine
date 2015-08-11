using UnityEngine;
using System.Collections.Generic;

public class FocusPointConstraint : CameraConstraint
{

    public override void Constrain(CameraInformation camera)
    {
        Vector3 weightedViewportPosition = camera.WorldToViewportPoint(CalculateFocusPoint(camera));
        Vector3 cameraPosition = camera.ViewportToWorldPoint(new Vector3(weightedViewportPosition.x, weightedViewportPosition.y, 0));

        transform.position = cameraPosition;
    }

    protected Vector3 CalculateFocusPoint(CameraInformation camera)
    {
        List<FocusPoint> focusPoints = FocusPoint.GetFocusPoints();

        float totalWeight = 0;
        Vector3 cumulativePosition = Vector2.zero;

        foreach (FocusPoint focusPoint in focusPoints)
        {
            cumulativePosition += focusPoint.WeightedPosition;
            totalWeight += Mathf.Abs(focusPoint.Weight);
        }
        
        if (totalWeight == 0) return transform.position;

        return cumulativePosition / totalWeight;
    }
}
