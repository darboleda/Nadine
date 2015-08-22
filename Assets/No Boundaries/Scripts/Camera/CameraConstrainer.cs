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
        this.Deactivate();
        this.StartCoroutine(Constrain(camera));
    }

    public void Deactivate()
    {
        this.StopAllCoroutines();
    }

    public IEnumerator Constrain(Camera camera)
    {
        CameraConstraint.CameraInformation info = new CameraConstraint.CameraInformation(camera);
        while (camera != null)
        {
            foreach (CameraConstraint constraint in Constraints)
            {
                if (!constraint.enabled) continue;
                constraint.Constrain(info);
            }
            yield return null;
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
