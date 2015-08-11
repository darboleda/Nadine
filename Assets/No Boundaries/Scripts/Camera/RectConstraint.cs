using UnityEngine;

public class RectConstraint : CameraConstraint
{
    public Transform CornerA;
    public Transform CornerB;

    public bool ConstrainView;


    public override void Constrain(CameraInformation camera)
    {
        Vector3 a = camera.WorldToViewportPoint(CornerA.position);
        Vector3 b = camera.WorldToViewportPoint(CornerB.position);

        Vector3 d = camera.WorldToViewportPoint(transform.position);

        float viewConstraint = (ConstrainView ? 0.5f : 0);

        float min = Mathf.Min(a.x, b.x) + viewConstraint;
        float max = Mathf.Max(a.x, b.x) - viewConstraint;

        float x = (max - min >= 0 ? Mathf.Clamp(d.x, min, max) : (min + max) * 0.5f);
        
        min = Mathf.Min(a.y, b.y) + viewConstraint;
        max = Mathf.Max(a.y, b.y) - viewConstraint;

        float y = (max - min >= 0 ? Mathf.Clamp(d.y, min, max) : (min + max) * 0.5f);

        transform.position = camera.ViewportToWorldPoint(new Vector3(x, y, 0));
    }
}
