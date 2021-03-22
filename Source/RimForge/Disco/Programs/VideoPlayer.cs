using System;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VideoTool;

namespace RimForge.Disco.Programs
{
    public class VideoPlayer : DiscoProgram
    {
        public Color WhiteColor, BlackColor;

        public string FilePath { get; private set; } = @"C:\Users\The Superior One\Desktop\BadApple.bwcv";
        public int VideoWidth => video?.Width ?? -1;
        public int VideoHeight => video?.Height ?? -1;
        public int VideoFrameRate => video?.FrameRate ?? -1;

        private VideoLoader video;
        private int frameSwapInterval;
        private int tickCounterCustom;
        private CellRect vidBounds;

        public VideoPlayer(DiscoProgramDef def) : base(def)
        {
        }

        public override void Init()
        {
            WhiteColor = Def.Get("whiteColor", Color.white);
            WhiteColor = Def.Get("blackColor", new Color(0, 0, 0, 0));

            FilePath = Def.Get<string>("filePath");

            // Load video on another thread.
            Task.Run(() =>
            {
                try
                {
                    VideoLoader vid = new VideoLoader();
                    vid.Load(FilePath);
                    if (!vid.LoadNextFrame())
                        throw new Exception("Failed to load first frame");
                    this.video = vid;
                }
                catch(Exception e)
                {
                    Core.Error($"Exception loading video program from '{FilePath}'", e);
                    this.Remove();
                }
            });
        }

        public override void Tick()
        {
            base.Tick();

            if (video == null)
                return;

            if (frameSwapInterval <= 0)
            {
                frameSwapInterval = 60 / VideoFrameRate;

                int gw = DJStand.FloorBounds.Width;
                int gh = DJStand.FloorBounds.Height;
                int offX = Mathf.RoundToInt((gw - VideoWidth) / 2f);
                int offZ = Mathf.RoundToInt((gh - VideoHeight) / 2f);
                int minX = DJStand.FloorBounds.minX + offX;
                int minZ = DJStand.FloorBounds.minZ + offZ;
                vidBounds = new CellRect(minX, minZ, VideoWidth, VideoHeight);
            }

            tickCounterCustom++;
            if (tickCounterCustom % frameSwapInterval == 0)
            {
                bool hasNextFrame = video.LoadNextFrame();
                if (!hasNextFrame)
                {
                    Remove();
                    video.Dispose();
                    video = null;
                }
            }
        }

        protected virtual int CellToVidIndex(IntVec3 cell)
        {
            if (!vidBounds.Contains(cell))
                return -1;

            int localX = cell.x - vidBounds.minX;
            int localZ = cell.z - vidBounds.minZ;

            return localX + localZ * VideoWidth;
        }

        public override Color ColorFor(IntVec3 cell)
        {
            if (video == null)
                return default;

            int index = CellToVidIndex(cell);
            if (index == -1)
                return default;

            return video.CurrentFrame[index] ? WhiteColor : BlackColor;
        }
    }
}
