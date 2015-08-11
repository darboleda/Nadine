using UnityEngine;
using System.Collections;

public class ShieldBurstSoundController : MonoBehaviour
{
    public AudioClip StartupSound;
    public AudioClip ReflectSound;

    public HitBox HitBox;

    public void OnEnable()
    {
        GetComponent<AudioSource>().PlayOneShot(StartupSound);
        HitBox.HitResolved += OnHit;
    }

    public void OnDisable()
    {
        HitBox.HitResolved -= OnHit;
    }

    public void OnHit(HitBox hitbox, HurtBox hurtbox)
    {
        GetComponent<AudioSource>().PlayOneShot(ReflectSound);
    }
}
