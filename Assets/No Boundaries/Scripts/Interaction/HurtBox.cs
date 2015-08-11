using UnityEngine;
using System.Collections.Generic;

public abstract class HurtBox : MonoBehaviour
{
    public int Priority;

    public virtual HurtBox Root
    {
        get { return this; }
    }

    public List<DamageDescriptor> DamageDescriptors;

    public virtual void Start() { }

    public virtual bool CanTakeHit(HitBox hit)
    {
        return MatchesDescription(hit.HitProperties.Description)
               && !ShouldAvoid(hit.HitProperties);
    }

    public virtual bool MatchesDescription(List<DamageDescriptor> description)
    {
        foreach (DamageDescriptor descriptor in DamageDescriptors)
        {
            if (!description.Contains(descriptor))
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool ShouldAvoid(HitBox.Properties hit)
    {
        return false;
    }
    
    public virtual void TakeHit(HitBox hit) { }
}
