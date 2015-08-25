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

    public void Fire(Vector3 velocity)
    {
        Velocity = velocity;
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
