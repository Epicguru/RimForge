using RimForge.Comps;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimForge.Power
{
    public class PowerChannel : IExposable
    {
        private static readonly Color[] Colors = 
        {
            Color.magenta,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.red
        };
        private static List<ThingWithComps> tempIOThings = new List<ThingWithComps>();

        public int Id;
        public string Name;
        public bool Destroyed;

        /// <summary>
        /// Gets a list of all the pylons on this channel that are receivers.
        /// However, some of these pylons may be switched off or disconnected from a network.
        /// See <see cref="ActiveReceivers"/> to filter those out.
        /// </summary>
        public List<CompWirelessPower> Receivers
        {
            get
            {
                Sanitize(_receivers);
                return _receivers;
            }
        }
        /// <summary>
        /// Gets a list of all the pylons on this channel that are transmitters.
        /// However, some of these pylons may be switched off or disconnected from a network.
        /// See <see cref="ActiveTransmitters"/> to filter those out.
        /// </summary>
        public List<CompWirelessPower> Transmitters
        {
            get
            {
                Sanitize(_transmitters);
                return _transmitters;
            }
        }
        public IEnumerable<CompWirelessPower> ActiveReceivers
        {
            get
            {
                foreach (var item in Receivers)
                {
                    if (item.HasPowerNet && item.IsFlickedOn)
                        yield return item;
                }
            }
        }
        public IEnumerable<CompWirelessPower> ActiveTransmitters
        {
            get
            {
                foreach (var item in Transmitters)
                {
                    if (item.HasPowerNet && item.IsFlickedOn && item.IsPowered)
                        yield return item;
                }
            }
        }
        public int ActiveTransmitterCount => ActiveTransmitters.Count();
        public int ActiveReceiversCount => ActiveReceivers.Count();
        public int UnsatisfiedReceivers { get; private set; }

        /// <summary>
        /// Gets the total current input watts to the channel, from
        /// all active transmitters.
        /// </summary>
        public float TotalInputWatts
        {
            get
            {
                float sum = 0;
                foreach (var trs in ActiveTransmitters)
                {
                    sum += -trs.Watts; // These watts are negative because they are pulling power.
                }
                return sum;
            }
        }

        /// <summary>
        /// Gets the total current requested watts, from
        /// all active receivers.
        /// </summary>
        public float TotalRequestedWatts
        {
            get
            {
                float sum = 0;
                foreach (var trs in ActiveReceivers)
                {
                    sum += trs.TargetWatts;
                }
                return sum;
            }
        }

        private List<CompWirelessPower> _receivers = new List<CompWirelessPower>();
        private List<CompWirelessPower> _transmitters = new List<CompWirelessPower>();
        private HashSet<PowerNet> tempNets = new HashSet<PowerNet>();

        private void Sanitize(List<CompWirelessPower> pylons)
        {
            if (pylons == null)
                return;

            for (int i = 0; i < pylons.Count; i++)
            {
                var pylon = pylons[i];
                bool remove = false;

                if (pylon == null)
                    remove = true;
                else if (pylon.parent.DestroyedOrNull() || !pylon.parent.Spawned)
                    remove = true;

                if (remove)
                {
                    pylons.RemoveAt(i);
                    i--;
                }
            }
        }

        public Color GetColor() => Colors[Id % Colors.Length];

        public void MarkDestroyed()
        {
            Destroyed = true;
            _transmitters = null;
            _receivers = null;
            tempNets = null;
        }

        public void Register(CompWirelessPower pylon)
        {
            if (pylon == null)
                return;
            if (pylon.Type == WirelessType.None)
                return;

            if (pylon.Type == WirelessType.Receiver)
            {
                if (_transmitters.Contains(pylon))
                    _transmitters.Remove(pylon);
                if (!_receivers.Contains(pylon))
                    _receivers.Add(pylon);
            }
            else
            {
                if (_receivers.Contains(pylon))
                    _receivers.Remove(pylon);
                if (!_transmitters.Contains(pylon))
                    _transmitters.Add(pylon);
            }
        }

        public void UnRegister(CompWirelessPower pylon)
        {
            if (pylon == null)
                return;

            if (_receivers.Contains(pylon))
                _receivers.Remove(pylon);
            if (_transmitters.Contains(pylon))
                _transmitters.Remove(pylon);
        }

        public void Tick()
        {
            tempNets.Clear();

            foreach (var pylon in Transmitters)
            {
                // Yes, this is pretty stupid, the pylon could just do this in it's own tick.
                // However, this is more consistent with the way the receivers are handled.
                pylon.SetTransmitterInput(pylon.TargetWatts);

                // It also allows me to do this necessary feedback-loop prevention code:
                pylon.ForceDisable = false;
                var net = pylon.Power?.PowerNet;
                if (net != null)
                    tempNets.Add(net);
            }

            float wattsToDistribute = TotalInputWatts;
            UnsatisfiedReceivers = 0;
            Receivers.Sort((a, b) => a.ReceiverPriority - b.ReceiverPriority);
            foreach (var pylon in Receivers)
            {
                if (!pylon.IsFlickedOn || !pylon.HasPowerNet)
                {
                    // Pylon is not on a power network or is switched off, don't send it power.
                    pylon.SetReceiverOutput(0f); 
                    continue;
                }
                var net = pylon.Power.PowerNet;
                if (tempNets.Contains(net))
                {
                    // There is a transmitter on the same power grid as this receiver...
                    // No good.
                    pylon.ForceDisable = true;
                    pylon.SetReceiverOutput(0f);
                    continue;
                }

                pylon.ForceDisable = false;
                float requested = Mathf.Max(0, pylon.TargetWatts);
                if (wattsToDistribute >= requested)
                {
                    // There is enough power to give to this pylon, nice.
                    wattsToDistribute -= requested;
                    pylon.SetReceiverOutput(requested);
                }
                else
                {
                    // There is not enough power for this pylon's requested amount.
                    // Give it the remainder of the power, or 0 if there is none left.
                    pylon.SetReceiverOutput(wattsToDistribute);
                    wattsToDistribute = 0;
                    UnsatisfiedReceivers++;
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Id, "id", -1);
            if (Id == -1)
                Core.Error("Channel '{Name}' exposed data, Id came back as -1.");
            Scribe_Values.Look(ref Name, "name", "<ERR_MISSING_NAME>");
            Scribe_Values.Look(ref Destroyed, "destroyed", false);

            ExposeComps(_transmitters, "transmitters");
            ExposeComps(_receivers, "receivers");
        }

        /// <summary>
        /// Exposes the list of comps.
        /// LookMode.Reference cannot be used with components, because they are not registered in the load
        /// system the same way Things are.
        /// Therefore, it is necessary to save a reference to the comp's parent, and then extract the comp afterwards.
        /// </summary>
        private void ExposeComps(List<CompWirelessPower> comps, string label)
        {
            tempIOThings.Clear();
            foreach (var comp in comps)
                tempIOThings.Add(comp.parent);

            Scribe_Collections.Look(ref tempIOThings, label, LookMode.Reference);
            tempIOThings ??= new List<ThingWithComps>();

            comps.Clear();
            foreach (var thing in tempIOThings)
            {
                var comp = thing.GetComp<CompWirelessPower>();
                if (comp != null)
                    comps.Add(comp);
                else
                    Core.Error($"Failed to find CompWirelessPower on loading thing: {thing}");
            }
            tempIOThings.Clear();
        }
    }
}
