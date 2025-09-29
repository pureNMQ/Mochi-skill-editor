using System;
using PlasticGui.Configuration.CloudEdition.Welcome;
using UnityEngine;

namespace MochiFramework.Skill
{
    public abstract class TrackHandler : IDisposable 
    {
        protected int lateFrame = 0;
        protected Track track;
        
        public TrackHandler(Track track)
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