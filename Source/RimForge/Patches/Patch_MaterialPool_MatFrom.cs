using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RimForge.Patches
{
    [HarmonyPatch(typeof(MaterialPool), "MatFrom", new Type[] {typeof(MaterialRequest) })]
    static class Patch_MaterialPool_MatFrom
    {
        public static bool Active = false;

        static bool Prefix(MaterialRequest req)
        {
            if (!Active)
                return true;

            req.mainTex.wrapMode = TextureWrapMode.Clamp;
            req.mainTex.filterMode = FilterMode.Point;
            req.mainTex.requestedMipmapLevel = 0;
            return true;
        }
    }
}
