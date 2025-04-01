using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    public abstract class Clip
    {
        public abstract string ClipName { get; }
        public int StartFrame { get => startFrame; set => startFrame = value; }
        public int EndFrame {get => startFrame + duration;}
        public int Duration => duration;

        protected int startFrame;
        protected int duration;

        public Clip(int startFrame)
        {
            this.startFrame = startFrame;
        }
    }
}
