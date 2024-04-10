using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class RitualSparks : VelocityParticle
    {
        #region Static stuff

        [TweakValue("_RimForge", 0, 50)]
        private static float GC = 1.2f, GC2 = 13f;
        private static Dictionary<Color, Material> matCache = new Dictionary<Color, Material>();

        [DebugAction("RimForge", "Debug Sparks Material Cache")]
        private static void DebugMatCache()
        {
            Core.Log($"There are {matCache.Count} materials in the cache.");
        }

        [DebugAction("RimForge", "Clear Sparks Material Cache")]
        private static void ClearMatCache()
        {
            matCache.Clear();
            Core.Log("Cleared the materials cache.");
        }

        public static Material GetMat(Color color, bool unlit = true)
        {
            if (matCache.TryGetValue(color, out var mat))
                return mat;

            mat = MaterialPool.MatFrom("RF/Effects/Spark", unlit ? ShaderDatabase.MoteGlow : ShaderDatabase.Mote, color);
            matCache.Add(color, mat);
            return mat;
        }

        #endregion

        public Color Color = new Color(1f, 170f / 255f, 0, 0.35f);
        public float? FixedLength;
        public float LengthCoefficient = 0.1f;
        public Vector2? GravitateTowards;
        public float DestroyDistance = 0.5f;
        public float LifeTime = 10f;

        private float timeAlive;
        private Vector3 endDraw;
        private Material mat;

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            timeAlive += deltaTime;
            if (timeAlive > LifeTime)
            {
                Destroy();
                return;
            }

            float len = FixedLength ?? (Velocity.magnitude * LengthCoefficient);
            Vector2 tail = Velocity.normalized * len;
            endDraw = DrawPos + new Vector3(tail.x, 0f, tail.y);

            if (GravitateTowards != null)
            {
                Vector2 delta = (GravitateTowards.Value - Position);
                if (delta.sqrMagnitude < 16)
                    delta = delta.normalized * 4f;
                Velocity = Vector2.MoveTowards(Velocity, delta * GC, GC2 * deltaTime);

                float sqrDst = (GravitateTowards.Value - Position).sqrMagnitude;

                if (sqrDst <= DestroyDistance * DestroyDistance)
                {
                    Destroy();
                }
            }
        }

        public override void Draw(Map map)
        {
            if (mat == null)
                mat = GetMat(Color);

            if (endDraw == Vector3.zero)
                return;

            GenDraw.DrawLineBetween(DrawPos, endDraw, mat);
        }
    }
}
