using UnityEngine;
using System.Collections.Generic;

public class CollisionTags : MonoBehaviour {

    public List<string> Tags;

    public bool Matches(IList<string> requiredTags)
    {
        foreach (string required in requiredTags)
        {
            if (!Tags.Contains(required)) return false;
        }
        return true;
    }
}
