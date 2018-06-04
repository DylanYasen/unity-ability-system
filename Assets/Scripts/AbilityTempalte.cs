using System;
using System.Collections.Generic;
using UnityEngine;

namespace uAbility
{
    [CreateAssetMenu]
    public class AbilityTempalte : ScriptableObject
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
            Self = 1 << 0,
            Enemy = 1 << 1,
            Ground = 1 << 2,
        }

        [Flags]
        public enum Target
        {
            Caster,
            Target,
            Point,
        }

        public bool CanTargetSelf
        {
            get
            {
                return ((TargetFlags & TargetFlag.Self) != 0);
            }
        }
        public bool CanTargetEnemy
        {
            get
            {
                return ((TargetFlags & TargetFlag.Enemy) != 0);
            }
        }
        public bool CanTargetGround
        {
            get
            {
                return ((TargetFlags & TargetFlag.Ground) != 0);
            }
        }

        public BehaviorFlag BehaviorFlags;
        public TargetFlag TargetFlags;
        public string Name;
        public string Description;
        public float Cooldown;
        public int Price;
        public string Animation = "Spell1";
        public float AnimCastPoint;
        public Sprite Icon;
        public AoeData AoeData;
        public float Duration;

        public List<AbilityEventData> events = new List<AbilityEventData>();
    }

    [Serializable]
    public struct AoeData
    {
        public enum CenterType
        {
            Caster,
            Target,
            Point,
            Projectile,
            Attacker
        }
        public enum ShapeType
        {
            Circle,
            Rect,
            Arc
        }

        public CenterType Center;
        public ShapeType Shape;

        // circle shape
        public int Radius;

        // rect shape
        public int Width;
        public int Distance;

        public int MaxTargets;
    }


}
