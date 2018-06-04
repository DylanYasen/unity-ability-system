using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearProjectile : BaseAction
{
	private int speed;
	private float radius;

	public LinearProjectile(AbilityActionData data) : base(data)
	{
		speed = this.data.IntFields.Find((a) => a.Name == "speed").Value;
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		Transform weaponAttachPoint = owner.owner.GetComponent<AttachPoint>().weapon;
		Vector3 weaponPos = weaponAttachPoint.position;
		GameObject obj = PhotonNetwork.Instantiate(effectPath, weaponPos, Quaternion.identity, 0);
		ProjectileController projectileController = obj.GetComponent<ProjectileController>();

		yield return new WaitForEndOfFrame();

		// config
		targetPoint.y = weaponPos.y;
		Vector3 dir = targetPoint - weaponPos;

		projectileController.OnHitCallback += owner.OnProjectileHit;
		projectileController.Fire(dir);
		///owner.owner, dir, speed);

		yield return null;
	}

	public override BaseAction Clone()
	{
		return new LinearProjectile(data);
	}

}
