using System;
using RimForge.Power;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_WirelessPowerPylon : Building
    {
        public CompPowerTrader Power { get; private set; }
        public CompFlickable Flick { get; private set; }

        public PowerChannel Channel
        {
            get
            {
                if (_channel == null)
                    return null;
                if (_channel.Destroyed)
                    _channel = null;
                return _channel;
            }
            set => _channel = value;
        }
        private PowerChannel _channel;

        public PylonType Type { get; private set; } = PylonType.None;
        public bool IsFlickedOn => Flick?.SwitchIsOn ?? false;
        public bool HasPowerNet => Power?.PowerNet != null;
        public bool IsPowered => Power?.PowerOn ?? false;
        public float Watts => Power?.PowerOutput ?? 0;

        public int ReceiverRequestedWatts;
        public int ReceiverPriority;

        private float currentPower;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Power = GetComp<CompPowerTrader>();
            Flick = GetComp<CompFlickable>();

            if (!respawningAfterLoad)
            {
                Type = PylonType.Receiver;
            }
        }

        public void SetReceiverOutput(float watts)
        {
            // targetPower should be positive.
            this.currentPower = watts;
        }

        public void SetTransmitterInput(float watts)
        {
            // targetPower should be negative.
            this.currentPower = -Mathf.Abs(watts);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            // Note: Not really necessary, since this lists in the channel are constantly sanitized.
            // However, it is good practice anyway.
            base.DeSpawn(mode);
            Channel?.UnRegister(this);
        }

        public override void Tick()
        {
            base.Tick();

            if (Channel == null)
            {
                currentPower = 0;
            }
            Power.powerOutputInt = currentPower;
        }

        public override string GetInspectString()
        {
            return SummaryString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
            // TODO implement
        }

        public void SwitchToChannel(PowerChannel channel)
        {
            var current = this.Channel;
            current?.UnRegister(this);

            channel.Register(this); // Will register as a receiver or transmitter depending on current type.
            this.Channel = channel;
        }

        public string SummaryString()
        {
            bool Common(out string msg)
            {
                if (!HasPowerNet)
                {
                    msg = "RF.Pylon.NoNetwork".Translate();
                    return true;
                }
                if (!IsFlickedOn)
                {
                    msg = "RF.Pylon.NoFlick".Translate();
                    return true;
                }
                if (Channel == null)
                {
                    msg = "RF.Pylon.NoChannel".Translate();
                    return true;
                }
                msg = null;
                return false;
            }

            switch (Type)
            {
                case PylonType.Receiver:

                    if (Common(out string msg))
                        return msg;
                    bool hasFull = Math.Abs(ReceiverRequestedWatts - currentPower) < 0.5f;
                    bool isEmpty = currentPower < 0.5f;
                    string channelName = Channel.Name;
                    int powerInt = Mathf.RoundToInt(currentPower);
                    if (hasFull)
                        return "RF.Pylon.ReceivingFull".Translate(powerInt, channelName);
                    if (isEmpty)
                        return "RF.Pylon.ReceivingNone".Translate(channelName);
                    return "RF.Pylon.ReceivingPart".Translate(powerInt, ReceiverRequestedWatts, channelName);

                case PylonType.Transmitter:

                    if (Common(out msg))
                        return msg;

                    bool sending = IsPowered;
                    powerInt = Mathf.RoundToInt(currentPower);
                    channelName = Channel.Name;

                    if (sending)
                        return "RF.Pylon.SendingFull".Translate(-powerInt, channelName);
                    return "RF.Pylon.SendingNone".Translate(channelName);

                default:
                    return "RF.Pylon.InvalidState".Translate();
            }

        }
    }

    public enum PylonType
    {
        None,
        Transmitter,
        Receiver
    }
}
