using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public struct SpecialDirection
{
    public enum DirectionType 
    {
        Constant,
        Origin,
    }

    public DirectionType Type;

    [Range(0, 360)]
    public float Direction;
}

[System.Serializable]
public struct Knockback
{
    public SpecialDirection Direction;
    public float Strength;
}

public enum DescriptorType
{
    Target,
    Element,
    Direction
}

[System.Serializable]
public struct DamageDescriptor
{
    public DescriptorType Descriptor;
    public string Tag;

}

public class HitBox : MonoBehaviour
{
    [System.Serializable]
    public struct Properties
    {
        public int Damage;
        public Transform Origin;
        public Knockback Knockback;
        public List<DamageDescriptor> Description;
    }

    public Properties HitProperties;

    public event Action<HitBox, HurtBox> HitResolved;
    
    private List<HurtBox> alreadyHit = new List<HurtBox>();
    private List<HurtBox> unresolvedHits = new List<HurtBox>();

    public virtual void Start() { }

    public void OnTriggerEnter(Collider other)
    {
        QueueHurtBoxes(other.GetComponents<HurtBox>() ?? other.GetComponentsInParent<HurtBox>());
    }

    public void OnTriggerStay(Collider other)
    {
        QueueHurtBoxes(other.GetComponents<HurtBox>() ?? other.GetComponentsInParent<HurtBox>());
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        QueueHurtBoxes(other.GetComponents<HurtBox>() ?? other.GetComponentsInParent<HurtBox>());

    }

    public void OnTriggerStay2D(Collider2D other)
    {
        QueueHurtBoxes(other.GetComponents<HurtBox>() ?? other.GetComponentsInParent<HurtBox>());
    }

    private void QueueHurtBoxes(HurtBox[] hurts)
    {
        foreach (HurtBox hurt in hurts)
        {
            if (alreadyHit.Contains(hurt.Root)) continue;

            unresolvedHits.Add(hurt);
            if (!IsInvoking("ResolveHits"))
            {
                Invoke("ResolveHits", 0);
            }
        }
    }

    public void ClearHits()
    {
        alreadyHit.Clear();
    }

    public void ClearHit(HurtBox hurt)
    {
        alreadyHit.Remove(hurt);
    }

    private void ResolveHits()
    {
        HurtBox hurt = GetBestHurtBox(unresolvedHits);
        unresolvedHits.Clear();

        if (hurt == null) return;
        hurt = hurt.Root;
        alreadyHit.Add(hurt);
        hurt.TakeHit(this);
        if (HitResolved != null)
        {
            HitResolved(this, hurt);
        }
    }

    private HurtBox GetBestHurtBox(List<HurtBox> hurtBoxes)
    {
        HurtBox bestHurt = null;
        int bestPriority = int.MinValue;
        foreach (HurtBox hurt in hurtBoxes)
        {
            if (!hurt.enabled) continue;
            if (!hurt.CanTakeHit(this)) continue;
            
            if (bestHurt == null)
            {
                bestHurt = hurt;
                bestPriority = hurt.Priority;
            }
            else if (bestPriority < hurt.Priority)
            {
                bestHurt = hurt;
                bestPriority = hurt.Priority;
            }
        }

        return bestHurt;
    }
}
