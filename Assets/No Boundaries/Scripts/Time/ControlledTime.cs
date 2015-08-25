using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlledTime : MonoBehaviour
{
    public ClockReference Clock;

    [SerializeField]
    private Animator[] animators;

    private Dictionary<Animator, float> animatorMultipliers = new Dictionary<Animator, float>();

    public float Speed
    {
        get
        {
            if (!enabled) return 1;
            Clock clock = Clock.GetClock();
            return (clock != null ? clock.CalculateTotalSpeed() : 1);
        }
    }

    public float DeltaTime
    {
        get
        {
            return Time.deltaTime * Speed;
        }
    }

    public void OnEnable()
    {
        SetAnimatorSpeeds(Speed);
    }

    public void OnDisable()
    {
        SetAnimatorSpeeds(1);
    }

    public void Update()
    {
        SetAnimatorSpeeds(Speed);
    }

    public void SetSpeed(Animator animator, float speed)
    {
        animatorMultipliers[animator] = speed;
        UpdateSpeed(animator, Speed);
    }
    
    private void SetAnimatorSpeeds(float clockMultiplier)
    {
        foreach (Animator animator in animators)
        {
            UpdateSpeed(animator, clockMultiplier);
        }
    }

    private void UpdateSpeed(Animator animator, float clockMultiplier)
    {
        float multiplier;
        multiplier = (animatorMultipliers.TryGetValue(animator, out multiplier) ? multiplier : 1);
        animator.speed = clockMultiplier * multiplier;
    }

    public Coroutine SetClockForDuration(string clockId, float speed, float duration)
    {
        return StartCoroutine(SetClockForDurationHelper(clockId, speed, duration));
    }

    private IEnumerator SetClockForDurationHelper(string clockId, float speed, float duration)
    {
        ClockMaster master = ClockMaster.Find();
        if (master == null) yield break;
        
        Clock clock = master.GetClock(clockId);
        if (clock == null) yield break;

        clock.Speed = speed;
        yield return new WaitForSeconds(duration);
        clock.Speed = 1;
    }
}
