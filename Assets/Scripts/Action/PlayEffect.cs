using UnityEngine;
using System.Collections;

public class PlayEffect : BaseAction
{
    public string origin = "cast_point";

    private float timer = 0;

    public PlayEffect(AbilityActionData data) : base(data)
    {
        var originField = this.data.StrFields.Find((a) => a.Name == "origin");
        if (originField != null)
        {
            origin = originField.Value;
        }
    }

    public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
    {
        if (HasSfx())
        {
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(sfxPath), targetPoint);
        }

        Vector3 point = targetPoint;
        if (origin == "cast_point")
        {
            point = owner.castPoint;
        }
        else if (origin == "target_point")
        {
            point = targetPoint;
        }
        // z-fighting or y-fighting in this case
        point.y += 1;

        // if (owner.Directional)
        // {

        // }
        GameObject effect = PhotonNetwork.Instantiate(effectPath, point, Quaternion.LookRotation(targetPoint - owner.castPoint), 0) as GameObject;
        if (owner.Directional)
        {
            effect.transform.LookAt(owner.targetPoint);
        }

        if (effect.GetComponent<ParticleSystem>() != null)
        {
            effect.AddComponent<SelfDestoryParticle>();
        }
        else if (effect.GetComponent<Animator>() != null)
        {
            effect.AddComponent<SelfDestoryAfterAnim>();
        }

        yield return null;
    }

    public override BaseAction Clone()
    {
        return new PlayEffect(data);
    }
}
