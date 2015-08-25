using UnityEngine;
using System.Collections;

public class EnemyHurt : HurtBox
{
    public OctorockTest Owner;

	public override void TakeHit(HitBox hit)
    {
        Owner.TakeHit(hit.HitProperties);
    }
}
