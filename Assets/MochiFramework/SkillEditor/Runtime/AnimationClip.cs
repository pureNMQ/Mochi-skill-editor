using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    // [SkillEditorClip("#436036")]
    public class AnimationClip : Clip
    {
        public override string ClipName => clip ? clip.name : "NoAnimationClip";
        [SerializeField] protected UnityEngine.AnimationClip clip;
        public AnimationClip(int startFrame, UnityEngine.AnimationClip clip)
        {
            this.clip = clip;
            duration = Mathf.CeilToInt(clip.length * clip.frameRate);
        }

        public AnimationClip(int startFrame, UnityEngine.AnimationClip clip, int duration)
        {
            this.duration = duration;
        }

        public static AnimationClip CreateAnimationClip(int startFrame, UnityEngine.AnimationClip unityAnimationClip)
        {
            //TODO 按照SkillConfig中的帧率来计算
            int duration = Mathf.CeilToInt(unityAnimationClip.length * unityAnimationClip.frameRate);
            return CreateAnimationClip(startFrame,unityAnimationClip,duration);
        }

        public static AnimationClip CreateAnimationClip(int startFrame, UnityEngine.AnimationClip unityAnimationClip,int duration)
        {
            AnimationClip clip = ScriptableObject.CreateInstance<AnimationClip>();
            clip.startFrame = startFrame;
            clip.clip = unityAnimationClip;
            clip.duration = duration;
            clip.name = clip.ClipName;
            return clip;
        }


        public override string ToString()
        {
            return $"动画名{ClipName},动画起始帧{startFrame},动画长度{duration}";
        }
    }
}
