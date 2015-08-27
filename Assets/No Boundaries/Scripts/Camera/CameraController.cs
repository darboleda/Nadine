using UnityEngine;
using System.Collections.Generic;

public class CameraController : TimedBehavior
{

    private static Dictionary<string, CameraController> activeControllers = new Dictionary<string, CameraController>();
    public static CameraController GetCamera(string id)
    {
        if (activeControllers.ContainsKey(id)) return activeControllers[id];
        return null;
    }

    public string CameraId;

    public float MaxSpeed = 20f;

    public CameraConstrainer Target;

    public void OnEnable()
    {
        activeControllers.Add(CameraId, this);
    }

    public void OnDisable()
    {
        CameraController stored;
        if (activeControllers.TryGetValue(CameraId, out stored) && stored == this)
        {
            activeControllers.Remove(CameraId);
        }
    }

    public void Start()
    {
        if (Target == null) return;

        Target.Activate(GetComponent<Camera>());
    }

    private Vector3 velocity;
    public void LateUpdate()
    {
        if (Target == null) return;

        Vector3 position = transform.position;
        Vector3 target = Target.transform.position;

        velocity = (target - position).normalized * MaxSpeed;

        Vector3 newPosition = position + (velocity * DeltaTime);
        if (Vector3.Dot(target - position, target - newPosition) < 0)
        {
            newPosition = target;
        }

        transform.position = newPosition;
    }

    public void Jump(float lerpAmount)
    {
        if (Target == null) return;

        transform.position = Vector3.Lerp(transform.position, Target.transform.position, lerpAmount);
    }
}
