using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimForge.Disco.Audio
{
    public static class SoundDefGenerator
    {
        private static FieldInfo resolvedGrainsInfo = typeof(SubSoundDef).GetField("resolvedGrains", BindingFlags.Instance | BindingFlags.NonPublic);

        public static SoundDef GenerateFor(ModContentPack mod, string defName, string audioFilePath, AudioType audioType, bool register = true)
        {
            SoundDef def = new SoundDef();
            def.defName = defName;
            def.label = defName;
            def.description = "An auto-generated SoundDef from the Disco! mod.";
            def.modContentPack = mod;

            SubSoundDef sub = new SubSoundDef();
            sub.name = "Main sub";
            sub.onCamera = true;
            sub.muteWhenPaused = true;
            sub.tempoAffectedByGameSpeed = true;
            sub.sustainLoop = false;
            sub.parentDef = def;
            def.subSounds.Add(sub);

            var grain = new AudioGrain_DiscoLoaded();
            grain.filePath = audioFilePath;
            grain.fileType = audioType;
            sub.grains.Add(grain);

            var grains = resolvedGrainsInfo.GetValue(sub) as List<ResolvedGrain>;
            foreach (var g in sub.grains)
            {
                Task.Run((g as AudioGrain_DiscoLoaded).GetResolvedGrainAsync).ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Core.Error($"Exception loading custom disco audio grain '{defName}'", task.Exception);
                        return;
                    }
                    grains.Add(task.Result);
                });
            }

            if (register)
                DefDatabase<SoundDef>.Add(def);

            return def;
        }
    }
}
