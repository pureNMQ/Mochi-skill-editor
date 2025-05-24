using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{

    public class SkillAnimationSkillTrack : SkillTrack
    {
        public override string TrackName => "动画轨道";
        public override int ClipCount => clips.Count;
        
        public static SkillAnimationSkillTrack CreateAnimationTrack(SkillConfig skillConfig)
        {
            SkillAnimationSkillTrack skillAnimationSkillTrack = new SkillAnimationSkillTrack();
            skillAnimationSkillTrack.skillConfig = skillConfig;
            return skillAnimationSkillTrack;
        }

      
        public override void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,bool isPlaying)
        {
            if(clips is null) return;
             
            SkillAnimationSkillClip currentSkillClip = clips.FirstOrDefault(clip => clip.StartFrame <= currentFrame && clip.EndFrame > currentFrame) as SkillAnimationSkillClip;
            if (currentSkillClip != default && currentSkillClip.AnimationAsset is not null)
            {
                float time = currentTime - currentSkillClip.StartFrame * skillConfig.frameTime;
                if (currentSkillClip.AnimationAsset.isLooping)
                {
                    time %= currentSkillClip.AnimationAsset.length;
                }
                currentSkillClip.AnimationAsset.SampleAnimation(previewObject, time);
                Debug.Log($"预览播放动画时间:{time}");
            }
        }
        
    }
}
