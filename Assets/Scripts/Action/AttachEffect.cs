using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachEffect : BaseAction
{
	public AttachEffect(AbilityActionData data) : base(data)
	{
	}

	public override BaseAction Clone()
	{
		return new AttachEffect(this.data);
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		yield return null;
	}
}
