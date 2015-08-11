using UnityEngine;
using System.Collections;

public class HitBoxReset : MonoBehaviour
{
    public HitBox HitBox;
    public float ResetTime;

    public void OnEnable()
    {
        HitBox.HitResolved += OnHitResolved;
    }

    public void OnDisable()
    {
        HitBox.HitResolved -= OnHitResolved;
    }

    private void OnHitResolved(HitBox hit, HurtBox hurt)
    {
        StartCoroutine(ClearHitAfterDuration(hit, hurt, ResetTime));
    }

    private IEnumerator ClearHitAfterDuration(HitBox hit, HurtBox hurt, float time)
    {
        yield return new WaitForSeconds(time);
        hit.ClearHit(hurt);
    }
}
