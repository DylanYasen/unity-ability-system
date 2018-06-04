using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingProjectile : BaseAction
{
	private int speed;

	public TrackingProjectile(AbilityActionData data) : base(data)
	{
		speed = this.data.IntFields.Find((a) => a.Name == "speed").Value;
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		var targetProfile = target.GetComponent<Profile>();

		if (targetProfile)
		{
			var obj = PhotonNetwork.Instantiate(effectPath, owner.owner.GetComponent<AttachPoint>().weapon.position, Quaternion.identity, 0);
			ProjectileController projectileController = obj.GetComponent<ProjectileController>();

			projectileController.OnHitCallback += owner.OnProjectileHit;

			projectileController.TrackingFire(target.name);
			//owner.owner, targetProfile.gameObject, speed
		}

		yield return null;
	}

	public override BaseAction Clone()
	{
		return new TrackingProjectile(data);
	}
}
