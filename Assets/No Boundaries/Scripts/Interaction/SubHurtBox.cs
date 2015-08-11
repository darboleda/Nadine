using UnityEngine;
using System.Collections.Generic;

public class SubHurtBox : HurtBox {

    public HurtBox Parent;

    public bool OverrideRootTags;

    public override HurtBox Root
    {
        get { return (Parent != null ? Parent.Root : this); }
    }

    public override bool CanTakeHit(HitBox hit)
    {
        return MatchesDescription(hit.HitProperties.Description) &&
               !ShouldAvoid(hit.HitProperties);
    }

    public override bool MatchesDescription(List<DamageDescriptor> description)
    {
        if (OverrideRootTags)
        {
            return base.MatchesDescription(description);
        }
        
        return Root.MatchesDescription(description);
    }

    public override bool ShouldAvoid(HitBox.Properties hit)
    {
        return Root.ShouldAvoid(hit);
    }
    
    public override void TakeHit(HitBox hit)
    {
        // this should never be called
        Debug.LogErrorFormat("In CanTakeHit: SubHurtBox {0} without parent detected.", name);
    }
}
