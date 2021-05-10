using UnityEngine;
using Verse;

namespace RimForge.Effects
{
    public class BombShadowEffect : MapEffect
    {
        private static MaterialPropertyBlock block;

        private int duration;
        private Vector3 pos;
        private int progress;
        private Matrix4x4 trs;
        private Matrix4x4 bombTrs;
        private Color color;

        public BombShadowEffect(Vector3 pos, int duration)
        {
            this.duration = duration;
            this.pos = pos;
        }

        public override void TickAccurate()
        {
            base.TickAccurate();

            float p = 0f;
            float p2 = 0f;
            const int DROP_TICKS = 30;
            const int DROP_TICKS_2 = 90;
            int left = duration - progress;
            if (left <= DROP_TICKS)
                p = 1f - Mathf.Clamp01((float)left / DROP_TICKS);
            if (left <= DROP_TICKS_2)
                p2 = 1f - Mathf.Clamp01((float)left / DROP_TICKS_2);

            float s = Mathf.Lerp(2.5f, 0.3f, p);

            trs = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(s, 1, s));
            bombTrs = Matrix4x4.TRS(pos + (1f - p2) * new Vector3(0, 0, 250), Quaternion.identity, new Vector3(2f, 1f, 2f));

            color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), p);

            progress++;
            if (progress > duration)
                Destroy();
        }

        public override void Draw(bool tick, Map map)
        {
            block ??= new MaterialPropertyBlock();
            block.SetColor("_Color", color);

            var graphic = Content.BombShadowGraphic;
            var graphic2 = Content.FallingBombGraphic;

            Graphics.DrawMesh(MeshPool.plane10, trs, graphic.MatSingle, 0, null, 0, block);
            Graphics.DrawMesh(MeshPool.plane10, bombTrs, graphic2.MatSingle, 0);

            Vector3 mapTop = pos;
            mapTop.z = map.Size.z + 1;

            GenDraw.DrawLineBetween(mapTop, pos, SimpleColor.Red);
            GenDraw.DrawLineBetween(mapTop, pos, SimpleColor.Red);
            GenDraw.DrawLineBetween(mapTop, pos, SimpleColor.Red);
        }
    }
}
