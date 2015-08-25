using UnityEngine;
using System.Collections.Generic;

public class ClockMaster : MonoBehaviour {

    public static ClockMaster Find()
    {
        return FindObjectOfType<ClockMaster>();
    }

    private Dictionary<string, Clock> Clocks = new Dictionary<string, Clock>();

    public Clock GetClock(string id)
    {
        Clock clock;
        return (Clocks.TryGetValue(id, out clock) ? clock : null);
    }

    public void RegisterClock(string id, Clock clock)
    {
        Clocks[id] = clock;
    }

    public void UnregisterClock(string id, Clock clock)
    {
        Clock stored;
        if (Clocks.TryGetValue(id, out stored) && stored == clock)
        {
            Clocks.Remove(id);
        }
    }
}
