using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour {

    public AudioClip CollectionSound;

    public void DisableCollisions()
    {
        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }
    }

    public void EnableCollisions()
    {
        foreach (Collider collider in GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = true;
        }
        
        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = true;
        }
    }

    public virtual void StartCollectionAnimation(float expectedDisplayTime)
    {

    }

    public virtual void DestroySelf()
    {
        GameObject.Destroy(gameObject);
    }
}
