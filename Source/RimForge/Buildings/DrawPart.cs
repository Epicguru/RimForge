using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimForge.Buildings
{
    public class DrawPart
    {
        public DrawPart Parent { get; private set; }
        public Vector2 DrawSize => Graphic?.drawSize ?? Vector2.zero;
        public bool OffsetUsingGrandparent = false;
        public float Depth
        {
            get
            {
                if (Parent == null)
                    return DepthOffset;
                return Parent.Depth + 0.001f + DepthOffset;
            }
        }
        public Matrix4x4 FinalMatrix
        {
            get
            {
                if (Parent == null)
                    return Matrix * Matrix4x4.Translate(DrawOffset);

                if(OffsetUsingGrandparent)
                    return Parent.FinalMatrix * Matrix;
                else
                    return Parent.FinalMatrix * Matrix * Matrix4x4.Translate(DrawOffset);
            }
        }
        public DrawPart MatchRotation;

        public Graphic Graphic;

        public Matrix4x4 Matrix;
        public Vector2 DrawOffset;
        public float DepthOffset;

        private readonly List<DrawPart> children = new List<DrawPart>();

        public DrawPart(Graphic graphic, Vector2 drawOffset)
        {
            Matrix = Matrix4x4.identity;
            this.Graphic = graphic;
            this.DrawOffset = drawOffset;
        }

        public bool AddChild(DrawPart child)
        {
            if (child == null || child.Parent != null || Parent == child)
                return false;

            children.Add(child);
            child.Parent = this;
            return true;
        }

        public void SetTR(Vector2 pos, float rot)
        {
            Matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, rot), Vector3.one);
        }

        public bool RemoveChild(DrawPart child)
        {
            if (child == null || child.Parent != this)
                return false;

            child.Parent = null;
            children.Remove(child);
            return true;
        }

        public virtual void Draw(Thing thing)
        {
            if (Graphic != null)
            {
                var final = FinalMatrix;
                Vector2 pos = final.MultiplyPoint3x4(Vector3.zero);
                if (OffsetUsingGrandparent)
                {
                    pos += (Vector2)Parent.Parent.FinalMatrix.MultiplyVector(DrawOffset);
                }
                float rot = MatchRotation == null ? -final.rotation.eulerAngles.z : -MatchRotation.FinalMatrix.rotation.eulerAngles.z;

                Graphic.Draw(pos.FlatToWorld(Depth), Rot4.North, thing, rot);
            }

            foreach (var child in children)
            {
                child.Draw(thing);
            }
        }
    }
}
