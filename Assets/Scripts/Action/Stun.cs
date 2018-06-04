using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : BaseAction
{
	private float duration;

	public Stun(AbilityActionData data) : base(data)
	{
		duration = data.FloatFields.Find((a) => a.Name == "duration").Value;
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		var targetProfile = target.GetComponent<Profile>();

		if (targetProfile)
		{
			var obj = PhotonNetwork.Instantiate(effectPath, targetProfile.attachPoint.overhead.position, Quaternion.identity, 0);
			obj.transform.SetParent(targetProfile.attachPoint.overhead);
			effectObjs.Add(obj);

			targetProfile.navAgent.isStopped = true;
			targetProfile.navAgent.ResetPath();
			targetProfile.fsm.SetBool("Moving", false);
			targetProfile.combatController.IsStuned = true;

			yield return new WaitForSeconds(duration);

			targetProfile.combatController.IsStuned = false;
		}

	}

	public override BaseAction Clone()
	{
		return new Stun(data);
	}
}
