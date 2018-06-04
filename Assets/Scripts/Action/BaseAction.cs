using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public abstract class BaseAction
{
	public AbilityActionData data;
	protected string effectPath;
	protected string sfxPath;
	protected List<GameObject> effectObjs = new List<GameObject>();
	protected Profile targetProfile;

	public BaseAction(AbilityActionData data)
	{
		this.data = data;

		var effectPathData = this.data.StrFields.Find((a) => a.Name == "effectPath");
		if (effectPathData != null)
		{
			effectPath = effectPathData.Value;
		}

		var sfxPathData = this.data.StrFields.Find((a) => a.Name == "sfxPath");
		if (sfxPathData != null)
		{
			sfxPath = sfxPathData.Value;
		}
	}

	protected bool HasEffect()
	{
		return !string.IsNullOrEmpty(effectPath);
	}

	protected bool HasSfx()
	{
		return !string.IsNullOrEmpty(sfxPath);
	}

	public abstract IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint);

	public virtual IEnumerator Reset()
	{
		for (int i = 0; i < effectObjs.Count; i++)
		{
			PhotonNetwork.Destroy(effectObjs[i]);
		}
		effectObjs.Clear();
		yield return null;
	}

	protected GameObject CreateEffect(Vector3 position)
	{
		var obj = PhotonNetwork.Instantiate(effectPath, position, Quaternion.identity, 0) as GameObject;
		obj.AddComponent<SelfDestoryParticle>();
		return obj;
	}

	public abstract BaseAction Clone();

}