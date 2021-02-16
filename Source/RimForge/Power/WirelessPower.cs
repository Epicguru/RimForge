using RimWorld.Planet;
using System.Collections.Generic;
using RimForge.Buildings;
using RimForge.Comps;
using Verse;

namespace RimForge.Power
{
    /// <summary>
    /// Handles and saves everything relating to wireless power in this map.
    /// </summary>
    public class WirelessPower : WorldComponent
    {
        public int MaxChannelId => maxId;

        private int maxId;
        private Dictionary<int, PowerChannel> channels = new Dictionary<int, PowerChannel>();

        public WirelessPower(World world) : base(world)
        {
            Core.Log($"Created wireless power component for world '{world.info.name}'");
            CreateNewChannel("JamesNet");
        }

        public IEnumerable<PowerChannel> GetAvailableChannels(CompWirelessPower pylon)
        {
            if (pylon == null)
                yield break;

            var current = pylon.Channel;
            foreach (var pair in channels)
            {
                if (pair.Value != current)
                    yield return pair.Value;
            }
        }

        public PowerChannel TryGetChannel(int id)
        {
            if (channels.TryGetValue(id, out var found))
                return found;

            return null;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref maxId, "maxId", 0);
            Scribe_Collections.Look(ref channels, "channels", LookMode.Undefined, LookMode.Deep);
            channels ??= new Dictionary<int, PowerChannel>();
        }

        public int CreateNewChannel(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Core.Error($"Null or blank name in WirelessPower.CreateNewChannel(): {name ?? "<null>"}");
                return -1;
            }

            int newId = maxId + 1;
            maxId++;
            var channel = new PowerChannel {Id = newId, Name = name.Trim()};
            channels.Add(channel.Id, channel);
            return newId;
        }

        public void DeleteChannel(int id)
        {
            var channel = TryGetChannel(id);
            if (channel == null)
            {
                Core.Error($"Tried to delete channel {id}, but that channel could not be found.");
                return;
            }

            channel.MarkDestroyed();
            channels.Remove(channel.Id);
        }

        public bool IsValidNewChannelName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.Trim();
            foreach (var pair in channels)
            {
                if (pair.Value.Destroyed)
                    continue;

                if (pair.Value.Name == name)
                    return false;
            }
            return true;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            foreach (var pair in channels)
            {
                pair.Value.Tick();
            }
        }
    }
}
