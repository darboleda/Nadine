using UnityEngine;
using System.Collections;

[System.Serializable]
public struct NadineDamagedConfig
{
	public float KnockbackSpeed;
	public float KnockbackFriction;
	public int KnockbackAngleSnap;
	public AudioClip DamagedSound;
}

public abstract class NadineController : MonoBehaviour {


	public abstract void BeginAttack();
	public abstract void EndAttack();
	public abstract void TakeHit(HitBox.Properties hitProperties, HurtBox hurtBox);
}
