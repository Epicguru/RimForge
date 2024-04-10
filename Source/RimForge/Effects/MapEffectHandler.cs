using System.Collections.Generic;
using System.Threading;
using LudeonTK;
using RimForge.Patches;
using RimWorld.Planet;
using Verse;

namespace RimForge.Effects
{
    public class MapEffectHandler : WorldComponent
    {
        /// <summary>
        /// Slow!
        /// </summary>
        public static MapEffectHandler Current => Find.World?.GetComponent<MapEffectHandler>();
        public static ThreadedEffectHandler ThreadedHandler = new ThreadedEffectHandler();

        //private TraitTracker traitTracker;

        [DebugAction("RimForge", "Debug Map Effects")]
        private static void DebugMapEffects()
        {
            var instance = Current;
            if(instance == null)
            {
                Core.Error("Failed to find map effect handler instance.");
                return;
            }

            int mapCount = instance.effects.Count;
            Core.Log($"Threaded handler running: {ThreadedHandler.IsRunning}");
            Core.Log($"There are {mapCount} maps with effects:");
            foreach(var pair in instance.effects)
            {
                Map map = null;
                if(pair.Key >= 0 && pair.Key < Find.Maps.Count)
                    map = Find.Maps[pair.Key];

                int total = pair.Value.Count;
                int dead = 0;
                int normal = 0;
                int threaded = 0;

                foreach (var effect in pair.Value)
                {
                    if (effect == null)
                        continue;

                    if (effect.Destroyed)
                    {
                        dead++;
                        continue;
                    }

                    if (effect is ThreadedEffect)
                        threaded++;
                    else
                        normal++;
                }

                Core.Log($"  -Map {(map == null ? "<null>" : $"'{map}'")}: {total} total, {normal} normal, {threaded} threaded, {dead} dead.");
            }
        }

        private readonly Dictionary<int, List<MapEffect>> effects = new Dictionary<int, List<MapEffect>>();

        public MapEffectHandler(World world) : base(world)
        {
            
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Patch_DynamicDrawManager_DrawDynamicThings.TryRegisterListener(OnDrawLate);

            if (ThreadedHandler.IsRunning)
            {
                ThreadedHandler.Stop();
                Thread.Sleep(100);
            }
            ThreadedHandler.Start();
        }

        public void RegisterEffect(MapEffect effect, Map map)
        {
            if (map == null)
            {
                Core.Error("Tried to register effect to null map.");
                return;
            }
            if (effect == null)
            {
                Core.Error("Tried to register null effect.");
                return;
            }
            if (effect.MapId != -1)
                return;

            int id = map.uniqueID;
            effect.MapId = id;

            if (effects.TryGetValue(id, out var list))
            {
                list.Add(effect);
            }
            else
            {
                var newList = new List<MapEffect>(32) { effect };
                effects.Add(id, newList);
            }

            if (effect is ThreadedEffect te)
            {
                ThreadedHandler.AddEffect(te);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            foreach (var list in effects.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (item == null || item.ShouldDeSpawn())
                    {
                        list.RemoveAt(i);
                        if (item != null)
                            item.MapId = -1;
                        i--;
                        continue;
                    }

                    item.TickAccurate();
                }
            }
        }

        private void OnDrawLate(Map map)
        {
            //traitTracker ??= TraitTracker.Current;
            //traitTracker?.OnDrawLate(map);

            if (map != null && effects.TryGetValue(map.uniqueID, out var found))
            {
                bool tick = !Find.TickManager.Paused;
                for (int i = 0; i < found.Count; i++)
                {
                    var item = found[i];
                    if (item == null || item.ShouldDeSpawn())
                    {
                        found.RemoveAt(i);
                        if (item != null)
                            item.MapId = -1;
                        i--;
                        continue;
                    }

                    item.Draw(tick, map);
                }
            }
        }
    }
}
