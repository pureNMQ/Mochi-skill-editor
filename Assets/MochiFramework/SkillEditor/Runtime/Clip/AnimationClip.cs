using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill
{
    // [SkillEditorClip("#436036")]
    [Serializable]
    public class AnimationClip : Clip
    {
        public override string ClipName => animationAsset ? animationAsset.name : "NoAnimationClip";
        public override int OriginalDuration => Mathf.CeilToInt(animationAsset.length * animationAsset.frameRate);
        public override Object UnityClip
        {
            get => animationAsset;
            set => animationAsset = (UnityEngine.AnimationClip)value;
        }

        public UnityEngine.AnimationClip AnimationAsset
        {
            get => animationAsset;
            set => animationAsset = value;
        }

        [SerializeField] protected UnityEngine.AnimationClip animationAsset;
        public static AnimationClip CreateAnimationClip(Track track,int startFrame, UnityEngine.AnimationClip unityAnimationClip)
        {
            //TODO 按照SkillConfig中的帧率来计算
            int duration = Mathf.CeilToInt(unityAnimationClip.length * unityAnimationClip.frameRate);
            return CreateAnimationClip(track,startFrame,unityAnimationClip,duration);
        }

        public static AnimationClip CreateAnimationClip(Track track,int startFrame, UnityEngine.AnimationClip unityAnimationClip,int duration)
        {
            AnimationClip clip = new AnimationClip();
            clip.track = track;
            clip.StartFrame = startFrame;
            clip.animationAsset = unityAnimationClip;
            clip.duration = duration;
            return clip;
        }
        
        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{StartFrame},动画长度{duration}";
        }
    }
}
