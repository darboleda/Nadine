using UnityEngine;
using System.Collections.Generic;

public class Collector : MonoBehaviour
{
    public StateRelay Relay;
    public CollectionDisplayer Displayer;

    public List<CollectibleInfo> Collectibles;

    public void OnTriggerEnter2D(Collider2D other)
    {
        DoCollection(other.GetComponent<Collectible>() ?? other.GetComponentInParent<Collectible>());
    }

    public void OnTriggerEnter(Collider other)
    {
        DoCollection(other.GetComponent<Collectible>() ?? other.GetComponentInParent<Collectible>());
    }

    private void DoCollection(Collectible collectible)
    {
        if (collectible == null) return;
        if (ShouldCollect(collectible))
        {
            Collect(collectible);
        }
    }

    private bool ShouldCollect(Collectible collectible)
    {
        foreach (CollectibleInfo info in Collectibles)
        {
            if (info.Id == collectible.Info.Id) return true;
        }
        return false;
    }

    protected void Collect(Collectible collectible)
    {
        GameObject.Destroy(collectible.gameObject);
        if (Displayer != null) Displayer.Display(collectible);
        Relay.UpdateState(collectible.Info.Id);
    }
}
