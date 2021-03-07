using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class RitualSparks : VelocityParticle
    {
        #region Static stuff

        [TweakValue("_RimForge", 0, 50)]
        private static float GC = 1.5f, GC2 = 6f;
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

        private static Material GetMat(Color color)
        {
            if (matCache.TryGetValue(color, out var mat))
                return mat;

            mat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.WorldOverlayCutout, color);
            matCache.Add(color, mat);
            return mat;
        }

        #endregion

        public Color Color = Color.red;
        public float? FixedLength;
        public float LengthCoefficient = 0.1f;
        public Vector2? GravitateTowards;
        public float GravitationalConstant = 0.1f;
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
                Velocity = Vector2.MoveTowards(Velocity, (GravitateTowards.Value - Position) * GC, GC2 * deltaTime);
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
