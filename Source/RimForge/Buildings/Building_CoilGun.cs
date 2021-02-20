using System.Collections.Generic;
using RimForge.Effects;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class Building_Coilgun : Building
    {
        [TweakValue("_TEMP", -8, 5)]
        private static float OffA, OffB, OffD, OffE;
        [TweakValue("_TEMP", -180, 180)]
        private static float OffC, TurretRot;

        public CoilgunDef Def => def as CoilgunDef;

        [TweakValue("_TEMP", 0, 1f)]
        public static float ArmLerp = 0f;
        public DrawPart Top, Cables, LeftPivot, RightPivot;
        public DrawPart BarLeft, BarRight;

        private List<LinearElectricArc> backArcs = new List<LinearElectricArc>();
        private List<LinearElectricArc> frontArcs = new List<LinearElectricArc>();

        public virtual void Setup()
        {
            if (Content.CoilgunTop == null)
                Content.LoadBuildingGraphics(this);

            Top = new DrawPart(Content.CoilgunTop, new Vector2(2.2f, 0f));
            Cables = new DrawPart(Content.CoilgunCables, new Vector2(2.2f, 0f));
            Top.DepthOffset = AltitudeLayer.PawnUnused.AltitudeFor() + 0.001f; // Barrel of gun draws over pawns.
            Cables.DepthOffset = Top.DepthOffset + 0.05f;

            LeftPivot = new DrawPart(Content.CoilgunLinkLeft, new Vector2(1f, 0f));
            RightPivot = new DrawPart(Content.CoilgunLinkRight, new Vector2(1f, 0f));

            BarLeft = new DrawPart(Content.CoilgunBarLeft, new Vector2(0f, 0f));
            BarRight = new DrawPart(Content.CoilgunBarRight, new Vector2(0f, 0f));

            BarLeft.OffsetUsingGrandparent = true;
            BarRight.OffsetUsingGrandparent = true;

            Top.AddChild(LeftPivot);
            Top.AddChild(RightPivot);

            LeftPivot.AddChild(BarLeft);
            RightPivot.AddChild(BarRight);
            BarLeft.MatchRotation = Top;
            BarRight.MatchRotation = Top;
        }

        public override void Tick()
        {
            base.Tick();

            TickBackArcs();
            TickFrontArcs();
        }

        private void TickBackArcs()
        {
            bool wantsBackArcs = ArmLerp >= 0.5f;
            if (!wantsBackArcs)
            {
                foreach (var arc in backArcs)
                {
                    arc.Destroy();
                }
                backArcs.Clear();
                return;
            }

            const int TO_SPAWN = 10;
            while (backArcs.Count < TO_SPAWN)
            {
                var newArc = new LinearElectricArc(20);
                newArc.Amplitude = Vector2.Lerp(new Vector2(0.01f, 0.1f), new Vector2(0.1f, 0.2f), (float)backArcs.Count / TO_SPAWN);
                newArc.Spawn(this.Map);
                backArcs.Add(newArc);
            }

            float alt = AltitudeLayer.VisEffects.AltitudeFor();
            foreach (var arc in backArcs)
            {
                float xa = Rand.Range(-5f, -3.13f);
                float xb = Rand.Range(-5f, -3.13f);
                float addA = (1f - Mathf.Abs(0.5f - Mathf.InverseLerp(-5f, -3.13f, xa)) * 2f) * 0.1f;
                float addB = (1f - Mathf.Abs(0.5f - Mathf.InverseLerp(-5f, -3.13f, xb)) * 2f) * 0.1f;
                var a = (Vector2) Top.FinalMatrix.MultiplyPoint3x4(new Vector2(xa, 2.23f + addA));
                var b = (Vector2) Top.FinalMatrix.MultiplyPoint3x4(new Vector2(xb, -2.23f - addB));

                arc.Start = a;
                arc.End = b;
            }
        }

        private void TickFrontArcs()
        {
            bool wantsFrontArcs = ArmLerp >= 0.5f;
            if (!wantsFrontArcs)
            {
                foreach (var arc in frontArcs)
                {
                    arc.Destroy();
                }
                frontArcs.Clear();
                return;
            }

            const int TO_SPAWN = 25;
            while (frontArcs.Count < TO_SPAWN)
            {
                var newArc = new LinearElectricArc(4);
                newArc.Amplitude = Vector2.Lerp(new Vector2(0.01f, 0.1f), new Vector2(0.1f, 0.2f), (float)frontArcs.Count / TO_SPAWN);
                newArc.Spawn(this.Map);
                frontArcs.Add(newArc);
            }

            float alt = AltitudeLayer.VisEffects.AltitudeFor();
            foreach (var arc in frontArcs)
            {
                float h = Rand.Range(-0.94f, 3.843f);
                var a = (Vector2)Top.FinalMatrix.MultiplyPoint3x4(new Vector2(h, 0.489f));
                var b = (Vector2)Top.FinalMatrix.MultiplyPoint3x4(new Vector2(h, -0.489f));

                arc.Start = a;
                arc.End = b;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (Top == null)
                Setup();

            ArmLerp = Mathf.Clamp01(ArmLerp);
            float realLerp = 0.625f * ArmLerp; // 1.0 is full extended but I think 0.625 looks better.
            float armAngle = Mathf.Lerp(0f, 145f, realLerp);

            Top.SetTR(DrawPos.WorldToFlat(), TurretRot);
            Top.DrawOffset = new Vector2(2.2f - OffC, 0f);
            Cables.SetTR(DrawPos.WorldToFlat(), TurretRot);
            Cables.DrawOffset = new Vector2(0.128f - OffC, 0f);

            LeftPivot.SetTR(new Vector2(-3.376f, 0.8038f), armAngle);
            LeftPivot.DrawOffset = new Vector2(0.7074f, 0.1286f);

            RightPivot.SetTR(new Vector2(-3.376f, -0.8038f), -armAngle);
            RightPivot.DrawOffset = new Vector2(0.7074f, -0.1286f);

            BarLeft.SetTR(new Vector2(0.7717f, -0.3536f), 0f);
            BarLeft.DrawOffset  = Vector2.Lerp(new Vector2(-0.772f, 0.322f), new Vector2(-0.9f, -0.03f), realLerp);
            
            BarRight.SetTR(new Vector2(0.7717f, 0.3536f), 0f);
            BarRight.DrawOffset = Vector2.Lerp(new Vector2(-0.772f, -0.322f), new Vector2(-0.9f, 0.03f), realLerp);

            Top.Draw(this);
            Cables.Draw(this);
        }
    }
}
