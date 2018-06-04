using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class Blink : BaseAction
{
	public Blink(AbilityActionData data) : base(data)
	{
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		var targetProfile = target.GetComponent<Profile>();

		if (targetProfile)
		{
			targetProfile.navAgent.Warp(targetPoint);
		}

		yield return null;
	}
	public override BaseAction Clone()
	{
		return new Blink(this.data);
	}
}