using UnityEngine;
using System.Collections;

public class CollisionCameraConstrainerActivator : MonoBehaviour
{
    public string[] RequiredCollisionTags;
    public CameraConstrainer Constrainer;
    
    public void OnTriggerEnter(Collider collider)
    {
        Component other = RigidbodyOrDefault(collider);
        Activate(other);
    }
    
    public void OnTriggerEnter2D(Collider2D collider)
    {
        Component other = RigidbodyOrDefault(collider);
        Activate(other);
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
    
    public void Activate(Component other)
    {
        CollisionTags tags;
        if ((tags = other.GetComponentInParent<CollisionTags>()) == null || !tags.Matches(RequiredCollisionTags)) return;

        Constrainer.Activate(Camera.main);
    }
}
