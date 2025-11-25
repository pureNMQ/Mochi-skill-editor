using System;
using UnityEngine;

namespace MochiFramework.Skill
{
    public abstract class TrackHandler : IDisposable
    {
        protected int lateFrame = 0;
        protected ITrack track;

        public TrackHandler(ITrack track)
        {
            this.track = track;
        }

        public abstract void Play(int currentFrame = 0);
        /// <summary>
        /// 运行模式下，根据当前帧更新轨道上的状态
        /// </summary>
        /// <param name="currentFrame"></param>
        public abstract void Update(int currentFrame);
        /// <summary>
        /// 预览模式下，根据当前帧计算轨道上的状态
        /// </summary>
        /// <param name="currentFrame">当前帧</param>
        public abstract void Evaluate(int currentFrame);
        public abstract void Stop();
        public virtual void Dispose() { }
    }
}