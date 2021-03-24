using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RimForge.Disco.Audio;
using UnityEngine;
using Verse;

namespace RimForge.Disco.Programs
{
    public class SongPlayer : DiscoProgram
    {
        private static readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

        [DebugAction("Disco!", actionType = DebugActionType.Action)]
        private static void LogLoadedClips()
        {
            Core.Log($"There are {clipCache.Count} loaded audio clips:");
            foreach (var pair in clipCache)
            {
                Core.Log($"{new FileInfo(pair.Key).Name}: {pair.Value.length} seconds of {pair.Value.frequency / 1000}Hz, {pair.Value.channels} channels.");
            }
        }

        private ManagedAudioSource source;
        private bool removed = false;
        private float volume;
        private AudioClip clipToAdd;
        private string filePath;
        private bool playingLastFrame = true;

        public SongPlayer(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            filePath = Def.Get<string>("filePath");
            volume = Def.Get("volume", 1f);
            string format = Def.Get("format", "OGG");
            if (!Enum.TryParse<AudioType>(format.Trim(), true, out var formatEnum))
            {
                Core.Error($"Failed to parse audio format '{format}'.");
                Array arr = Enum.GetValues(typeof(AudioType));
                object[] args = arr.Cast<object>().ToArray();
                Core.Error($"Valid formats are: {string.Join(", ", args)}");
                Core.Error("Hint: Use MPEG for .mp3 files, OGGVORBIS for .ogg files, WAV for .wav files.");
                Remove();
                return;
            }

            // Resolve the file path.
            ModContentPack mcp = Def.modContentPack;
            if (mcp == null)
            {
                string[] split = filePath.Split('/');
                split[0] = split[0].ToLowerInvariant();
                Core.Warn($"Song player program def '{Def.defName}' has been added by a patch operation. Attempting to resolve filepath...");
                var found = LoadedModManager.RunningModsListForReading.FirstOrFallback(mod => mod.PackageId.ToLowerInvariant() == split[0]);
                if (found == null)
                {
                    Core.Error($"Failed to resolve mod folder path from id '{split[0]}'. See below for how to solve this issue.");
                    Core.Error("If you mod's package ID is 'my.mod.name' and your song file is in 'MyModFolder/Songs/Song.mp3' then the correct filePath would be 'my.mod.name/Songs/Song.mp3'");
                    Remove();
                    return;
                }
                Core.Warn("Successfully resolved file path.");
                mcp = found;
            }
            filePath = Path.Combine(mcp.RootDir, filePath);

            if (clipCache.TryGetValue(filePath, out var clip))
            {
                FlagLoadedForClip(clip);
            }
            else
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Core.Log($"Loading '{filePath}' as {formatEnum} ...");
                        var c = await AudioLoader.TryLoadAsync(filePath, formatEnum);
                        clipToAdd = c; // Push to main thread, see Tick()
                        Core.Log("Done");
                    }
                    catch (Exception e)
                    {
                        Core.Error($"Failed loading song clip from '{filePath}' in format {formatEnum}", e);
                        Remove();
                    }
                });
            }
        }

        private void FlagLoadedForClip(AudioClip clip)
        {
            source = AudioSourceManager.CreateSource(clip, DJStand.Map);
            source.TargetVolume = volume;
            source.Area = DJStand.FloorBounds;
            source.Source.Play();
            source.IsPlaying = () => source?.Source != null && !removed;
        }

        public override void Tick()
        {
            base.Tick();

            if (clipToAdd != null)
            {
                clipCache.Add(filePath, clipToAdd);
                FlagLoadedForClip(clipToAdd);
                clipToAdd = null;
            }

            if (source != null && source.Source != null)
            {
                bool playing = source.Source.isPlaying;
                if (!playing && !playingLastFrame)
                    Remove();
                playingLastFrame = playing;
            }
        }

        public override Color ColorFor(IntVec3 cell)
        {
            return default;
        }

        public override void Dispose()
        {
            base.Dispose();

            removed = true;
        }
    }
}
