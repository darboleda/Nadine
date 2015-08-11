using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    public float LerpAmount = 0.5f;

    public CameraConstrainer Target;

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
