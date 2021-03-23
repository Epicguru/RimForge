using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;

namespace RimForge.Disco.Audio
{
    public class AudioGrain_DiscoLoaded : AudioGrain
    {
        public string filePath;
        public AudioType fileType;

        public override IEnumerable<ResolvedGrain> GetResolvedGrains()
        {
            yield break;
        }

        public async Task<ResolvedGrain> GetResolvedGrainAsync()
        {
            var clip = await AudioLoader.TryLoadAsync(filePath, fileType);
            return new ResolvedGrain_Clip(clip);
        }
    }
}
