using UnityEngine;
using System.Collections;

public class PlaySound : BaseAction
{
	public PlaySound(AbilityActionData data) : base(data)
	{
	}

	public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
	{
		// if (HasSfx())
		// {
		//     // AudioManager.Instance.PlaySfx(true, Resources.Load<AudioClip>(sfxPath));
		//     // AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(sfxPath), targetPoint);
		// }

		yield return null;
	}
	public override BaseAction Clone()
	{
		return new PlaySound(data);
	}
}
