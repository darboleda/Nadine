using UnityEngine;
using System.Collections.Generic;

public static class EventListenerExtensions
{
    public static void Event(this MonoBehaviour target, string eventName, params object[] args)
    {
        IEventListener listener;
        if ((listener = target as IEventListener) != null)
        {
            listener.Event(eventName, args);
        }
    }
}

public interface IEventListener
{
    void Event(string eventName, params object[] args);
}

public class EventBroadcaster : MonoBehaviour {

    public void BroadcastEvent(GameObject target, string eventName, params object[] args)
    {
        foreach (MonoBehaviour behavior in target.GetComponents<MonoBehaviour>())
        {
            BroadcastEvent(behavior, eventName, args);
        }
    }

    protected void BroadcastEvent(MonoBehaviour target, string eventName, params object[] args)
    {
        target.Event(eventName, args);
    }
}
