using UnityEngine;
using System.Collections;

public class Damage : BaseAction
{
    private int amount;

    public Damage(AbilityActionData data) : base(data)
    {
        amount = this.data.IntFields.Find((a) => a.Name == "amount").Value;
    }

    public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
    {
        targetProfile = target.GetComponent<Profile>();

        if (HasSfx())
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(sfxPath), target.transform.position);
        }

        if (HasEffect())
        {
            if (targetProfile)
            {
                CreateEffect(targetProfile.attachPoint.head.position);
            }
        }

        if (targetProfile)
        {
            targetProfile.combatController.UpdateHp(-amount, owner.owner.name);
        }

        yield return null;
    }

	public override BaseAction Clone()
	{
		return new Damage(this.data);
	}
}
