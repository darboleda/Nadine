using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraConstrainer : MonoBehaviour {
    public List<CameraConstraint> Constraints;

    public void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            this.Activate(camera);
        }
    }

    public void Activate(Camera camera)
    {
        Debug.Log(this);
        enabled = true;

        CameraController controller = camera.GetComponent<CameraController>();

        if (controller.Target != null && controller.Target != this) controller.Target.Deactivate();
        controller.Target = this;

        info = new CameraConstraint.CameraInformation(camera);
        Debug.Log(info);
        Constrain();
    }

    public void Deactivate()
    {
        enabled = false;
        info = default(CameraConstraint.CameraInformation);
    }

    private CameraConstraint.CameraInformation info;
    public void Update()
    {
        Constrain();
    }

    public void Constrain()
    {
        if (!info.HasCamera) return;
        foreach (CameraConstraint constraint in Constraints)
        {
            if (!constraint.enabled) continue;
            constraint.Constrain(info);
        }
    }

    public T GetConstraint<T>() where T : CameraConstraint
    {
        foreach (CameraConstraint constraint in Constraints)
        {
            if (constraint is T) return (T)constraint;
        }
        return default(T);
    }
}
