using RimForge.Disco;
using RimForge.Disco.Programs;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class DiscoProgramDef : Def
    {
        public Type programClass;
        public List<Color> colors = new List<Color>();
        public List<int> ints = new List<int>();
        public List<float> floats = new List<float>();
        public List<bool> bools = new List<bool>();
        public List<string> strings = new List<string>();

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

        public DiscoProgram MakeProgram(Building_DJStand stand)
        {
            if (programClass == null)
                return null;

            var instance = Activator.CreateInstance(programClass, this) as DiscoProgram;
            if (instance == null)
                return null;

            instance.DJStand = stand;
            instance.Init();
            return instance;
        }
    }
}
