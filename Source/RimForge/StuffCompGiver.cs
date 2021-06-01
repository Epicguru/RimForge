using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimForge
{
    public class StuffCompGiver : DefModExtension
    {
        public Type compClass;
        public CompProperties props;
        public bool allowDuplicate = false;
        public bool exactDuplicate = true;

        public bool onlyMeleeWeapons = false;
        public bool onlyApparel = false;

        public ThingComp TryGiveComp(ThingWithComps parent, List<ThingComp> comps, bool allowDuplicate = false, bool exactDuplicate = true)
        {
            if (compClass == null)
                return null;
            if (parent == null || comps == null)
                return null;

            if (onlyMeleeWeapons && !parent.def.IsMeleeWeapon)
                return null;

            if (onlyApparel && !(parent is Apparel))
                return null;

            if (!allowDuplicate)
            {
                foreach (var item in comps)
                {
                    if (exactDuplicate)
                    {
                        if (item.GetType() == compClass)
                            return null;
                    }
                    else
                    {
                        if (item.GetType().IsAssignableFrom(compClass))
                            return null;
                    }
                }
            }

            if (props == null)
            {
                Core.Warn($"StuffCompGiver for comp '{compClass.FullName}' has null props!");
                props = new CompProperties();
            }
            props.compClass = compClass;

            var createdComp = (ThingComp)Activator.CreateInstance(compClass);
            createdComp.parent = parent;
            comps.Add(createdComp);
            createdComp.Initialize(props);
            return createdComp;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (base.ConfigErrors() != null)
                foreach (var error in base.ConfigErrors())
                    yield return error;

            if (compClass == null)
            {
                yield return "Null comp in this StuffCompGiver.";
            }
            else if(!typeof(ThingComp).IsAssignableFrom(compClass))
            {
                yield return $"'{compClass.FullName}' does not inherit from ThingComp.";
                compClass = null;
            }
        }
    }
}
