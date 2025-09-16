using System;
using UnityEngine;

namespace MochiFramework.Skill
{
    // [SkillEditorClip("#436036")]
    [Serializable]
    public class AnimationClip : Clip
    {
        public override string ClipName => animationAsset ? animationAsset.name : "NoAnimationClip";
        public override int OriginalDuration => Mathf.CeilToInt(animationAsset.length * animationAsset.frameRate);

        public UnityEngine.AnimationClip AnimationAsset
        {
            get => animationAsset;
            set => animationAsset = value;
        }

        [SerializeField] protected UnityEngine.AnimationClip animationAsset;

        public static AnimationClip CreateAnimationClip(Track track,int startFrame, UnityEngine.AnimationClip unityAnimationClip,int duration)
        {
            AnimationClip clip = new AnimationClip();
            clip.track = track;
            clip.startFrame = startFrame;
            clip.animationAsset = unityAnimationClip;
            clip.duration = duration;
            return clip;
        }
        
        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{startFrame},动画长度{duration}";
        }
    }
}
