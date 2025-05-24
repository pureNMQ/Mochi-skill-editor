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
    public class SkillAnimationSkillClip : SkillClip
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
        public static SkillAnimationSkillClip CreateAnimationClip(SkillTrack skillTrack,int startFrame, UnityEngine.AnimationClip unityAnimationClip)
        {
            //TODO 按照SkillConfig中的帧率来计算
            int duration = Mathf.CeilToInt(unityAnimationClip.length * unityAnimationClip.frameRate);
            return CreateAnimationClip(skillTrack,startFrame,unityAnimationClip,duration);
        }

        public static SkillAnimationSkillClip CreateAnimationClip(SkillTrack skillTrack,int startFrame, UnityEngine.AnimationClip unityAnimationClip,int duration)
        {
            SkillAnimationSkillClip skillClip = new SkillAnimationSkillClip();
           // skillClip.skillTrack = skillTrack;
            skillClip.StartFrame = startFrame;
            skillClip.animationAsset = unityAnimationClip;
            skillClip.duration = duration;
            return skillClip;
        }
        
        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{StartFrame},动画长度{duration}";
        }
    }
}
