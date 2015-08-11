using UnityEngine;
using System.Collections;

public class RailConstraint : CameraConstraint
{
    public Transform StartPoint;
    public Transform EndPoint;

    public float VerticalPadding;

    public override void Constrain(CameraInformation camera)
    {
        Vector3 a = camera.WorldToViewportPoint(StartPoint.position);
        Vector3 b = camera.WorldToViewportPoint(EndPoint.position);
        Vector3 c = new Vector3(a.x, b.y, a.z);

        Vector3 d = camera.WorldToViewportPoint(transform.position);

        Vector3 cb = b - c;
        Vector3 cd = d - c;

        Vector3 cbNormalized = cb.normalized;
        Vector3 projection = Vector3.Project(cd, cb);

        float sign = Mathf.Sign(Vector3.Dot(cbNormalized, projection));

        Vector3 viewportPos = UnclampedLerp(a, b, sign * Mathf.Sqrt(projection.sqrMagnitude / cb.sqrMagnitude));
        
        viewportPos.z = 0;
        Vector3 railPosition = camera.ViewportToWorldPoint(viewportPos);

        if (VerticalPadding == 0)
        {
            transform.position = railPosition;
            return;
        }
        
        Vector3 railPositionVertical = camera.ViewportToWorldPoint(viewportPos + Vector3.up);

        Vector3 normalizedVertical = (railPositionVertical - railPosition).normalized;

        Vector3 positionDelta = Vector3.Project(transform.position - railPosition, normalizedVertical);
        if (positionDelta.sqrMagnitude > VerticalPadding * VerticalPadding)
        {
            positionDelta = Mathf.Sign(Vector3.Dot(normalizedVertical, positionDelta)) * VerticalPadding * normalizedVertical;
        }

        transform.position = railPosition + positionDelta;


    }

    private Vector3 UnclampedLerp(Vector3 a, Vector3 b, float t)
    {
        return a + (b - a) * t;
    }
    
}
