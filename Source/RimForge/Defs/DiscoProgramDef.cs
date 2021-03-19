using System;
using System.Collections.Generic;
using RimForge.Buildings.DiscoPrograms;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class DiscoProgramDef : Def
    {
        public Type programClass;
        public List<Color> colors = new List<Color>();
        public List<int> ints = new List<int>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(var item in base.ConfigErrors())
                yield return item;

            if (programClass == null)
                yield return "Null programClass. Check spelling and recompile?";
            else if (!typeof(DiscoProgram).IsAssignableFrom(programClass))
            {
                yield return $"programClass '{programClass.FullName}' is not a subclass of DiscoProgram. Expect errors.";
                programClass = null;
            }
        }

        public DiscoProgram MakeProgram()
        {
            if (programClass == null)
                return null;

            var instance = Activator.CreateInstance(programClass, this) as DiscoProgram;
            if (instance == null)
                return null;

            instance.Init();
            return instance;
        }
    }
}
