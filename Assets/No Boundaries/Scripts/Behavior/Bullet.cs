using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public Vector3 Velocity;

    public HitBox HitBox;

    public void OnEnable()
    {
        HitBox.HitResolved += CheckHitResolution;
    }

    public void OnDisable()
    {
        HitBox.HitResolved -= CheckHitResolution;
    }

    public void CheckHitResolution(HitBox hit, HurtBox hurt)
    {
        GameObject.Destroy(gameObject);
    }

    public void Fire(Vector3 velocity, string newTarget)
    {
        Velocity = velocity;

        List<DamageDescriptor> damageDescriptors = HitBox.HitProperties.Description;
        for (int i = damageDescriptors.Count - 1; i >= 0; i--)
        {
            if (damageDescriptors[i].Descriptor != DescriptorType.Target) continue;
            if (damageDescriptors[i].Tag == newTarget)
            {
                newTarget = null;
                continue;
            }
            damageDescriptors.RemoveAt(i);
        }

        if (newTarget == null) return;
        DamageDescriptor descriptor = new DamageDescriptor();
        descriptor.Descriptor = DescriptorType.Target;
        descriptor.Tag = newTarget;
        damageDescriptors.Add(descriptor);

        SetDirectionTag(velocity.GetAngle(Vector3.right, Vector3.up), damageDescriptors);
    }
    
    private void SetDirectionTag(float angle, List<DamageDescriptor> damageDescriptors)
    {
        angle = (int)angle + 180;
        angle = angle.SnapToAngle(90);
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;

        string newDirection = null;
        switch ((int)angle)
        {
            case 0:
                newDirection = "East";
                break;

            case 90:
                newDirection = "North";
                break;

            case 180:
                newDirection = "West";
                break;

            case 270:
                newDirection = "South";
                break;
        }

        for (int i = damageDescriptors.Count - 1; i >= 0; i--)
        {
            if (damageDescriptors[i].Descriptor != DescriptorType.Direction) continue;
            if (damageDescriptors[i].Tag == newDirection)
            {
                newDirection = null;
                continue;
            }
            damageDescriptors.RemoveAt(i);
        }
        
        if (newDirection == null) return;

        DamageDescriptor direction = new DamageDescriptor();
        direction.Descriptor = DescriptorType.Direction;
        direction.Tag = newDirection;
        damageDescriptors.Add(direction);
    }

    public void FixedUpdate()
    {
        transform.Translate(Velocity * Time.deltaTime);
    }

    public void OnBecameInvisible()
    {
        GameObject.Destroy(gameObject);
    }
}
