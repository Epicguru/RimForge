using RimForge.Comps;
using RimForge.Effects;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_RitualCore : Building, IConditionalGlower
    {
        [DebugAction("RimForge", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MakeRed()
        {
            MakeColor(Color.red);
        }

        [DebugAction("RimForge", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void MakeDarkGrey()
        {
            MakeColor(new Color(0.2f, 0.2f, 0.2f ,1f));
        }

        [DebugAction("RimForge", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void TryDestroyHeart()
        {
            foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
            {
                TakeHeart(thing as Pawn);
            }
        }

        [DebugAction("RimForge", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetPlayerPerformedRituals()
        {
            RitualTracker.Current.PlayerPerformedRituals = 0;
            Core.Log("Reset the number of player performed rituals to zero.");
        }

        [DebugAction("RimForge", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void IncrementPlayerPerformedRituals()
        {
            RitualTracker.Current.PlayerPerformedRituals++;
            Core.Log($"Increased the number of player performed rituals to {RitualTracker.Current.PlayerPerformedRituals}");
        }

        private static void TakeHeart(Pawn target)
        {
            if (target == null)
                return;

            BodyPartRecord heart = target.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == BodyPartDefOf.Heart);
            if (heart == null)
            {
                Core.Warn($"{target.LabelCap} does not have a heart!");
                return;
            }
            var info = new DamageInfo(RFDefOf.RF_RitualDamage, 9999, 2, hitPart: heart);
            var result = target.TakeDamage(info);
            Core.Log($"Dealt {result.totalDamageDealt} damage to {target.LabelCap}'s heart.");
        }

        private static void TakeSpineAndArms(Pawn target)
        {
            if (target == null)
                return;

            void DamagePart(BodyPartDef def, float damage = 9999)
            {
                BodyPartRecord heart = target.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == def);
                if (heart == null)
                    return;
                
                var info = new DamageInfo(RFDefOf.RF_RitualDamage, damage, 2, hitPart: heart);
                target.TakeDamage(info);
            }

            DamagePart(BodyPartDefOf.Leg);
            DamagePart(BodyPartDefOf.Leg);
            DamagePart(RFDefOf.Spine);
        }

        private static void MakeColor(Color color)
        {
            foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
            {
                if (thing.TryGetComp<CompColorable>() != null)
                    thing.SetColor(color);
            }
        }

        public static float ChanceToFailRitual(int timesPerformedBefore)
        {
            if (timesPerformedBefore <= 0)
                return 0f;

            float x = timesPerformedBefore;
            float func = Mathf.Clamp01(-1f / (x * 0.85f + 0.5f) + 1); // Go graph it.

            return func * Settings.RitualFailCoefficient;
        }

        private static readonly List<IntVec3> tempCells = new List<IntVec3>(8);
        
        // Ex-tweakValues
        private static float ArcDuration = 1;
        private static float ArcMag = 0.55f;
        private static float SymbolDrawSize = 1.2f;
        private static float SymbolDrawAlpha = 1f;
        private static float SymbolDrawOffset = 6f;
        private static float SymbolDrawBA = 22.5f;

        public const int TOTAL_DURATION = 2500;
        public const int FADE_IN_TICKS = 150;
        public const int FADE_OUT_TICKS = 150;

        public bool IsOnCooldown => CooldownTicksRemaining > 0;
        public bool IsActive => !IsOnCooldown && RitualProgressTicks > 0;

        public bool DrawGuide = true;
        public int CooldownTicksRemaining;
        public int RitualProgressTicks = 0;
        public Pawn TargetPawn;
        public Pawn SacrificePawn;

        private float gearDrawSize = 12.2f, circleDrawSize = 20, textDrawSize = 8;
        private float gearDrawRot, textDrawRot;
        private float gearAlpha = 1, circleAlpha = 1, textAlpha = 1;
        private float gearTurnSpeed = -20f, textTurnSpeed = 9f;
        private float circleBaseAlpha = 0.55f, circleAlphaMag = 0.06f;
        private float circleAlphaFreq = 2, ballOffsetFreq = 0.24f;
        private float ballOffsetBase = 1;
        private float ballOffsetMag = 0.11f, ballDrawSize = 0.72f;
        private float ballOffset;
        private float timer;
        private List<string> missing = new List<string>();
        private int tickCounter = -1;
        private List<(BezierElectricArc arc, float age)> arcs = new List<(BezierElectricArc arc, float age)>();
        private float timeToSparks = 1;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref DrawGuide, "rc_drawGuide", true);
            Scribe_Values.Look(ref CooldownTicksRemaining, "rc_cooldownTicks", 0);
            Scribe_Values.Look(ref RitualProgressTicks, "rc_progressTicks", 0);
            Scribe_References.Look(ref TargetPawn, "rc_targetPawn");
            Scribe_References.Look(ref SacrificePawn, "rc_sacrificePawn");
        }

        public override void Tick()
        {
            base.Tick();

            if (CooldownTicksRemaining > 0)
                CooldownTicksRemaining--;

            if (IsActive)
            {
                if (RitualProgressTicks < TOTAL_DURATION - FADE_OUT_TICKS)
                {
                    if (TargetPawn == null || TargetPawn != GetFirstValidColonist(TargetPawn))
                        EndRitual("RF.Ritual.Cancelled_PawnMissing".Translate());
                    else if(SacrificePawn.DestroyedOrNull() || SacrificePawn.Dead)
                        EndRitual("RF.Ritual.Cancelled_SacrificeMissing".Translate());
                }

                RitualProgressTicks++;
                if (RitualProgressTicks == TOTAL_DURATION - FADE_OUT_TICKS - 1)
                {
                    // Natural end.
                    float chanceToFail = ChanceToFailRitual(RitualTracker.Current.PlayerPerformedRituals);
                    bool fail = Rand.Chance(chanceToFail);
                    bool major = Rand.Chance(Settings.RitualFailMajorChance);
                    string msg = null;
                    if (fail)
                    {
                        string pawnName = TargetPawn.NameShortColored;
                        msg = major ? "RF.Ritual.FailureMessageMajor".Translate(pawnName) : "RF.Ritual.FailureMessageMinor".Translate(pawnName);
                    }
                    EndRitual(msg, fail ? major ? 2 : 1 : 0);
                }
                if (RitualProgressTicks > TOTAL_DURATION)
                    RitualProgressTicks = 0;
            }

            // Update the missing display once per second.
            if (tickCounter == -1)
                tickCounter = Rand.RangeInclusive(0, 60);
            tickCounter++;
            if (tickCounter % 60 == 0)
                UpdateMissing();

            // Tick electrical arcs (causes them to despawn)
            TickArcs();
            if (!IsActive)
                return;

            // Spawn space-distortion mote.
            if (tickCounter % (60 * 5) == 0)
                SpawnDistortion();

            // Turn the ritual gears.
            gearDrawRot += gearTurnSpeed / 60f;
            textDrawRot += textTurnSpeed / 60f;

            timer += 1f / 60f;

            circleAlpha = Mathf.Sin(timer * Mathf.PI * 2f * circleAlphaFreq) * circleAlphaMag + circleBaseAlpha;
            ballOffset = Mathf.Sin((timer + 12) * Mathf.PI * 2f * ballOffsetFreq) * ballOffsetMag + ballOffsetBase;

            bool spawnSparks = false;
            if (timeToSparks >= 0f)
            {
                timeToSparks -= 1f / 60f;
                if (timeToSparks <= 0f)
                {
                    spawnSparks = true;
                    timeToSparks = Rand.Range(2.2f, 3f);
                }
            }

            if (spawnSparks)
            {
                int a = Rand.Range(0, 8);
                int b = a + (Rand.RangeInclusive(1, 2) * (Rand.Chance(0.5f) ? 1 : -1));
                var start = GetPillarPosition(a).ToVector3().WorldToFlat() + new Vector2(0.5f, 1.2f);
                var end = GetPillarPosition(b).ToVector3().WorldToFlat() + new Vector2(0.5f, 1.2f);
                
                for (int i = 0; i < 2; i++)
                {
                    var arc = new BezierElectricArc(25);
                    Vector2 midA = Vector2.Lerp(start, end, 0.3f);
                    Vector2 midB = Vector2.Lerp(start, end, 0.7f);

                    arc.P0 = start;
                    arc.P1 = midA + new Vector2(0, 3);
                    arc.P2 = midB + new Vector2(0, 3);
                    arc.P3 = end;
                    arc.Yellow = true;

                    arc.Spawn(this.Map);
                    arcs.Add((arc, 0));
                }

                for (int i = 0; i < 2; i++)
                {
                    FleckMaker.ThrowLightningGlow(start, Map, 0.8f);
                    FleckMaker.ThrowLightningGlow(end, Map, 0.8f);
                }

                Vector2 gravTowards = TargetPawn?.DrawPos.WorldToFlat() ?? Position.ToVector3Shifted().WorldToFlat();
                for (int i = 0; i < 15; i++)
                {
                    var sparks = new RitualSparks();
                    sparks.Position = start;
                    sparks.GravitateTowards = gravTowards;
                    sparks.Velocity = Rand.InsideUnitCircle.normalized * Rand.Range(0.5f, 6.5f);
                    sparks.Spawn(this.Map);

                    sparks = new RitualSparks();
                    sparks.Position = end;
                    sparks.GravitateTowards = gravTowards;
                    sparks.Velocity = Rand.InsideUnitCircle.normalized * Rand.Range(0.5f, 6.5f);
                    sparks.Spawn(this.Map);
                }
            }
        }

        private void TickArcs()
        {
            for(int i = 0; i < arcs.Count; i++)
            {
                var pair = arcs[i];

                float age = pair.age;
                var arc = pair.arc;

                age += 1f / 60f;
                arcs[i] = (arc, age);
                if(age > ArcDuration)
                {
                    arc.Destroy();
                    arcs.RemoveAt(i);
                    i--;
                    continue;
                }

                float amp = Mathf.Lerp(ArcMag, ArcMag * 0.2f, age / ArcDuration);

                arc.Amplitude = new Vector2(amp * 0.5f, amp);
            }
        }

        public void EndRitual(string error, int failLevel = 0)
        {
            RitualProgressTicks = TOTAL_DURATION - FADE_OUT_TICKS;

            if (error != null)
            {
                Messages.Message(error, MessageTypeDefOf.RejectInput);

                if (failLevel > 0)
                {
                    Core.GenericAchievementEvent(Core.AchievementEvent.RitualFailure);

                    // Kill the sacrifice.
                    if (SacrificePawn != null)
                        TakeHeart(SacrificePawn);

                    if (failLevel == 2)
                    {
                        // Kill the target.
                        if (TargetPawn != null)
                            TakeHeart(TargetPawn);
                    }
                    else
                    {
                        // Cripple the target.
                        if (TargetPawn != null) { }
                            TakeSpineAndArms(TargetPawn);
                    }
                }
            }
            else
            {
                // BSpawn some motes. Flashy.
                for (int i = 0; i < 4; i++)
                {
                    FleckMaker.ThrowLightningGlow(TargetPawn.DrawPos + Rand.InsideUnitCircleVec3 * 0.5f, Map, 2f);
                }

                // Give blessing and thought.
                Trait trait = new Trait(RFDefOf.RF_BlessingOfZir, forced: true);
                TargetPawn.story.traits.GainTrait(trait);
                TargetPawn.TryGiveThought(RFDefOf.RF_RitualBlessed);

                Core.GenericAchievementEvent(Core.AchievementEvent.RitualPerformed);

                // Give negative thoughts to all colonists.
                IEnumerable<Pawn> mapPawns = this.Map.mapPawns.FreeColonistsAndPrisoners;
                IEnumerable<Pawn> worldPawns = Find.WorldPawns.AllPawnsAlive;
                Func<Pawn, bool> keep = p =>
                p.IsColonistPlayerControlled
                || p.IsPrisonerOfColony;
                int thoughtLevel = SacrificePawn.guilt.IsGuilty ? 1 : 0;
                var targets = mapPawns
                    .Concat(worldPawns)
                    .Where(keep)
                    .Distinct();
                foreach (var pawn in targets)
                    pawn.TryGiveThought(RFDefOf.RF_RitualBadThought, thoughtLevel);

                // Kill the sacrifice.
                TakeHeart(SacrificePawn);

                // Increment the number of rituals performed.
                RitualTracker.Current.PlayerPerformedRituals++;

                // Send notification.
                Messages.Message("RF.Ritual.Success".Translate(TargetPawn.NameShortColored), MessageTypeDefOf.PositiveEvent);
                
            }
            TargetPawn = null;
            SacrificePawn = null;
            CooldownTicksRemaining = 2500 * 20; // 20-hour cooldown.
        }

        public IntVec3 GetPillarPosition(int index)
        {
            if (index < 0)
                index += 8000; // Just don't pass in index < 8000 please :)
            index %= 8;
            int i = 0;
            foreach (var item in GetPillarPositions())
            {
                if (i == index)
                    return item;
                i++;
            }

            return default;
        }

        public IEnumerable<IntVec3> GetPillarPositions()
        {
            IntVec3 basePos = Position;

            yield return basePos + new IntVec3(7, 0, 0);
            yield return basePos + new IntVec3(5, 0, 5);
            yield return basePos + new IntVec3(0, 0, 7);
            yield return basePos + new IntVec3(-5, 0, 5);
            yield return basePos + new IntVec3(-7, 0, 0);
            yield return basePos + new IntVec3(-5, 0, -5);
            yield return basePos + new IntVec3(0, 0, -7);
            yield return basePos + new IntVec3(5, 0, -5);
        }

        public bool IsPillarPresent(IntVec3 pos)
        {
            var thing = pos.GetFirstThing(Map, RFDefOf.Column);
            if (thing != null && thing.Stuff == RFDefOf.RF_Copper)
                return true;
            return false;
        }

        public void UpdateMissing()
        {
            missing.Clear();

            int missingPillars = 0;
            foreach (var pos in GetPillarPositions())
            {
                bool isThere = IsPillarPresent(pos);
                if (!isThere)
                    missingPillars++;
            }

            if(missingPillars > 0)
                missing.Add("RF.Ritual.Missing".Translate($"{RFDefOf.RF_Copper.LabelCap} {RFDefOf.Column.label}", missingPillars));
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);

            if (phase != DrawPhase.Draw)
                return;

            if(DrawGuide && !IsActive)
                DrawGhosts();

            if(IsActive)
                DrawRitualEffects();
        }

        private string GetPrettyTimesString(int timesPerformed)
        {
            int timesSanitized = Mathf.Clamp(timesPerformed, 1, 6);
            return $"RF.Ritual.{timesSanitized}".Translate();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            int timesPerformed = RitualTracker.Current.PlayerPerformedRituals;
            float chanceToFail = ChanceToFailRitual(timesPerformed);

            yield return new Command_TargetWithMessage()
            {
                defaultLabel = "RF.Ritual.StartLabel".Translate(),
                defaultDesc = "RF.Ritual.StartDesc".Translate(),
                Message = "RF.Ritual.StartMessage".Translate(),
                action = thing =>
                {
                    Pawn sacrifice = thing.Pawn;

                    if (sacrifice.DestroyedOrNull())
                        return;

                    bool canStart = true;
                    foreach (var reason in GetReasonsCannotStartRitual())
                    {
                        Messages.Message(reason, canStart ? MessageTypeDefOf.RejectInput : MessageTypeDefOf.SilentInput);
                        canStart = false;
                    }

                    if (!canStart)
                        return;

                    TargetPawn = GetFirstValidColonist();
                    SacrificePawn = sacrifice;
                    RitualProgressTicks = 1;
                },
                targetingParams = new TargetingParameters()
                {
                    canTargetHumans = true,
                    canTargetPawns = true,
                    canTargetBuildings = false,
                    canTargetSelf = false,
                    canTargetAnimals = false,
                    canTargetMechs = false,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    validator = info =>
                    {
                        if (!info.HasThing)
                            return false;
                        return info.Thing is Pawn {IsPrisoner: true, Dead: false};
                    }
                },
                icon = Content.RitualStartIcon,
                defaultIconColor = new Color(0.8f, 0.5f, 0.5f, 1f),
                PreProcess = chanceToFail <= 0f ? (Action<Action>)null : act =>
                {
                    Find.WindowStack.Add(new WarningDialog()
                    {
                        Text = "RF.Ritual.FailWarning".Translate(GetPrettyTimesString(timesPerformed), (chanceToFail * 100f).ToString("F0")),
                        ButtonText = "RF.Ritual.IUnderstand".Translate(),
                        OnAccept = () =>
                        {
                            if (chanceToFail > 0.5f)
                                Core.GenericAchievementEvent(Core.AchievementEvent.Ritual50ChanceFailure);
                            act();
                        }
                    });
                },
                disabled = GetReasonsCannotStartRitual().Any(),
                disabledReason = GetReasonsCannotStartRitual().FirstOrDefault()
            };

            string label = DrawGuide ? "RF.Ritual.DrawGuideHideLabel".Translate() : "RF.Ritual.DrawGuideShowLabel".Translate();
            string desc  = DrawGuide ? "RF.Ritual.DrawGuideHideDesc".Translate()  : "RF.Ritual.DrawGuideShowDesc".Translate();

            yield return new Command_Action()
            {
                icon = Content.BuildBlueprintIcon,
                defaultLabel = label,
                defaultDesc = desc,
                action = () =>
                {
                    DrawGuide = !DrawGuide;
                }
            };

            var allowedDesignator = BuildCopyCommandUtility.BuildCommand(RFDefOf.Column, RFDefOf.RF_Copper, null, null, false, "RF.Ritual.BuildColumnLabel".Translate(), "RF.Ritual.BuildColumnDesc".Translate(), true);

            if (allowedDesignator != null)
                yield return allowedDesignator;

            if (!Prefs.DevMode)
                yield break;

            yield return new Command_Action()
            {
                defaultLabel = "spawn distort mote",
                action = SpawnDistortion
            };
            yield return new Command_Action()
            {
                defaultLabel = "fix",
                action = () =>
                {
                    CooldownTicksRemaining = 0;
                    RitualProgressTicks = 0;
                    TargetPawn = null;
                }
            };
            yield return new Command_Action()
            {
                defaultLabel = "finish cooldown",
                action = () =>
                {
                    CooldownTicksRemaining = 0;
                }
            };
        }

        public void SpawnDistortion()
        {
            Map map = base.Map;

            FleckCreationData dataStatic = FleckMaker.GetDataStatic(DrawPos, map, RFDefOf.RF_Motes_RitualDistort, 1f);
            dataStatic.rotationRate = 0f;
            dataStatic.velocityAngle = 0f;
            dataStatic.velocitySpeed = 0f;
            map.flecks.CreateFleck(dataStatic);
        }

        private void DrawGhosts()
        {
            var map = this.Map;
            if (map == null)
                return;

            bool ShouldDrawAt(IntVec3 pos)
            {
                if (IsPillarPresent(pos))
                    return false;

                int index = map.cellIndices.CellToIndex(pos);
                var bps = map.blueprintGrid.InnerArray[index];
                if (bps != null)
                {
                    foreach (var bp in bps)
                    {
                        if (bp == null)
                            continue;
                        
                        if (bp.def.entityDefToBuild == RFDefOf.Column)
                            return false;
                    }
                }

                int time = (int) (Time.realtimeSinceStartup * 2);
                return time % 2 == 0;
            }

            Color color = Color.Lerp(Color.red, Color.yellow, 0.5f);
            foreach (var pos in GetPillarPositions())
            {
                if (ShouldDrawAt(pos))
                    DrawGhost(pos, color);
            }
        }

        private void DrawGhost(IntVec3 at, Color color)
        {
            ThingDef blueprintDef = RFDefOf.Column.blueprintDef;
            GraphicDatabase.Get(blueprintDef.graphic.GetType(), blueprintDef.graphic.path, blueprintDef.graphic.Shader, blueprintDef.graphic.drawSize, color, Color.white, blueprintDef.graphicData, null).DrawFromDef(at.ToVector3ShiftedWithAltitude(AltitudeLayer.Blueprint), Rot4.North, RFDefOf.Column.blueprintDef);
        }

        private void DrawRitualEffects()
        {
            if (Content.RitualCircle == null)
                Content.LoadRitualGraphics(this);

            float a = 1f;
            if (RitualProgressTicks < FADE_IN_TICKS)
                a = (float)RitualProgressTicks / FADE_IN_TICKS;
            if (RitualProgressTicks >= TOTAL_DURATION - FADE_OUT_TICKS)
                a = 1f - (float) (RitualProgressTicks - (TOTAL_DURATION - FADE_OUT_TICKS)) / FADE_OUT_TICKS;

            Vector3 drawPos = DrawPos + new Vector3(0, 0, -0.5f); // Because of the draw size of the ritual core.
            drawPos.y = AltitudeLayer.DoorMoveable.AltitudeFor();

            Content.RitualGear.drawSize = new Vector2(gearDrawSize, gearDrawSize);
            Content.RitualGear.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, gearAlpha * a);
            Content.RitualGear.Draw(drawPos, Rot4.North, this, gearDrawRot);

            for (int i = 0; i < 8; i++)
            {
                var symbol = i % 2 == 0 ? Content.RitualSymbolA : Content.RitualSymbolB;
                symbol.drawSize = new Vector2(SymbolDrawSize, SymbolDrawSize);
                symbol.MatNorth.color = new Color(0.9f, 0.35f, 0.15f, SymbolDrawAlpha * a);
                float angle = (SymbolDrawBA + i * (360f / 8f) - gearDrawRot) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * SymbolDrawOffset;
                symbol.Draw(drawPos + offset, Rot4.North, this);
            }
            

            Content.RitualCircleText.drawSize = new Vector2(textDrawSize, textDrawSize);
            Content.RitualCircleText.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, textAlpha * a);
            Content.RitualCircleText.Draw(drawPos, Rot4.North, this, textDrawRot);

            Content.RitualCircle.drawSize = new Vector2(circleDrawSize, circleDrawSize);
            Content.RitualCircle.MatNorth.color = new Color(0.9f, 0.15f, 0.15f, circleAlpha * a);
            Content.RitualCircle.Draw(drawPos, Rot4.North, this);

            drawPos.y = AltitudeLayer.VisEffects.AltitudeFor();
            Content.RitualBall.drawSize = new Vector2(ballDrawSize, ballDrawSize);
            Content.RitualBall.MatNorth.color = new Color(1f, 145f / 255f, 0f, 1f);
            Content.RitualBall.Draw(drawPos + new Vector3(0f, 0f, ballOffset), Rot4.North, this);
        }

        public bool ShouldGlowNow()
        {
            return IsActive;
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            if (IsActive)
                return;

            tempCells.Clear();
            tempCells.AddRange(GetColonistStandCells());
            GenDraw.DrawFieldEdges(tempCells, Color.green);
        }

        private IEnumerable<IntVec3> GetColonistStandCells()
        {
            var selfPos = Position;
            for (int x = selfPos.x - 1; x <= selfPos.x + 1; x++)
            {
                for (int z = selfPos.z - 1; z <= selfPos.z + 1; z++)
                {
                    IntVec3 cell = new IntVec3(x, selfPos.y, z);
                    if (cell != selfPos)
                        yield return cell;
                }
            }
        }

        public Pawn GetFirstValidColonist(Pawn prefer = null)
        {
            var thingGrid = Map.thingGrid;
            foreach (var cell in GetColonistStandCells())
            {
                var things = thingGrid.ThingsListAt(cell);
                if (things == null)
                    continue;

                foreach (var thing in things)
                {
                    if (!(thing is Pawn pawn))
                        continue;

                    if (!pawn.IsColonistPlayerControlled || pawn.IsPrisoner)
                        continue;
                    if (pawn.Dead || pawn.Downed)
                        continue;
                    if (!pawn.health.capacities.CanBeAwake)
                        continue;
                    if (pawn.story?.traits?.HasTrait(RFDefOf.RF_BlessingOfZir) ?? false)
                        continue;
                    if (pawn.story?.traits?.HasTrait(RFDefOf.RF_ZirsCorruption) ?? false)
                        continue;

                    if (prefer != null && pawn != prefer)
                        continue;

                    return pawn;
                }
            }
            return null;
        }

        public bool IsValidTimePeriod()
        {
            if (!Settings.RitualMustBeAtNight)
                return true;

            var map = Map;
            int hour = GenLocalDate.HourOfDay(map);
            
            return hour >= 21 || hour < 3;
        }

        public IEnumerable<string> GetReasonsCannotStartRitual()
        {
            if (IsActive)
                yield return "RF.Ritual.CannotStart_AlreadyActive".Translate();

            if (IsOnCooldown)
            {
                float hoursRemaining = CooldownTicksRemaining / 2500f;
                yield return "RF.Ritual.CannotStart_OnCooldown".Translate(hoursRemaining.ToString("F1"));
            }

            UpdateMissing();
            foreach (var reason in missing)
                yield return reason;

            if (!IsValidTimePeriod())
                yield return "RF.Ritual.CannotStart_NotNight".Translate();

            if (GetFirstValidColonist() == null)
                yield return "RF.Ritual.CannotStart_NoColonist".Translate();
        }

        public override string GetInspectString()
        {
            return IsActive ? "RF.Ritual.InProgress".Translate() : "RF.Ritual.QuickGuide".Translate();
        }
    }

    public class WarningDialog : Window
    {
        public override Vector2 InitialSize => new Vector2(500f, 300f);

        public string Text;
        public string ButtonText;
        public Action OnAccept;

        public WarningDialog()
        {
            forcePause = true;
            draggable = true;
            doCloseButton = false;
            doCloseX = false;
            closeOnClickedOutside = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Verse.Text.Font = GameFont.Small;
            Rect textRect = inRect;
            textRect.height -= 150;
            Widgets.Label(textRect, Text);

            GUI.color = new Color(0.8f, 0.5f, 0.5f, 1f);
            Rect buttonA = new Rect(inRect.x, inRect.yMax - 32, 190, 32);
            if(Widgets.ButtonText(buttonA, ButtonText))
            {
                OnAccept?.Invoke();
                Close();
            }
            GUI.color = Color.white;

            Rect buttonB = new Rect(inRect.xMax - 190, inRect.yMax - 32, 190, 32);
            if (Widgets.ButtonText(buttonB, "RF.Cancel".Translate().CapitalizeFirst()))
            {
                Close();
            }
        }
    }
}
