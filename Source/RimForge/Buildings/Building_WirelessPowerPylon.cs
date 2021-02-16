using System;
using RimForge.Power;
using RimWorld;
using System.Collections.Generic;
using RimWorld.Planet;
using RuntimeAudioClipLoader;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_WirelessPowerPylon : Building
    {
        public WirelessPower Manager => Current.Game?.World?.GetComponent<WirelessPower>();

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

        /// <summary>
        /// Used to forcibly disable when there is a receiver and transmitter on the
        /// same power grid, leading to an infinite feedback loop.
        /// </summary>
        public bool ForceDisable { get; set; }

        public int TargetWatts = 200;
        public int ReceiverPriority;

        private float currentPower;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Power = GetComp<CompPowerTrader>();
            Flick = GetComp<CompFlickable>();

            if (!respawningAfterLoad)
            {
                SwitchType(PylonType.Receiver);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref TargetWatts, "targetWatts", 200);
            Scribe_Values.Look(ref currentPower, "currentPower", 0);
            Scribe_Values.Look(ref ReceiverPriority, "receiverPriority", 0);

            var type = Type;
            Scribe_Values.Look(ref type, "type", PylonType.Receiver);
            SwitchType(type);

            int channelId = Channel?.Id ?? -1;
            Scribe_Values.Look(ref channelId, "channelId", -1);
            Channel = Manager.TryGetChannel(channelId);
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
            return $"{base.GetInspectString()}\n{SummaryString()}";
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var item in base.GetGizmos())
                yield return item;

            yield return new Command_Action()
            {
                action = () =>
                {
                    if (Type == PylonType.Receiver)
                        SwitchType(PylonType.Transmitter);
                    else
                        SwitchType(PylonType.Receiver);
                },
                defaultLabel = "Change mode",
                defaultDesc = $"Current: {Type}"
            };
            yield return new Command_Action()
            {
                action = () =>
                {
                    Find.WindowStack.Add(new ConfigWindow(){Pylon = this});
                },
                defaultLabel = "Configure",
                defaultDesc = $"Configure this pylon"
            };
            yield return new Command_Action()
            {
                action = () =>
                {
                    TargetWatts += 100;
                    if (TargetWatts > 2000)
                        TargetWatts = 0;
                },
                defaultLabel = "Change power",
                defaultDesc = $"Current: {TargetWatts}"
            };
            yield return new Command_Action()
            {
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    if (Manager != null)
                    {
                        foreach (var item in Manager.GetAvailableChannels(this))
                        {
                            var op = new FloatMenuOption(item.Name, () =>
                            {
                                SwitchToChannel(item);
                            });
                            options.Add(op);
                        }
                    }
                    Find.WindowStack.Add((Window)new FloatMenu(options));
                },
                defaultLabel = "Set channel",
                defaultDesc = $"Current: {Channel?.Name ?? "None"}"
            };
        }

        public void SwitchToChannel(PowerChannel channel)
        {
            var current = this.Channel;
            current?.UnRegister(this);

            if (channel != null && !channel.Destroyed)
            {
                channel.Register(this); // Will register as a receiver or transmitter depending on current type.
                this.Channel = channel;
            }
            else
            {
                this.Channel = null;
            }
        }

        public void SwitchType(PylonType newType)
        {
            if (newType == PylonType.None)
            {
                Core.Error("Cannot switch to PylonType.None");
                return;
            }

            Type = newType;
            Channel?.Register(this); // This will swap types within the channel.
            Core.Log($"Changed type to {Type}");
        }

        public string SummaryString(bool richText = true)
        {
            string InColor(string txt, string color)
            {
                return !richText ? txt : $"<color={color}>{txt}</color>";
            }

            bool Common(out string msg)
            {
                if (ForceDisable && Channel != null && Type == PylonType.Receiver)
                {
                    msg = "RF.Pylon.ForceDisabled".Translate(Channel.Name);
                    return true;
                }
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

            const string GREEN = "#97ff57";
            const string YELLOW = "#ffdf4f";
            const string RED = "#ff5757";

            switch (Type)
            {
                case PylonType.Receiver:

                    if (Common(out string msg))
                        return msg;
                    bool hasFull = Math.Abs(TargetWatts - currentPower) < 0.5f;
                    bool isEmpty = currentPower < 0.5f;
                    string channelName = Channel.Name;
                    int powerInt = Mathf.RoundToInt(currentPower);
                    if (hasFull)
                        return "RF.Pylon.ReceivingFull".Translate(InColor($"{TargetWatts}W", GREEN), channelName);
                    if (isEmpty)
                        return "RF.Pylon.ReceivingNone".Translate(InColor("0W", RED), channelName);
                    return "RF.Pylon.ReceivingPart".Translate(InColor($"{powerInt}/{TargetWatts}W", YELLOW), channelName);

                case PylonType.Transmitter:

                    if (Common(out msg))
                        return msg;

                    bool sending = IsPowered;
                    powerInt = Mathf.RoundToInt(currentPower);
                    channelName = Channel.Name;

                    if (sending)
                        return "RF.Pylon.SendingFull".Translate(InColor($"{-powerInt}W", GREEN), channelName);
                    return "RF.Pylon.SendingNone".Translate(InColor("0W", RED), channelName);

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

    public class ConfigWindow : Window
    {
        public override Vector2 InitialSize => new Vector2(810, 200);
        public Building_WirelessPowerPylon Pylon;

        private string wattsBuffer;

        public ConfigWindow()
        {
            optionalTitle = "RF.Pylon.ConfigureTitle".Translate();
            drawShadow = true;
            draggable = true;
            preventCameraMotion = false;
            resizeable = false;
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Pylon == null || Pylon.Destroyed || !Pylon.Spawned || Pylon.Type == PylonType.None)
            {
                Core.Warn("Pylon was destroyed or invalid, closing config window.");
                Close();
                return;
            }
            Text.Font = GameFont.Medium;

            const float h = 28;
            const float p = 6;
            float x = inRect.x;
            float y = inRect.y;

            bool isSending = Pylon.Type == PylonType.Transmitter;
            string channelName = Pylon.Channel?.Name ?? "None";

            string txt = isSending ? "RF.Pylon.Sending".Translate() : "RF.Pylon.Requesting".Translate();
            if (Widgets.ButtonText(new Rect(x, y, 150, h), txt))
            {
                Pylon.SwitchType(isSending ? PylonType.Receiver : PylonType.Transmitter);
            }
            x += 150 + p;
            int amount = Pylon.TargetWatts;
            Widgets.IntEntry(new Rect(x, y, 250, h), ref amount, ref wattsBuffer);
            Pylon.TargetWatts = Mathf.Clamp(amount, 0, 999999); // Allow up to (almost) a megawatt.
            wattsBuffer = Pylon.TargetWatts.ToString();
            x += 250 + p;
            txt = "RF.Pylon.On".Translate();
            var size = Text.CalcSize(txt);
            Widgets.Label(new Rect(x, y, size.x, h), txt);
            x += size.x + p;
            if (Widgets.ButtonText(new Rect(x, y, 200, h), channelName))
            {
                var options = new List<FloatMenuOption>();
                if (Pylon.Manager != null)
                {
                    foreach (var item in Pylon.Manager.GetAvailableChannels(Pylon))
                    {
                        var op = new FloatMenuOption(item.Name, () =>
                        {
                            Pylon.SwitchToChannel(item);
                        });
                        options.Add(op);
                    }
                }

                options.Add(new FloatMenuOption("RF.Pylon.CreateNewChannel".Translate(), () =>
                {
                    Find.WindowStack.Add(new NewChannelWindow() {Manager = Pylon.Manager, CreateNewChannel = name =>
                    {
                        Core.Log($"User created new channel '{name}'");
                        int id = Pylon.Manager.CreateNewChannel(name);
                        var newChannel = Pylon.Manager.TryGetChannel(id);

                        if (newChannel == null)
                            Core.Error("Create new channel, but TryGetChannel returned null!? Why?");

                        Pylon.SwitchToChannel(newChannel);
                    }});
                }));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            var channel = Pylon.Channel;
            if (channel == null || channel.Destroyed)
                return;

            Text.Font = GameFont.Small;
            x = 0;
            y += h + 10;
            int input = (int) channel.TotalInputWatts;
            int requested = (int) channel.TotalRequestedWatts;
            int balanceInt = input - requested;
            string balance = balanceInt > 0 ? $"<color=green>+{balanceInt}W</color>" : balanceInt < 0 ? $"<color=red>{balanceInt}W</color>" : "0W";
            Widgets.Label(new Rect(x, y, inRect.width, inRect.height - y), $"{"RF.Pylon.ChannelInfoHeader".Translate(channel.Name)}\n" +
                                                                           $"{"RF.Pylon.ChannelInfoInput".Translate(channel.ActiveTransmitterCount, input)}\n" +
                                                                           $"{"RF.Pylon.ChannelInfoOutput".Translate(channel.ActiveReceiversCount, requested)}\n" +
                                                                           $"{"RF.Pylon.ChannelInfoBalance".Translate(balance)}" + 
                                                                           (channel.UnsatisfiedReceivers > 0 ? "\n" + (string)"RF.Pylon.ChannelInfoUnsatisfied".Translate(channel.UnsatisfiedReceivers) : ""));
        }
    }

    public class NewChannelWindow : Window
    {
        public override Vector2 InitialSize => new Vector2(560, 65);

        public WirelessPower Manager;
        public Action<string> CreateNewChannel;

        private string name;

        public NewChannelWindow()
        {
            closeOnClickedOutside = true;
            resizeable = false;
            drawShadow = true;
            doCloseButton = false;
            doCloseX = false;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            windowRect.y -= 165;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Manager == null)
            {
                Core.Error("Missing Manager in this NewChannelWindow, closing.");
                Close();
                return;
            }

            if (name == null)
            {
                name = MakeDefaultName();
            }

            Text.Font = GameFont.Medium;
            string txt = "RF.Pylon.NewChannelName".Translate();
            Vector2 size = Text.CalcSize(txt);
            float x = inRect.x;
            float y = inRect.y;
            Widgets.Label(new Rect(x, y, size.x, size.y), txt);
            x += size.x + 10;
            name = Widgets.TextField(new Rect(x, y, 220, size.y), name);
            if (name.Length > 20)
                name = name.Substring(0, 20);
            x += 230;

            bool validName = Manager.IsValidNewChannelName(name);
            if (Widgets.ButtonText(new Rect(x, y, 100, size.y), "RF.Pylon.NewChannelCreate".Translate()))
            {
                string finalName = name.Trim();
                Close();
                CreateNewChannel?.Invoke(finalName);
            }
        }

        public string MakeDefaultName()
        {
            return $"Channel #{Manager.MaxChannelId}";
        }
    }
}
