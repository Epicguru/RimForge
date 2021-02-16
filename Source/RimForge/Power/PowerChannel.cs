using RimForge.Buildings;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace RimForge.Power
{
    public class PowerChannel
    {
        public int Id;
        public string Name;
        public bool Destroyed;

        /// <summary>
        /// Gets a list of all the pylons on this channel that are receivers.
        /// However, some of these pylons may be switched off or disconnected from a network.
        /// See <see cref="ActiveReceivers"/> to filter those out.
        /// </summary>
        public List<Building_WirelessPowerPylon> Receivers
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
        public List<Building_WirelessPowerPylon> Transmitters
        {
            get
            {
                Sanitize(_transmitters);
                return _transmitters;
            }
        }
        public IEnumerable<Building_WirelessPowerPylon> ActiveReceivers
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
        public IEnumerable<Building_WirelessPowerPylon> ActiveTransmitters
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

        private List<Building_WirelessPowerPylon> _receivers = new List<Building_WirelessPowerPylon>();
        private List<Building_WirelessPowerPylon> _transmitters = new List<Building_WirelessPowerPylon>();
        private HashSet<PowerNet> tempNets = new HashSet<PowerNet>();

        private void Sanitize(List<Building_WirelessPowerPylon> pylons)
        {
            if (pylons == null)
                return;

            for (int i = 0; i < pylons.Count; i++)
            {
                var pylon = pylons[i];
                bool remove = false;

                if (pylon == null)
                    remove = true;
                else if (pylon.Destroyed || !pylon.Spawned)
                    remove = true;

                if (remove)
                {
                    pylons.RemoveAt(i);
                    i--;
                }
            }
        }

        public void MarkDestroyed()
        {
            Destroyed = true;
            _transmitters = null;
            _receivers = null;
            tempNets = null;
        }

        public void Register(Building_WirelessPowerPylon pylon)
        {
            if (pylon == null)
                return;
            if (pylon.Type == PylonType.None)
                return;

            if (pylon.Type == PylonType.Receiver)
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

        public void UnRegister(Building_WirelessPowerPylon pylon)
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
    }
}
