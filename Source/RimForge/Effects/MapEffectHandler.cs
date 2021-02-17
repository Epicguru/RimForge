using System.Collections.Generic;
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
        public static MapEffectHandler Current => Find.World.GetComponent<MapEffectHandler>();

        private readonly Dictionary<int, List<MapEffect>> effects = new Dictionary<int, List<MapEffect>>();

        public MapEffectHandler(World world) : base(world)
        {
            
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Patch_DynamicDrawManager_DrawDynamicThings.TryRegisterListener(OnDrawLate);
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
        }

        private void OnDrawLate(Map map)
        {

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
