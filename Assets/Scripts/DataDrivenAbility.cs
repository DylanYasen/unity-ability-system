using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.ComponentModel;

[System.Serializable]
public class AbilityActionData
{
	public enum Action
	{
		[Description("Takes SfxPath:Str")]
		PlaySound,
		[Description("Takes effectPath:Str")]
		PlayEffect,
		[Description("Takes effectPath:Str")]
		AttachEffect,
		[Description("Takes amount:Int, effectPath:Str")]
		Damage,
		[Description("Takes amount:Int, effectPath:Str")]
		Heal,
		[Description("Takes force:Int")]
		Knockback,
		[Description("Takes effectPath:Str, speed:Int, origin:Str(cast_point||target_point)")]
		LinearProjectile,
		[Description("Takes effectPath:Str, speed:Int")]
		TrackingProjectile,
		[Description("Takes duration:Float")]
		Stun,
		Move,
		[Description("Takes maxDistance:Int, effectPath:Str")]
		Blink
	}

	public Action Type;
	public DataDrivenAbility.Target Target;
	public bool MultipleTargets;

	// custom properties
	public List<IntField> IntFields = new List<IntField>();
	public List<FloatField> FloatFields = new List<FloatField>();
	public List<Vec3Field> Vec3Fields = new List<Vec3Field>();
	public List<Vec2Field> Vec2Fields = new List<Vec2Field>();
	public List<StrField> StrFields = new List<StrField>();
	public List<BoolField> BoolFields = new List<BoolField>();
	// editor flags
	public bool showInEditor = true;
	public bool tobeRemoved;
	public bool showCustomAttributes;
}

[System.Serializable]
public class AbilityModifierData
{
	public enum Modifier
	{
		MaxHP,
		Speed,
		CooldownMultiplier,
		KnockbackMultiplier,
	}
	public bool showInEditor = true;
	public bool tobeRemoved;
	public Modifier Type;
	public DataDrivenAbility.Target Target;
	public List<IntField> IntFields = new List<IntField>();
	public List<FloatField> FloatFields = new List<FloatField>();
	public List<Vec3Field> Vec3Fields = new List<Vec3Field>();
	public List<Vec2Field> Vec2Fields = new List<Vec2Field>();
	public List<StrField> StrFields = new List<StrField>();
	public List<BoolField> BoolFields = new List<BoolField>();
}

[System.Serializable]
public class AoeData
{
	public enum CenterType
	{
		CASTER,
		TARGET,
		POINT,
		PROJECTILE,
		ATTACKER
	}

	public enum ShapeType
	{
		CIRCLE,
		RECT,
		ARC
	}

	public CenterType Center;
	public ShapeType Shape;

	// circle shape
	public int Radius;

	// rect shape
	public int Width;
	public int Distance;

	public int MaxTargets = -1;
}


[System.Serializable]
public class AbilityEventData
{
	public enum Event
	{
		None,
		OnAbilityStart,
		OnChannelInterrupted,
		OnChannelSucceed,
		OnChannelFinish,
		OnSpellStart,
		OnProjectileHitUnit,
		OnTargetDied,
		OnUpgrade
	}

	[SerializeField]
	public Event Type;
	public bool showInEditor = true;
	public bool showActionsInEditor = true;
	public bool showModifiersInEditor = true;
	public bool tobeRemoved;
	[SerializeField]
	public List<AbilityModifierData> Modifiers = new List<AbilityModifierData>();
	[SerializeField]
	public List<AbilityActionData> Actions = new List<AbilityActionData>();
}

[System.Serializable]
public class IntField
{
	public string Name;
	public int Value;
	public bool tobeRemoved;
}
[System.Serializable]
public class FloatField
{
	public string Name;
	public float Value;
	public bool tobeRemoved;
}
[System.Serializable]
public class Vec3Field
{
	public string Name;
	public Vector3 Value;
	public bool tobeRemoved;
}
[System.Serializable]
public class Vec2Field
{
	public string Name;
	public Vector2 Value;
	public bool tobeRemoved;
}
[System.Serializable]
public class StrField
{
	public string Name;
	public string Value;
	public bool tobeRemoved;
}

[System.Serializable]
public class BoolField
{
	public string Name;
	public bool Value;
	public bool tobeRemoved;
}

[System.Serializable]
public class AbilityData
{
	public bool CanTargetSelf
	{
		get
		{
			return ((TargetFlags & DataDrivenAbility.TargetFlag.SELF) != 0);
		}
	}
	public bool CanTargetEnemy
	{
		get
		{
			return ((TargetFlags & DataDrivenAbility.TargetFlag.ENEMY) != 0);
		}
	}
	public bool CanTargetGround
	{
		get
		{
			return ((TargetFlags & DataDrivenAbility.TargetFlag.GROUND) != 0);
		}
	}

	public DataDrivenAbility.BehaviorFlag BehaviorFlags;
	public DataDrivenAbility.TargetFlag TargetFlags;
	public string Name;
	public string Description;
	public float Cooldown;
	public int Price;
	public string Animation = "Spell1";
	public float AnimCastPoint;
	public Sprite Icon;
	public AoeData AoeData;
	public float Duration;
}

public class DataDrivenAbility : ScriptableObject
{
	[Flags]
	public enum BehaviorFlag
	{
		HIDDEN = 1 << 0, //Can be owned by a unit but can't be cast and won't show up on the HUD.
		PASSIVE = 1 << 1, //Cannot be cast like above but this one shows up on the ability HUD.
		NO_TARGET = 1 << 2, //Doesn't need a target to be cast, ability fires off as soon as the button is pressed.
		UNIT_TARGET = 1 << 3, //Needs a target to be cast on.
		POINT_TARGET = 1 << 4, //Can be cast anywhere the mouse cursor is (if a unit is clicked it will just be cast where the unit was standing).
		AOE = 1 << 5, //Draws a radius where the ability will have effect. Kinda like POINT but with a an area of effect display.
		CHANNELLED = 1 << 6, //Channeled ability. If the user moves or is silenced the ability is interrupted.
		ITEM = 1 << 7, //Ability is tied up to an item.
		DIRECTIONAL = 1 << 8, //Has a direction from the hero, such as miranas arrow or pudge's hook.
		IMMEDIATE = 1 << 9, //Can be used instantly without going into the action queue.
		AUTOCAST = 1 << 10, //Can be cast automatically.
		AURA = 1 << 11, //Ability is an aura.  Not really used other than to tag the ability as such.
		ROOT_DISABLES = 1 << 12, //Cannot be used when rooted
		DONT_CANCEL_MOVEMENT = 1 << 13, //Doesn't cause certain modifiers to end, used for courier and speed burst.
	}

	[Flags]
	public enum TargetFlag
	{
		SELF = 1 << 0,
		ENEMY = 1 << 1,
		GROUND = 1 << 2,
	}
	public enum Target
	{
		CASTER,
		TARGET,
		POINT,
	}
	public AbilityData abilityData = new AbilityData();

	[SerializeField]
	public List<AbilityEventData> events = new List<AbilityEventData>();

	public DataDrivenAbility()
	{
	}

	public static Ability Parse(DataDrivenAbility rawAbility)
	{
		Ability ability = new Ability(rawAbility.abilityData);

		foreach (var abilityEvent in rawAbility.events)
		{
			foreach (var action in abilityEvent.Actions)
			{
				// get the type of action class
				var actionType = System.Type.GetType(action.Type.ToString());
				var actionObj = (BaseAction)System.Activator.CreateInstance(actionType, action);

				// register the corresponding event callback in ability class
				ability.EventRegister[abilityEvent.Type.ToString()].Add(actionObj);
			}
		}

		return ability;
	}
}