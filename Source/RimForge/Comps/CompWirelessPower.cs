using System;
using System.Collections.Generic;
using RimForge.Power;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge.Comps
{
    public class CompWirelessPower : ThingComp
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
        public CompProperties_WirelessPower Props => base.props as CompProperties_WirelessPower;

        public PylonType Type { get; private set; } = PylonType.None;
        public bool IsFlickedOn => Flick?.SwitchIsOn ?? false;
        public bool HasPowerNet => Power?.PowerNet != null;
        public bool IsPowered => Power?.PowerOn ?? false;
        public float Watts => Power?.PowerOutput ?? 0;
        public bool IsActive => IsFlickedOn && (currentPower != 0 && (Type != PylonType.Transmitter || IsPowered));

        /// <summary>
        /// Used to forcibly disable when there is a receiver and transmitter on the
        /// same power grid, leading to an infinite feedback loop.
        /// </summary>
        public bool ForceDisable { get; set; }

        public int TargetWatts = 200;
        public int ReceiverPriority;

        private float currentPower;
        private float lastFramePower = 0;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Power = parent.GetComp<CompPowerTrader>();
            Flick = parent.GetComp<CompFlickable>();

            if (!respawningAfterLoad)
            {
                SwitchType(PylonType.Receiver);
                Core.Log($"Props: {Props?.buildingName ?? "null"}");
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

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

        public override void PostDeSpawn(Map map)
        {
            // Note: Not really necessary, since this lists in the channel are constantly sanitized.
            // However, it is good practice anyway.
            base.PostDeSpawn(map);
            Channel?.UnRegister(this);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Props.fixedPowerLevel != null)
                TargetWatts = Props.fixedPowerLevel.Value;

            if (Channel == null)
                currentPower = 0;

            if(lastFramePower != currentPower)
                parent.GetComp<CompGlower>()?.UpdateLit(parent.Map);
            
            Power.powerOutputInt = currentPower;
            lastFramePower = currentPower;
        }

        public override string CompInspectStringExtra()
        {
            return SummaryString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
                yield return item;

            yield return new Command_Action()
            {
                action = () =>
                {
                    Find.WindowStack.Add(new ConfigWindow(this));
                },
                defaultLabel = "RF.Pylon.ConfigureLabel".Translate(Props.buildingName),
                defaultDesc = "RF.Pylon.ConfigureDesc".Translate(Props.buildingName),
                icon = Content.SignalIcon,
                defaultIconColor = Channel?.GetColor() ?? Color.grey,
                alsoClickIfOtherInGroupClicked = false
            };
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            DrawConnectionLines();
        }

        public void DrawConnectionLines()
        {
            if (Channel == null || Type == PylonType.None)
                return;

            Vector3 selfPos = parent.DrawPos;
            Map selfMap = parent.Map;

            if (Type == PylonType.Receiver)
            {
                // Draw lines to all the transmitters.
                foreach (var item in Channel.Transmitters)
                {
                    if(selfMap == item.parent.Map)
                        GenDraw.DrawLineBetween(selfPos, item.parent.DrawPos, item.IsActive ? SimpleColor.Cyan : SimpleColor.Red);
                }
            }
            else
            {
                // Draw lines to all the receivers.
                foreach (var item in Channel.Receivers)
                {
                    if(selfMap == item.parent.Map)
                        GenDraw.DrawLineBetween(selfPos, item.parent.DrawPos, item.IsActive ? SimpleColor.Cyan : SimpleColor.Red);
                }
            }
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
            if (newType == PylonType.Transmitter && !Props.canSendPower)
            {
                Core.Error("Tried to set type to transmitter, but Props.canSendPower is false.");
                return;
            }
            if (Type == newType)
                return;

            Type = newType;
            Channel?.Register(this); // This will swap types within the channel.
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
        public override Vector2 InitialSize => new Vector2(850, 200);
        public readonly CompWirelessPower Comp;

        private string wattsBuffer;
        private Window tempWindow;

        public ConfigWindow(CompWirelessPower comp)
        {
            Comp = comp;
            optionalTitle = "RF.Pylon.ConfigureTitle".Translate(Comp.Props.buildingName);
            drawShadow = true;
            draggable = true;
            preventCameraMotion = false;
            resizeable = false;
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = false;
        }

        public override void PreClose()
        {
            base.PreClose();
            tempWindow?.Close();
            tempWindow = null;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Comp == null || Comp.parent.DestroyedOrNull() || !Comp.parent.Spawned || Comp.Type == PylonType.None)
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

            bool isSending = Comp.Type == PylonType.Transmitter;
            string channelName = Comp.Channel?.Name ?? "None";

            bool canChangeMode = Comp.Props.canSendPower;
            string txt = isSending ? "RF.Pylon.Sending".Translate() : "RF.Pylon.Requesting".Translate();
            if (Widgets.ButtonText(new Rect(x, y, 150, h), txt, active: canChangeMode))
            {
                Comp.SwitchType(isSending ? PylonType.Receiver : PylonType.Transmitter);
            }
            x += 150 + p;

            bool canChange = Comp.Props.fixedPowerLevel == null;
            int amount = Comp.TargetWatts;
            GUI.enabled = canChange;
            Widgets.IntEntry(new Rect(x, y, 290, h), ref amount, ref wattsBuffer, 10);
            GUI.enabled = true;
            Comp.TargetWatts = Mathf.Clamp(amount, 0, Comp.Props.maxPower);
            wattsBuffer = Comp.TargetWatts.ToString();
            x += 290 + p;

            txt = "RF.Pylon.On".Translate();
            var size = Text.CalcSize(txt);
            Widgets.Label(new Rect(x, y, size.x, h), txt);

            x += size.x + p;
            if (Widgets.ButtonText(new Rect(x, y, 200, h), channelName))
            {
                var options = new List<FloatMenuOption>();
                if (Comp.Manager != null)
                {
                    foreach (var item in Comp.Manager.GetAvailableChannels(Comp))
                    {
                        var op = new FloatMenuOption(item.Name, () =>
                        {
                            Comp.SwitchToChannel(item);
                        });
                        options.Add(op);
                    }
                }

                if (Comp.Channel != null)
                {
                    options.Add(new FloatMenuOption("RF.Pylon.EmptyChannel".Translate(), () =>
                    {
                        Comp.SwitchToChannel(null);
                    }));
                }

                options.Add(new FloatMenuOption("RF.Pylon.CreateNewChannel".Translate(), () =>
                {
                    if (tempWindow != null)
                        return;

                    Find.WindowStack.Add(tempWindow = new NewChannelWindow() {Manager = Comp.Manager, CreateNewChannel = name =>
                    {
                        Core.Log($"User created new channel '{name}'");
                        int id = Comp.Manager.CreateNewChannel(name);
                        var newChannel = Comp.Manager.TryGetChannel(id);

                        if (newChannel == null)
                            Core.Error("Create new channel, but TryGetChannel returned null!? Why?");

                        Comp.SwitchToChannel(newChannel);
                    }, OnClose = () => {tempWindow = null; }});
                }));
                Find.WindowStack.Add(new FloatMenu(options));
            }

            var channel = Comp.Channel;
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
        public Action OnClose;

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

        public override void PreClose()
        {
            base.PreClose();
            OnClose?.Invoke();
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
