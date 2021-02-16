using RimWorld.Planet;
using System.Collections.Generic;

namespace RimForge.Power
{
    /// <summary>
    /// Handles everything relating to wireless power in this map.
    /// </summary>
    public class WirelessPower : WorldComponent
    {
        private int maxId;
        private Dictionary<int, PowerChannel> channels = new Dictionary<int, PowerChannel>();

        public WirelessPower(World world) : base(world)
        {
            Core.Log($"Created wireless power component for world '{world.info.name}'");
        }

        public PowerChannel TryGetChannel(int id)
        {
            if (channels.TryGetValue(id, out var found))
                return found;

            return null;
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
    }
}
