using UnityEngine;
using System.Collections.Generic;

public class ProjectileHurtBox : HurtBox
{
    public Bullet Bullet;
    public float SpeedMultiplier = 1;
    public float AngleSnap;

    public override void TakeHit(HitBox hit)
    {

        Vector3 direction = Bullet.Velocity.normalized;
        
        Knockback knockback = hit.HitProperties.Knockback;
        float speed = Bullet.Velocity.magnitude * knockback.Strength;

        float angle = knockback.Direction;

        if (knockback.IsOriginKnockback)
        {
            Vector3 v = transform.position - hit.HitProperties.Origin.position;
            v.z = 0;

            angle = v.GetAngle(Vector3.right, Vector3.up).SnapToAngle(AngleSnap) * Mathf.Deg2Rad;
        }

        Bullet.Fire(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * speed, "Someone else");
        
        //Bullet.HitBox.HitProperties.TargetTags = new List<string>(HitBoxReplacementTags);
    }
}
