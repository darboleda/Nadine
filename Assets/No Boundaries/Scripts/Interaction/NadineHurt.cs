using UnityEngine;
using System.Collections;

public class NadineHurt : HurtBox
{
    public NadineController Controller;
    
    public override void TakeHit(HitBox hit)
    {
        Controller.TakeHit(hit.HitProperties, this);
    }
}
