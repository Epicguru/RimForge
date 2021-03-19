using System;
using System.Collections.Generic;
using RimForge.Buildings;
using RimForge.Buildings.DiscoPrograms;
using UnityEngine;
using Verse;

namespace RimForge
{
    public class DiscoSequenceDef : Def
    {
        public int Duration
        {
            get
            {
                if (duration != null)
                    return duration.Value;
                int length = 0;
                foreach (var item in actions)
                {
                    if (item == null || item.Duration <= 0)
                        continue;
                    length += item.Duration;
                }

                return length;
            }
        }

        private int? duration;
        public List<DiscoSequenceAction> actions = new List<DiscoSequenceAction>();
        public Type handlerType = typeof(SequenceHandler);

        public SequenceHandler CreateAndInitHandler(Building_DJStand stand)
        {
            if (handlerType == null)
                return null;

            var instance = Activator.CreateInstance(handlerType, this, stand) as SequenceHandler;
            if (instance == null)
                return null;

            instance.Init();
            return instance;
        }
    }

    public enum DiscoSequenceActionType
    {
        Wait,
        Start,
        Add,
        Repeat,
        Clear,
        WaitForEnd,
        PickRandom
    }

    [Serializable]
    public class DiscoSequenceAction
    {
        private static readonly Color[] randomColors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.cyan,
            Color.magenta,
            Color.yellow
        };

        public int Duration
        {
            get
            {
                if (type == DiscoSequenceActionType.Repeat && actions != null && times > 0)
                {
                    int sum = 0;
                    foreach (var item in actions)
                        sum += item?.Duration ?? 0;
                    
                    return times * sum;
                }

                return selfDuration;
            }
        }

        public Color? Tint => randomTint ? randomColors.RandomElement() : tint;
        public DiscoProgramDef Program => (randomProgramFrom?.Count ?? 0) > 0 ? randomProgramFrom.RandomElement() : program;

        private int selfDuration => type == DiscoSequenceActionType.Wait ? ticks : 0;

        public DiscoSequenceActionType type = DiscoSequenceActionType.Wait;
        public Building_DJStand.BlendMode blend = Building_DJStand.BlendMode.Multiply;
        private int ticks = 30;
        private DiscoProgramDef program;
        private List<DiscoProgramDef> randomProgramFrom;
        public int times = 0;
        public List<DiscoSequenceAction> actions;

        private Color? tint = null;
        private bool randomTint = false;
        public bool oneMinus = false;
        public bool atBottom = false;
        public float weight = 1f;
    }
}
