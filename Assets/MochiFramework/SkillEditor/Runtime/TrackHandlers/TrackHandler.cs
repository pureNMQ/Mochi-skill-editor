using System;
using UnityEngine;

namespace MochiFramework.Skill
{
    public abstract class TrackHandler : IDisposable 
    {
        //上次播放的帧数,在卡顿的情况，方便追帧
        protected int lateFrame = 0;
        protected float lateTime = 0;
        protected Track track;
        
        public TrackHandler(Track track)
        {
            this.track = track;
        }

        public abstract void Play(int currentFrame = 0, float currentTime = 0);
        public abstract void Update(float deltaTime, int currentFrame, float currentTime);
        public abstract void Evaluate(int currentFrame, float currentTime);
        public abstract void Stop();
        

        public virtual void Dispose()
        {
        }
    }
}