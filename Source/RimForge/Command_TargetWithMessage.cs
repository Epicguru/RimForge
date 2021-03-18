using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class Command_TargetWithMessage : Command_Target
    {
        public string Message;
        public MessageTypeDef MessageTypeDef = MessageTypeDefOf.CautionInput;
        public Action<Action> PreProcess;

        public override void ProcessInput(Event ev)
        {
            void DoWork()
            {
                if (string.IsNullOrWhiteSpace(Message))
                    Core.Warn("Command_TargetWithMessage has null or blank message!");
                else
                    Messages.Message(Message, MessageTypeDef);
                base.ProcessInput(ev);
            }

            if (PreProcess == null)
            {
                DoWork();
            }
            else
            {
                PreProcess.Invoke(DoWork);
            }
        }
    }
}
