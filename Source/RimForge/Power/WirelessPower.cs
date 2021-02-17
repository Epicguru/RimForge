using RimForge.Comps;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimForge.Power
{
    /// <summary>
    /// Handles and saves everything relating to wireless power in this map.
    /// </summary>
    public class WirelessPower : WorldComponent
    {
        public class CopyPasteData
        {
            public readonly int ChannelId;
            public readonly int TargetPower;
            public readonly WirelessType Type;

            private readonly PowerChannel channel;

            public CopyPasteData(CompWirelessPower comp)
            {
                ChannelId = comp?.Channel?.Id ?? -1;
                TargetPower = comp?.TargetWatts ?? 0;
                Type = comp?.Type ?? WirelessType.None;

                channel =  ChannelId == -1 ? null : comp?.Manager?.TryGetChannel(ChannelId);
            }

            public bool IsValid(WirelessPower manager)
            {
                if (manager == null)
                    return false;

                if (channel == null || channel.Destroyed)
                    return false;

                if (Type == WirelessType.None)
                    return false;
                
                return true;
            }

            public bool TryApply(CompWirelessPower comp)
            {
                if(ChannelId  == -1)
                {
                    Core.Error("Tried to apply using an invalid CopyPasteData");
                    return false;
                }
                if (comp == null)
                {
                    Core.Error("Tried to apply CopyPasteData to a null wireless component.");
                    return false;
                }
                if (channel == null || channel.Destroyed)
                {
                    Core.Error("Tried to apply CopyPasteData that has a null or invalid channel. Check data.IsValid() first.");
                    return false;
                }

                if (!comp.Props.canSendPower && Type == WirelessType.Transmitter)
                {
                    Core.Warn($"Did not paste wireless settings on to {comp.parent?.LabelCap}, because it is not allowed to send power.");
                    return false;
                }

                if (comp.Props.fixedPowerLevel != null)
                    Core.Warn($"Pasting wireless settings, even though this {comp.parent?.LabelCap} has a fixed power level.");
                

                // Already has these settings?
                if (comp.Type == Type && (comp.Channel?.Id ?? -1) == ChannelId && comp.TargetWatts == TargetPower)
                    return true;

                comp.SwitchToChannel(channel);
                comp.SwitchType(Type);
                if(comp.Props.fixedPowerLevel == null)
                    comp.TargetWatts = TargetPower;
                return true;
            }

            public override string ToString()
            {
                // {0} {1}W {2} '{3}'
                string a = Type == WirelessType.Receiver ? "RF.Pylon.Request".Translate() : "RF.Pylon.Send".Translate();
                string b = TargetPower.ToString();
                string c = Type == WirelessType.Receiver ? "RF.Pylon.From".Translate() : "RF.Pylon.To".Translate();
                string d = channel?.Name;
                return $"{a} {b}W {c} '{d}'";
            }
        }

        public CopyPasteData CurrentCopyPasteData;
        public int MaxChannelId => maxId;

        private int maxId;
        private Dictionary<int, PowerChannel> channels = new Dictionary<int, PowerChannel>();

        public WirelessPower(World world) : base(world)
        {
            Core.Log($"Created wireless power component for world '{world.info.name}'");
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

        public PowerChannel TryGetDefaultChannel()
        {
            if ((channels?.Count ?? 0) == 0)
                return null;

            return channels.FirstOrFallback().Value;
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
            if (channels.Count == 0)
            {
                CreateNewChannel("Default Channel");
            }
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
