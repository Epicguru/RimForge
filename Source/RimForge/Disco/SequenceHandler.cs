using System.Collections.Generic;
using RimForge.Disco.Programs;
using UnityEngine;
using Verse;

namespace RimForge.Disco
{
    public class SequenceHandler
    {
        public readonly DiscoSequenceDef Def;
        public readonly Building_DJStand Stand;
        public readonly int RoughDuration;
        public bool IsDone { get; protected set; }

        private int ticksToWait;
        private bool waitForLast;
        private Queue<DiscoSequenceAction> actionQueue;
        private DiscoProgram lastAddedProgram;
        private Stack<DiscoProgram> memory = new Stack<DiscoProgram>();

        public SequenceHandler(DiscoSequenceDef def, Building_DJStand stand)
        {
            Def = def;
            Stand = stand;
            RoughDuration = def.Duration;
        }

        public virtual void Init()
        {
            MakeActionQueue();
        }

        protected virtual void MakeActionQueue()
        {
            actionQueue = new Queue<DiscoSequenceAction>(64);
            AddRecursive(Def.actions);
        }

        private void AddRecursive(List<DiscoSequenceAction> list)
        {
            void Handle(DiscoSequenceAction item)
            {
                if (item.type == DiscoSequenceActionType.None)
                    return;

                if (item.chance <= 0f)
                    return;
                if (item.chance < 1f && !Rand.Chance(item.chance))
                    return;

                if (item.type == DiscoSequenceActionType.Repeat)
                {
                    if (item.actions == null || item.times <= 0)
                    {
                        Core.Error("repeat action has null actions or repeats 0 times!");
                        return;
                    }

                    for (int i = 0; i < item.times; i++)
                    {
                        AddRecursive(item.actions);
                    }
                    return;
                }

                if (item.type == DiscoSequenceActionType.PickRandom)
                {
                    if (item.actions == null)
                    {
                        Core.Error("pick random action has null actions or repeats 0 times!");
                        return;
                    }

                    var selected = item.actions.RandomElementByWeight(a => a?.weight ?? 0);
                    Handle(selected);
                    return;
                }

                actionQueue.Enqueue(item);
            }

            foreach (var item in list)
            {
                if (item == null)
                    continue;

                Handle(item);
            }
        }

        public virtual void Tick()
        {
            if (ticksToWait > 0)
            {
                ticksToWait--;
                if(ticksToWait > 0)
                    return;
            }

            if (waitForLast)
            {
                if (lastAddedProgram == null)
                {
                    waitForLast = false;
                }
                else
                {
                    if (!lastAddedProgram.ShouldRemove)
                    {
                        return;
                    }
                    waitForLast = false;
                }
            }

            if (actionQueue.Count == 0)
            {
                IsDone = true;
                lastAddedProgram = null;
                return;
            }

            while (actionQueue.Count > 0)
            {
                var nextAction = actionQueue.Dequeue();
                bool keepGoing = ExecuteAction(nextAction);
                if (!keepGoing)
                    break;
            }

            if (actionQueue.Count == 0 && !waitForLast && ticksToWait <= 0)
                IsDone = true;
        }

        public virtual bool ExecuteAction(DiscoSequenceAction action)
        {
            if (action == null)
                return true;

            bool CheckMem()
            {
                if (memory.Count == 0)
                {
                    Core.Warn("Tried to do a memory action, but there is nothing on the memory stack.");
                    return true;
                }

                if (memory.Peek().ShouldRemove)
                {
                    Core.Warn("Executing a memory action, but the last program on the memory stack has already been removed.");
                    return false;
                }
                return false;
            }

            switch (action.type)
            {
                case DiscoSequenceActionType.Clear:
                    Stand.SetProgramStack(null);
                    return true;
                case DiscoSequenceActionType.Wait:
                    if (action.Duration > 0)
                        ticksToWait = action.Duration;
                    return false;
                case DiscoSequenceActionType.Start:
                    var instance = action.Program?.MakeProgram(Stand);
                    if (instance == null)
                        break;
                    instance.OneMinus = action.oneMinus;
                    instance.OneMinusAlpha = action.oneMinusAlpha;
                    instance.Tint = action.Tint;
                    Stand.SetProgramStack(instance);
                    lastAddedProgram = instance;
                    if (action.addToMemory)
                        memory.Push(instance);
                    return true;
                case DiscoSequenceActionType.Add:
                    instance = action.Program?.MakeProgram(Stand);
                    if (instance == null)
                        break;
                    instance.OneMinus = action.oneMinus;
                    instance.OneMinusAlpha = action.oneMinusAlpha;
                    instance.Tint = action.Tint;
                    Stand.AddProgramStack(instance, action.blend, action.atBottom ? 0 : (int?)null);
                    lastAddedProgram = instance;
                    if (action.addToMemory)
                        memory.Push(instance);
                    return true;
                case DiscoSequenceActionType.WaitForEnd:
                    if (lastAddedProgram == null || lastAddedProgram.ShouldRemove)
                    {
                        Core.Warn("Started WaitForEnd action but there is no previous program, or that program has already ended.");
                        return true;
                    }
                    waitForLast = true;
                    return false;
                case DiscoSequenceActionType.MemAdd:
                    if (lastAddedProgram == null || lastAddedProgram.ShouldRemove)
                    {
                        Core.Warn("Started MemAdd action, but there is no previous program, or that program has already ended.");
                        return true;
                    }
                    memory.Push(lastAddedProgram);
                    return true;
                case DiscoSequenceActionType.MemRemove:
                    if (memory.Count == 0)
                    {
                        Core.Warn("Started MemRemove action, but the memory stack is empty!");
                        return true;
                    }
                    memory.Pop();
                    return true;
                case DiscoSequenceActionType.TintMem:
                    if (CheckMem())
                        return true;
                    Color? tint = action.Tint;
                    memory.Peek().Tint = tint;
                    return true;
                case DiscoSequenceActionType.DestroyMem:
                    if (CheckMem())
                        return true;
                    memory.Peek().Remove();
                    memory.Pop();
                    return true;
            }
            return true;
        }
    }
}
