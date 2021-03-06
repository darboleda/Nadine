﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public struct AngleSnapPair
{
	public float AngleMax;
	public float AngleToSnap;
}

[System.Serializable]
public struct AngleSnapConfig
{
	public AngleSnapPair[] SnapSettings;
	
	public float Snap(float angle)
	{
		if (SnapSettings.Length == 0) return angle;
		
		while (angle < 0) angle += 360;
		while (angle >= 360) angle -= 360;
		
		System.Array.Sort(SnapSettings, (a, b) => a.AngleMax.CompareTo(b.AngleMax));
		for (int i = 0; i < SnapSettings.Length; i++)
		{
			if (angle < SnapSettings[i].AngleMax) return SnapSettings[i].AngleToSnap;
        }
        return SnapSettings[SnapSettings.Length - 1].AngleToSnap;
    }
}

public class NadineAnimator : TimedBehavior {

	public Animator Animator;
	public AngleSnapConfig Snap;
	
	public string HorizontalDirectionParamName;
	public string VerticalDirectionParamName;
	public string MovingParamName;
	public string DamagedTriggerName;
    public string RollTriggerName;
	
	public string ShieldAttackTriggerName;
	public string ShieldTypeIdParamName;

    public string SpecialDirectionParameterName;
    public string SpecialSpeedParameterName;

    public float FacingAngle { get; private set; }
	
	public void SetDirection(float degrees)
	{
        if (DeltaTime == 0) return;

        FacingAngle = Snap.Snap(degrees);
		float radians = FacingAngle * Mathf.Deg2Rad;

		float val = Mathf.Cos(radians);
		if (Mathf.Abs(val) < 0.01) val = 0;
		Animator.SetFloat(HorizontalDirectionParamName, val);
		val = Mathf.Sin(radians);
		if (Mathf.Abs(val) < 0.01) val = 0;
		Animator.SetFloat(VerticalDirectionParamName, val);
	}
	
	public void SetSpeed(float speed)
	{
        if (DeltaTime == 0) return;

		Animator.SetBool(MovingParamName, speed > 0);
	}
	
	public void TriggerDamaged()
	{
		Animator.SetTrigger(DamagedTriggerName);
	}
	
	public void StartShieldAttack(int shieldId)
	{
		Animator.SetTrigger(ShieldAttackTriggerName);
		Animator.SetInteger(ShieldTypeIdParamName, shieldId);
	}

    public void StartRoll()
    {
        Animator.SetTrigger(RollTriggerName);
    }

    public void SetSpecialParameters(float direction, float speed)
    {
        Animator.SetFloat(SpecialDirectionParameterName, direction);
        Animator.SetFloat(SpecialSpeedParameterName, speed);
    }
}
