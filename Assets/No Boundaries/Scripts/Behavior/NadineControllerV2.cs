using UnityEngine;
using System.Collections;
using Rewired;

public class NadineControllerV2 : NadineController
{

    public WalkerPhysics Physics;
    public NadineAnimator Animator;
    public NadineInput Input;
    public AudioSource Audio;
    public AudioClip DamagedSound;
    
    public override void BeginAttack()
    {
        Physics.Speed = 0;
    }

    public override void EndAttack()
    {
    }

    public override void TakeHit(HitBox.Properties hitProperties, HurtBox hurtBox)
    {
        Knockback knockback = hitProperties.Knockback;
        Audio.PlayOneShot(DamagedSound);

        float angle;
        if (knockback.IsOriginKnockback)
        {
            Vector3 v = hitProperties.Origin.position - hurtBox.transform.position;
            v.z = 0;
            
            angle = v.GetAngle(Physics.World.East, Physics.World.North);
        }
        else
        {
            angle = knockback.Direction;
        }

        angle = angle.SnapToAngle(45);

        Animator.SetSpecialParameters(angle + 180, knockback.Strength);
        Animator.TriggerDamaged();
    }

    public void Roll()
    {
        Animator.SetSpecialParameters(Physics.Direction, 1);
        Animator.StartRoll();
    }
}
