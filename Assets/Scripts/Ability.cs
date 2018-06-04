using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ICloneable
{
    public bool PointTarget
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.POINT_TARGET) != 0);
        }
    }
    public bool UnitTarget
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.UNIT_TARGET) != 0);
        }
    }
    public bool NoTarget
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.NO_TARGET) != 0);
        }
    }
    public bool AOE
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.AOE) != 0);
        }
    }
    public bool Channelled
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.CHANNELLED) != 0);
        }
    }
    public bool Hidden
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.HIDDEN) != 0);
        }
    }

    public bool Immediate
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.IMMEDIATE) != 0);
        }
    }
    public bool DontCancelMovement
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.DONT_CANCEL_MOVEMENT) != 0);
        }
    }

    public bool Directional
    {
        get
        {
            return ((abilityData.BehaviorFlags & DataDrivenAbility.BehaviorFlag.DIRECTIONAL) != 0);
        }
    }

    public bool IsOnCooldown
    {
        get
        {
            return CurrentCooldown > 0;
        }
    }

    public float CurrentCooldown { get; set; }

    public AbilityData abilityData = new AbilityData();

    //public delegate IEnumerator EventCallback(Ability ability);
    public Dictionary<String, List<BaseAction>> EventRegister = new Dictionary<String, List<BaseAction>>();

    const string ON_ABILITY_START = "OnAbilityStart";
    const string ON_SPELL_START = "OnSpellStart";
    const string ON_CHANNEL_INTERRUPTED = "OnChannelInterrupted";
    const string ON_CHANNEL_SUCCEED = "OnChannelSucceed";
    const string ON_PROJECTILE_HIT_UNIT = "OnProjectileHitUnit";
    const string ON_TARGET_DIED = "OnTargetDied";
    const string ON_UPGRADE = "OnUpgrade";

    // Internal Vars
    public bool isRunning { get; private set; }
    public bool isCast { get; private set; }
    public Vector3 targetPoint { get; private set; } // cast to target post
    public GameObject targetUnit { get; private set; }
    public Vector3 castPoint { get; private set; } // owner's position when cast
    public GameObject owner { get; private set; }
    public Profile ownerProfile { get; private set; }
    public List<GameObject> targets { get; private set; }

    public Ability(AbilityData data)
    {
        abilityData = data;
        targets = new List<GameObject>();

        EventRegister.Add(ON_ABILITY_START, new List<BaseAction>());
        EventRegister.Add(ON_SPELL_START, new List<BaseAction>());
        EventRegister.Add(ON_CHANNEL_INTERRUPTED, new List<BaseAction>());
        EventRegister.Add(ON_CHANNEL_SUCCEED, new List<BaseAction>());
        EventRegister.Add("OnChannelFinish", new List<BaseAction>());
        EventRegister.Add(ON_PROJECTILE_HIT_UNIT, new List<BaseAction>());
        EventRegister.Add(ON_TARGET_DIED, new List<BaseAction>());
        EventRegister.Add(ON_UPGRADE, new List<BaseAction>());
    }

    public IEnumerator Excecute(GameObject caster, params object[] target)
    {
        this.owner = caster;
        ownerProfile = caster.GetComponent<Profile>();

        isRunning = true;

        // Extract Target Point/Unit
        castPoint = caster.transform.position;
        targetPoint = (Vector3)target[0];
        targetUnit = (GameObject)target[1];

        if (EventRegister[ON_ABILITY_START] != null)
        {
            Debug.Log("start");
            yield return PerformActions(EventRegister[ON_ABILITY_START]);
        }

        // Turn first
        if (!DontCancelMovement)
        {
            // restrict movement
            ownerProfile.navAgent.isStopped = true;
            ownerProfile.fsm.SetBool("Moving", false);

            yield return ownerProfile.movementController.LookAtPos(targetPoint);
        }
        else
        {
            ownerProfile.StartCoroutine(ownerProfile.movementController.LookAtPos(targetPoint));
        }

        // Play Animation and wait for Cast Point
        // TODO: refactor
        if (!string.IsNullOrEmpty(abilityData.Animation))
        {
            yield return new WaitForEndOfFrame();  // Buffer for sync
            ownerProfile.fsm.SetTrigger(abilityData.Animation);
            yield return new WaitForSeconds(abilityData.AnimCastPoint);
        }

        if (Channelled)
        {
            int channelTimer = 0;
            yield return OnSpellStart();
            while (channelTimer < abilityData.Duration)
            {
                yield return OnChannelSucceed();
                yield return new WaitForSeconds(1);
                channelTimer += 1;
            }
        }
        else
        {
            yield return OnSpellStart();
        }

        yield return End();
    }

    IEnumerator OnSpellStart()
    {
        if (abilityData.Name == "Move")
        {
            if (UnityEngine.Random.Range(0, 5) < 2)
            {
                ownerProfile.photonView.RPC("PlayVoice", GameManager.Instance.PlayerRegistry[owner.name], ResourceManager.MOVE);
            }
        }
        // REFACTOR: either combine move and others 
        else
        {
            if (UnityEngine.Random.Range(0, 5) < 2)
            {
                ownerProfile.photonView.RPC("PlayVoice", GameManager.Instance.PlayerRegistry[owner.name], ResourceManager.SPELL_CAST);
            }
        }

        StartCooldownTimer();
        isCast = true;
        SpellManager.Instance.OnCastAbilitySucceed(abilityData.Name, owner.name);
        if (AOE) { AoeHitCheck(); }
        else { targets.Add(targetUnit); }
        yield return PerformActions(EventRegister[ON_SPELL_START]);
    }

    IEnumerator OnChannelSucceed()
    {
        targets.Clear();
        if (AOE) { AoeHitCheck(); }
        else { targets.Add(targetUnit); }
        yield return PerformActions(EventRegister[ON_CHANNEL_SUCCEED]);
    }

    public IEnumerator End()
    {
        Debug.Log("ability ended");

        targets.Clear();
        isRunning = false;
        isCast = false;

        Debug.Log("ability has " + targets.Count + " targets");

        foreach (var action in EventRegister[ON_SPELL_START])
        {
            yield return action.Reset();
        }

        yield return null;
    }

    public void OnProjectileHit(GameObject target)
    {
        Debug.Log("on hit");

        if (EventRegister[ON_PROJECTILE_HIT_UNIT] != null)
        {
            foreach (var action in EventRegister[ON_PROJECTILE_HIT_UNIT])
            {
                ownerProfile.StartCoroutine(action.Excecute(this, target, target.transform.position));
            }
        }
    }

    IEnumerator PerformActions(List<BaseAction> actions)
    {
        foreach (var action in actions)
        {
            Debug.Log("performing action: " + action.data.Type.ToString());

            if (action.data.Target == DataDrivenAbility.Target.CASTER)
            {
                Debug.Log("casting self");

                yield return action.Excecute(this, owner, targetPoint);
            }
            else if (action.data.Target == DataDrivenAbility.Target.TARGET)
            {
                if (action.data.MultipleTargets)
                {
                    foreach (var target in targets)
                    {
                        yield return action.Excecute(this, target, targetPoint);
                    }
                }
                else
                {
                    yield return action.Excecute(this, targets[0], targetPoint);
                }

            }
            else if (action.data.Target == DataDrivenAbility.Target.POINT)
            {
                if (action.data.MultipleTargets)
                {
                    foreach (var target in targets)
                    {
                        yield return action.Excecute(this, null, targetPoint);
                    }
                }
                else
                {
                    yield return action.Excecute(this, null, targetPoint);
                }
            }
            else
            {
                throw new Exception("non-recognized target type");
            }
        }
    }

    public object Clone()
    {
        // throw new NotImplementedException();
        var ability = new Ability(this.abilityData)
        {
            EventRegister = new Dictionary<string, List<BaseAction>>()
        };

        foreach (var entry in EventRegister)
        {
            ability.EventRegister[entry.Key] = new List<BaseAction>();
            foreach (var action in entry.Value)
            {
                ability.EventRegister[entry.Key].Add(action.Clone());
            }
        }

        return ability;
    }

    private void StartCooldownTimer()
    {
        CurrentCooldown = abilityData.Cooldown;
    }

    private void AoeHitCheck()
    {
        RaycastHit[] hits = null;
        //Physics.SphereCastAll(ParseAoeCenter(), abilityData.AoeData.Radius, Vector3.up, 1, 1 << LayerMask.NameToLayer("Player"));

        Vector3 aoeCenter = ParseAoeCenter();
        if (abilityData.AoeData.Shape == AoeData.ShapeType.CIRCLE)
        {
            hits = Physics.SphereCastAll(aoeCenter, abilityData.AoeData.Radius, Vector3.up, 1 << LayerMask.NameToLayer("Player"));
        }
        else if (abilityData.AoeData.Shape == AoeData.ShapeType.RECT)
        {
            hits = Physics.BoxCastAll(aoeCenter, new Vector3(abilityData.AoeData.Width, abilityData.AoeData.Width, abilityData.AoeData.Width), (targetPoint - castPoint), Quaternion.identity, abilityData.AoeData.Width);
        }

        Debug.Log("hit: " + hits.Length);
        // populate hit objs
        for (int i = 0; i < hits.Length; i++)
        {
            var obj = hits[i].collider.gameObject;
            if (!abilityData.CanTargetSelf && obj.name == owner.name)
            {
                Debug.Log("SELF");
                continue;
            }

            targets.Add(obj);
        }
    }

    private Vector3 ParseAoeCenter()
    {
        if (abilityData.AoeData.Center == AoeData.CenterType.CASTER)
        {
            return owner.transform.position;
        }
        else if (abilityData.AoeData.Center == AoeData.CenterType.POINT)
        {
            return targetPoint;
        }
        else if (abilityData.AoeData.Center == AoeData.CenterType.PROJECTILE)
        {
            // TODO: ref projectile
            return owner.transform.position;
        }
        else // target
        {
            return targets[0].transform.position;
        }
    }
}

