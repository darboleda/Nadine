using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{

    private static Dictionary<string, CameraController> activeControllers = new Dictionary<string, CameraController>();
    public static CameraController GetCamera(string id)
    {
        if (activeControllers.ContainsKey(id)) return activeControllers[id];
        return null;
    }

    public string CameraId;

    public float LerpAmount = 0.5f;

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
        Target.Activate(GetComponent<Camera>());
    }

    private float currentSpeed;
    private bool shouldSlow;

    public void LateUpdate()
    {
        /*
        Vector3 target = Target.transform.position;
        Vector3 deltaLeft = target - transform.position;
        if (deltaLeft == Vector3.zero)
        {
            return;
        }
        float magnitude = deltaLeft.magnitude;
        if (magnitude < 0.01f)
        {
            transform.position = target;
            currentSpeed = 0;
            shouldSlow = false;
            return;
        }
        Vector3 direction = deltaLeft / magnitude;
        Debug.Log(magnitude);

        currentSpeed += TranslationAcceleration * Time.deltaTime;
        currentSpeed = (currentSpeed > TranslationSpeed ? TranslationSpeed : currentSpeed);

        Vector3 delta = direction * Time.deltaTime * currentSpeed;
        Vector3 position = transform.position;

        Vector3 newPosition = position + delta;
        newPosition = Vector3.Dot(direction, target - newPosition) < 0 ? target : newPosition;
        this.transform.position = newPosition;
        */

        transform.position = (Vector3.Lerp(transform.position, Target.transform.position, LerpAmount));
    }
}
