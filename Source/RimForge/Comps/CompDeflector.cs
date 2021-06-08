using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimForge.Comps
{
    // Heavily inspired by Jec's Tools deflection system.
    // So why not use the Jec's version?
    // Because the code is a mess and it doesn't work as advertised (always reflects bullets, instead of deflecting).
    // It has half-implemented systems (accuracy roll) and several other quirks that I didn't feel happy
    // adding to my weapons.
    // This is a simplified version of that system, without the mess and spaghetti.
    public class CompDeflector : ThingComp
    {
        public Pawn Wielder => (parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;
        public CompProperties_Deflector Props;
        public bool IsOnCooldown => cooldownTicksRemaining > 0;
        public bool NeedsCustomTick => parent == null || parent.def.tickerType != TickerType.Normal;
        public float DrawAngleOffset;
        public bool Enabled = true;

        private int cooldownTicksRemaining;
        private float angleOffsetVel;

        public override void Initialize(CompProperties props)
        {
            Props = (CompProperties_Deflector) props;
            if (NeedsCustomTick)
                CompDeflectorTickerComp.Current.Add(this);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (NeedsCustomTick)
                CompDeflectorTickerComp.Current.Remove(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref Enabled, "rf_deflectEnabled", true);
        }

        public Gizmo GetToggleCommand()
        {
            return new Command_Toggle()
            {
                defaultLabel = "RF.Comps.DeflectLabel".Translate(),
                defaultDesc = "RF.Comps.DeflectDesc".Translate((100f * Props.deflectChance).ToString("F0")),
                alsoClickIfOtherInGroupClicked = true,
                activateIfAmbiguous = true,
                isActive = () => Enabled,
                toggleAction = () =>
                {
                    Enabled = !Enabled;
                },
                icon = Content.DeflectIcon
            };
        }

        public override void CompTick()
        {
            base.CompTick();

            // Only called on things that tick every frame.
            TickFinal();
        }

        public void TickCustom()
        {
            // Only called on things that don't tick every frame (by default).
            // This TickCustom is called from CompDeflectorTickerComp.
            TickFinal();
        }

        protected virtual void TickFinal()
        {
            if (cooldownTicksRemaining > 0)
                cooldownTicksRemaining--;

            DrawAngleOffset += angleOffsetVel;
            DrawAngleOffset *= 0.85f;
            angleOffsetVel *= 0.8f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Chance(float chance)
        {
            return chance switch
            {
                <= 0f => false,
                >= 1f => true,
                _ => Rand.Chance(chance)
            };
        }

        public virtual bool CanDeflect(DamageInfo info)
        {
            // Check that we aren't on cooldown.
            if (IsOnCooldown)
                return false;

            // If there is already a hit part specified, best not deflect it.
            // All normal projectiles will not have a pre-defined hit part, so it is likely that this
            // is some kind of special damage. Best not interfere.
            if (info.HitPart != null)
                return false;

            // If a weapon cannot be determined, the attack cannot be reflected.
            if (info.Weapon == null)
                return false;

            // Can't deflect a sword swing, unfortunately.
            if (info.Weapon.IsMeleeWeapon)
                return false;

            // Weapon must be a projectile firing weapon.
            //if (!info.Weapon.IsWeaponUsingProjectiles)
            //    return false;

            return true;
        }

        public virtual DeflectionResult GetDeflectionResult(DamageInfo info)
        {
            // The damage info is currently unused, but I'm leaving it in
            // in case a subclass wants to use armor pen as a factor in chance, for example.

            bool deflect = Chance(Props.deflectChance);
            if (!deflect)
                return DeflectionResult.NoChange;

            bool reflect = Chance(Props.reflectChance);
            return reflect ? DeflectionResult.Reflected : DeflectionResult.Deflected;
        }

        protected void SpawnDeflectionMote()
        {
            var pawn = Wielder;
            var map = pawn.Map;
            MoteMaker.ThrowMicroSparks(pawn.DrawPos, map);
            MoteMaker.ThrowLightningGlow(pawn.DrawPos, map, 0.5f);
        }

        protected void TakeWeaponDamage()
        {
            int damage = Props.damageToWeapon;
            if (damage <= 0)
                return;

            if (!parent.def.useHitPoints)
                return;

            int hp = parent.HitPoints;
            hp -= damage;
            if (hp < 1)
                hp = 1;
            parent.HitPoints = hp;
        }

        protected void DoWeaponRecoil()
        {
            // Makes the held item 'shake' or 'recoil' as if the impact of the bullet
            // violently pushes the blade towards the user as the projectile is deflected.

            angleOffsetVel = Rand.Range(15f, 25f) * Rand.Sign;
        }

        protected CompEquippable GetWeapon(Thing instigator, ThingDef weaponDef)
        {
            switch (instigator)
            {
                case Pawn pawn:
                    return pawn.equipment?.PrimaryEq;
                case Building_TurretGun turret:
                    return turret.GunCompEq;
                default:
                    //Log.Warning($"Failed to find weapon for instigator {instigator.LabelCap} ({instigator.GetType().Name}). Looking for weapon: {weaponDef.LabelCap}");
                    return null;
            }
        }

        protected virtual Verb MakeReflectionVerb(Pawn pawn, Verb original)
        {
            Verb created;
            if (original is Verb_Shoot)
            {
                // Most common case, so use regular constructor instead of reflection for speed.
                created = new Verb_Shoot();
            }
            else
            {
                try
                {
                    created = Activator.CreateInstance(original.GetType()) as Verb;
                }
                catch
                {
                    // Ignore, exception doesn't really matter.
                    return null;
                }
            }

            created.caster = pawn;

            var props = original.verbProps.MemberwiseClone();
            props.warmupTime = 0;
            props.defaultCooldownTime = 0;

            props.accuracyTouch *= Props.accuracyMultiTouch;
            props.accuracyShort *= Props.accuracyMultiShort;
            props.accuracyMedium *= Props.accuracyMultiMedium;
            props.accuracyLong *= Props.accuracyMultiLong;

            // This is actually very important.
            // Due to the way the verb 'burst' code works,
            // if the burst count is only 1 then the pawn's stance is not adjusted.
            // Therefore force the burst count to be 2, to update the stance.
            // This stance update makes the pawn look towards whoever shot them as they reflect the bullet.
            // Note: Even if the burst count is 2, only the single projectile is reflected,
            // because the verb is not ticked.
            props.burstShotCount = 2;

            // TODO add deflect sound.
            props.soundCast = SoundDefOf.MetalHitImportant;

            created.verbProps = props;
            created.verbTracker = pawn.VerbTracker;

            return created;
        }

        protected virtual bool HandleDeflection(DamageInfo dinfo)
        {
            var thisPawn = Wielder;
            if (thisPawn == null || thisPawn.Dead || thisPawn.Downed)
                return false;

            // Spawn a little deflection mote.
            SpawnDeflectionMote();
            TakeWeaponDamage();
            DoWeaponRecoil();

            return true;
        }

        protected virtual bool HandleReflection(DamageInfo dinfo)
        {
            var thisPawn = Wielder;
            if (thisPawn == null || thisPawn.Dead || thisPawn.Downed)
            {
                Core.Warn($"CompDeflector missing pawn?! ({ParentHolder})");
                return false;
            }

            // Spawn a little deflection mote.
            SpawnDeflectionMote();
            TakeWeaponDamage();
            DoWeaponRecoil();

            var weapon = GetWeapon(dinfo.Instigator, dinfo.Weapon);
            var weaponVerb = weapon?.PrimaryVerb;
            if (weaponVerb == null)
                return true; // Treats this situation as a deflection rather than a reflection.

            var reflectVerb = MakeReflectionVerb(thisPawn, weaponVerb);
            if (reflectVerb == null)
                return true; // Like above, if creating the verb fails, treat as a deflection.

            reflectVerb.TryStartCastOn(dinfo.Instigator);

            return true;
        }

        public void WielderPostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            if (!Enabled)
            {
                absorbed = false;
                return;
            }

            bool canDeflect = CanDeflect(dinfo);
            if (!canDeflect)
            {
                absorbed = false;
                return;
            }

            DeflectionResult result = GetDeflectionResult(dinfo);
            if (result != DeflectionResult.NoChange)
                cooldownTicksRemaining = Props.cooldown;

            switch (result)
            {
                case DeflectionResult.NoChange:
                    absorbed = false;
                    return;

                case DeflectionResult.Deflected:
                    absorbed = HandleDeflection(dinfo);
                    return;

                case DeflectionResult.Reflected:
                    absorbed = HandleReflection(dinfo);
                    return;

                default:
                    absorbed = false;
                    return;
            }
        }

        public enum DeflectionResult
        {
            NoChange,
            Deflected,
            Reflected
        }
    }
}
