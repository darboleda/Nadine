using UnityEngine;
using System.Collections;

public class TimedBehavior : MonoBehaviour
{

    private ControlledTime time;

    protected float DeltaTime
    {
        get
        {
            time = time ?? GetComponentInParent<ControlledTime>();
            return (time != null ? time.DeltaTime : Time.deltaTime);
        }
    }

    protected Coroutine WaitForSeconds(float seconds)
    {
        return StartCoroutine(ScaledWaitForSeconds(seconds));
    }

    private IEnumerator ScaledWaitForSeconds(float seconds)
    {
        while (seconds > 0)
        {
            seconds -= DeltaTime;
            yield return null;
        }
    }

    public Coroutine SetClockForDuration(string clockId, float speed, float duration)
    {
        time = time ?? GetComponentInParent<ControlledTime>();
        if (time != null)
        {
            return time.SetClockForDuration(clockId, speed, duration);
        }
        return null;
    }
}
