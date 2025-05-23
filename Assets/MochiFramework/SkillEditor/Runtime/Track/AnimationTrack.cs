using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{

    public class AnimationTrack : Track
    {
        public override string TrackName => "动画轨道";
        public override int ClipCount => clips.Count;
        
        public static AnimationTrack CreateAnimationTrack(SkillConfig skillConfig)
        {
            AnimationTrack animationTrack = new AnimationTrack();
            animationTrack.skillConfig = skillConfig;
            return animationTrack;
        }

      
        public override void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,bool isPlaying)
        {
            if(clips is null) return;
             
            AnimationClip currentClip = clips.FirstOrDefault(clip => clip.StartFrame <= currentFrame && clip.EndFrame > currentFrame) as AnimationClip;
            if (currentClip != default && currentClip.AnimationAsset is not null)
            {
                float time = currentTime - currentClip.StartFrame * skillConfig.frameTime;
                if (currentClip.AnimationAsset.isLooping)
                {
                    time %= currentClip.AnimationAsset.length;
                }
                currentClip.AnimationAsset.SampleAnimation(previewObject, time);
                Debug.Log($"预览播放动画时间:{time}");
            }
        }

        public override void PreviewStop(GameObject previewObject)
        {
            
        }
    }
}
