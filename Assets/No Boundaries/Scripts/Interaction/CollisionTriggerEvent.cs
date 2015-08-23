using UnityEngine;
using System;
using System.Collections.Generic;

public class CollisionTriggerEvent : EventBroadcaster {

    public enum EventTarget
    {
        Listeners,
        Other,
        Both
    }

    public string TriggerEnteredEvent;
    public string TriggerStayedEvent;
    public string TriggerExitedEvent;

    public MonoBehaviour[] Listeners;
    public EventTarget TargetSetting;

    public string[] RequiredCollisionTags;

    private List<Component> Others = new List<Component>();

    public void OnTriggerEnter(Collider collider)
    {
        Component other = RigidbodyOrDefault(collider);
        if (!Others.Contains(other))
        {
            BroadcastEvent(TriggerEnteredEvent, other);
        }

        Others.Add(other);
    }
    
    public void OnTriggerStay(Collider collider)
    {
        Component other = RigidbodyOrDefault(collider);
        BroadcastEvent(TriggerStayedEvent, other);
    }

    public void OnTriggerExit(Collider collider)
    {
        Component other = RigidbodyOrDefault(collider);
        if (Others.Contains(other))
        {
            Others.Remove(other);
            if (!Others.Contains(other))
            {
                BroadcastEvent(TriggerExitedEvent, other);
            }
        }
    }
    
    public void OnTriggerEnter2D(Collider2D collider)
    {
        Component other = RigidbodyOrDefault(collider);
        if (!Others.Contains(other))
        {
            BroadcastEvent(TriggerEnteredEvent, other);
        }
        Others.Add(other);
    }
    
    public void OnTriggerStay2D(Collider2D collider)
    {
        Component other = RigidbodyOrDefault(collider);
        BroadcastEvent(TriggerStayedEvent, other);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        Component other = RigidbodyOrDefault(collider);
        if (Others.Contains(other))
        {
            Others.Remove(other);
            if (!Others.Contains(other))
            {
                BroadcastEvent(TriggerExitedEvent, other);
            }
        }
    }

    private Component RigidbodyOrDefault(Collider2D collider)
    {
        if (collider.attachedRigidbody == null) return collider;
        return collider.attachedRigidbody;
    }

    private Component RigidbodyOrDefault(Collider collider)
    {
        if (collider.attachedRigidbody == null) return collider;
        return collider.attachedRigidbody;
    }

    private void BroadcastEvent(string eventName, Component other)
    {
        if (string.IsNullOrEmpty(eventName)) return;
        CollisionTags tags;
        if ((tags = other.GetComponentInParent<CollisionTags>()) == null || !tags.Matches(RequiredCollisionTags)) return;

        switch (TargetSetting)
        {
        case EventTarget.Listeners:
            foreach (MonoBehaviour behavior in Listeners)
            {
                BroadcastEvent(behavior, eventName);
            }
            break;

        case EventTarget.Other:
            BroadcastEvent(other.gameObject, eventName);
            break;


        case EventTarget.Both:
            foreach (MonoBehaviour behavior in Listeners)
            {
                BroadcastEvent(behavior, eventName);
            }
            BroadcastEvent(other.gameObject, eventName);
            break;
        }
    }
}
