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
        public abstract void Update(int currentFrame); 
        public abstract void Evaluate(int currentFrame);
        public abstract void Stop();
        public virtual void Dispose(){}
    }
}