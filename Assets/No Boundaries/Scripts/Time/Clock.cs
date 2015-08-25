using UnityEngine;
using System;


[Serializable]
public struct ClockReference
{
    private Clock clock;

    public string Id;

    public Clock GetClock()
    {
        if (string.IsNullOrEmpty(Id)) return null;
        if (clock != null && clock.Id == Id) return clock;
        ClockMaster master = ClockMaster.FindObjectOfType<ClockMaster>();
        if (master != null)
        {
            clock = master.GetClock(Id);
        }
        return clock;
    }
}

public class Clock : MonoBehaviour
{
    public delegate void SpeedUpdatedHandler(float newTotalSpeed);
    public event SpeedUpdatedHandler SpeedUpdated;

    public string Id;

    public ClockReference Parent;

    public float Speed = 1;
    public float CalculateTotalSpeed()
    {
        Clock parent = Parent.GetClock();
        return (parent != null ? parent.CalculateTotalSpeed() * Speed : Speed);
    }

    public void OnEnable()
    {
        ClockMaster master = ClockMaster.FindObjectOfType<ClockMaster>();
        if (master != null)
        {
            master.RegisterClock(Id, this);
        }
    }

    public void OnDisable()
    {
        ClockMaster master = ClockMaster.FindObjectOfType<ClockMaster>();
        if (master != null)
        {
            master.UnregisterClock(Id, this);
        }
    }
}
