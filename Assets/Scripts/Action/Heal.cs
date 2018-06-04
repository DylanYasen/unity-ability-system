using System;
using System.Collections;
using UnityEngine;

public class Heal : BaseAction
{
	private int amount;

	public Heal(AbilityActionData data) : base(data)
	{
		amount = this.data.IntFields.Find((a) => a.Name == "amount").Value;
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		var targetProfile = target.GetComponent<Profile>();

		// create effect
		if (targetProfile)
		{
			targetProfile.combatController.UpdateHp(amount, owner.owner.name);
		}

		yield return null;
	}

	public override BaseAction Clone()
	{
		return new Heal(data);
	}


}

