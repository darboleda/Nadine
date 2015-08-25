using UnityEngine;
using System.Collections.Generic;

public class ShieldHurt : HurtBox
{
    public NadineController Controller;
    public AudioClip BlockSound;
    public float ShieldDirection;

    public override bool MatchesDescription(List<DamageDescriptor> descriptors)
    {
        SetDirectionTag(ShieldDirection);
        return base.MatchesDescription(descriptors);
    }

    private void SetDirectionTag(float angle)
    {
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

        for (int i = DamageDescriptors.Count - 1; i >= 0; i--)
        {
            if (DamageDescriptors[i].Descriptor != DescriptorType.Direction) continue;
            if (DamageDescriptors[i].Tag == newDirection)
            {
                newDirection = null;
                continue;
            }
            DamageDescriptors.RemoveAt(i);
        }
        
        if (newDirection == null) return;

        DamageDescriptor direction = new DamageDescriptor();
        direction.Descriptor = DescriptorType.Direction;
        direction.Tag = newDirection;
        DamageDescriptors.Add(direction);
    }

    public override void TakeHit(HitBox hit)
    {
        GetComponent<AudioSource>().PlayOneShot(BlockSound);
        SetClockForDuration("Game", 0.05f, 0.15f);
    }
}
