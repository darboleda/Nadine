using UnityEngine;
using System.Collections;

public class EventRebroadcaster : EventBroadcaster {

    public MonoBehaviour[] Listeners;

    public void BroadcastEvent(string eventName)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        foreach (MonoBehaviour behavior in Listeners)
        {
            BroadcastEvent(behavior, eventName);
        }
    }
}
