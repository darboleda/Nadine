using UnityEngine;
using System.Collections;

public abstract class NadineController : MonoBehaviour {


	public abstract void BeginAttack();
	public abstract void EndAttack();
	public abstract void TakeHit(HitBox.Properties hitProperties, HurtBox hurtBox);
}
