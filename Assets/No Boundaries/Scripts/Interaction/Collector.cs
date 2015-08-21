using UnityEngine;
using System;

public abstract class Collector<T> : MonoBehaviour where T : Collectible
{

    public void OnTriggerEnter2D(Collider2D other)
    {
        DoCollection(other.GetComponent<T>() ?? other.GetComponentInParent<T>());
    }

    public void OnTriggerEnter(Collider other)
    {
        DoCollection(other.GetComponent<T>() ?? other.GetComponentInParent<T>());
    }

    private void DoCollection(T collectible)
    {
        if (collectible == null) return;
        Collect(collectible);
    }

    protected virtual void Collect(T collectible)
    {
        collectible.DestroySelf();
    }
}
