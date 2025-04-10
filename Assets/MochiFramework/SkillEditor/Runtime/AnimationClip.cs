using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MochiFramework.Skill
{
    // [SkillEditorClip("#436036")]
    public class AnimationClip : Clip
    {
        public override string ClipName => animationAsset ? animationAsset.name : "NoAnimationClip";
        public override int OriginalDuration => Mathf.CeilToInt(animationAsset.length * animationAsset.frameRate);
        public UnityEngine.AnimationClip AnimationAsset => animationAsset;

        [FormerlySerializedAs("clip")] [SerializeField] protected UnityEngine.AnimationClip animationAsset;
        public static AnimationClip CreateAnimationClip(Track track,int startFrame, UnityEngine.AnimationClip unityAnimationClip)
        {
            //TODO 按照SkillConfig中的帧率来计算
            int duration = Mathf.CeilToInt(unityAnimationClip.length * unityAnimationClip.frameRate);
            return CreateAnimationClip(track,startFrame,unityAnimationClip,duration);
        }

        public static AnimationClip CreateAnimationClip(Track track,int startFrame, UnityEngine.AnimationClip unityAnimationClip,int duration)
        {
            AnimationClip clip = ScriptableObject.CreateInstance<AnimationClip>();
            clip.track = track;
            clip.StartFrame = startFrame;
            clip.animationAsset = unityAnimationClip;
            clip.duration = duration;
            clip.name = $"{nameof(AnimationClip)}({clip.ClipName})";
            return clip;
        }
        
        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{StartFrame},动画长度{duration}";
        }
    }
}
