using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Knockback : BaseAction
{
	private int force = 100;
	public string origin = "cast_point";
	public Knockback(AbilityActionData data) : base(data)
	{
		var forceField = this.data.IntFields.Find((a) => a.Name == "force");
		if (forceField != null)
		{
			force = forceField.Value;
		}

		var originField = this.data.StrFields.Find((a) => a.Name == "origin");
		if (originField != null)
		{
			origin = originField.Value;
		}
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		var targetProfile = target.GetComponent<Profile>();

		//Debug.Log("Excecute knockback");

		if (targetProfile)
		{
			Vector3 dir = Vector3.zero;
			if (origin == "cast_point")
			{
				dir = (targetProfile.transform.position - owner.castPoint).normalized;
			}
			else if (origin == "target_point")
			{
				dir = (targetProfile.transform.position - targetPoint).normalized;
			}
			dir.y = 0;

			if (targetProfile.navAgent.isOnNavMesh)
			{
				targetProfile.navAgent.isStopped = true;
				targetProfile.movementController.ApplyForce(dir, force);
			}
		}
		yield return null;
	}

	public override BaseAction Clone()
	{
		return new Knockback(data);
	}
}
