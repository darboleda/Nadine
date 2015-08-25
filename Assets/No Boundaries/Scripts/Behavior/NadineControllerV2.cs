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

    public PlayerStateRelay Relay;
    
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
        Physics.Speed = 0;

        float angle;
        if (knockback.Direction.Type == SpecialDirection.DirectionType.Origin)
        {
            Vector3 v = hitProperties.Origin.position - hurtBox.transform.position;
            v.z = 0;
            
            angle = v.GetAngle(Physics.World.East, Physics.World.North);
            angle += 180;
        }
        else
        {
            angle = knockback.Direction.Direction;
        }

        angle = angle.SnapToAngle(45);

        Animator.SetSpecialParameters(angle, knockback.Strength);
        Animator.TriggerDamaged();

        if (Relay.State != null)
        {
            Relay.State.Health.TakeDamage(hitProperties.Damage);
        }

        CameraShake shake = CameraController.GetCamera("Main").Target.GetConstraint<CameraShake>();
        if (shake != null)
        {
            shake.Jolt(knockback.Strength * new Vector3(Mathf.Cos((angle) * Mathf.Deg2Rad), Mathf.Sin((angle) * Mathf.Deg2Rad), 0) * 150, 800, 30);
        }
    }

    public void Roll(Vector2 movement)
    {
        float angle = Animator.FacingAngle + 180;
        if (movement.sqrMagnitude > 0.01f) angle = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        Animator.SetSpecialParameters(angle, 1);
        Animator.StartRoll();
    }
}
