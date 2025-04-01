using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class AnimationClip : Clip
    {
        public override string ClipName => clip ? clip.name : "NoAnimationClip";
        protected UnityEngine.AnimationClip clip;
        public AnimationClip(int startFrame, UnityEngine.AnimationClip clip) : base(startFrame)
        {
            this.clip = clip;
            duration = Mathf.CeilToInt(clip.length * clip.frameRate);
        }

        public AnimationClip(int startFrame, UnityEngine.AnimationClip clip, int duration) : this(startFrame,clip)
        {
            this.duration = duration;
        }


        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{startFrame},动画长度{duration}";
        }
    }
}
