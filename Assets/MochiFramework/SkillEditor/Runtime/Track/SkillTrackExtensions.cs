using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    public static class SkillTrackExtensions
    {
        //判断是否能转换
        //后续重构
        public static bool  CanConvertToClip<T>(this Skill.SkillTrack skillTrack,object obj)// where T:Clip
        {
            return obj is T;
        }
        
        
        //判断是否可以插入进去
        public static bool CanInsertClipAtFrame(this Skill.SkillTrack skillTrack,int startFrame,int duration, out int correctionDuration,SkillClip ignoreSkillClip = null)
        {
            var clips = skillTrack.clips;
            var skillConfig = skillTrack.SkillConfig;
            
            correctionDuration = duration;
            foreach (var item in clips)
            {
                if(item == ignoreSkillClip) continue;
                
                //不允许插入到另一个Clip中间
                //情况一:插入Clip的起始点位于另一个Clip中
                if (startFrame >= item.StartFrame && startFrame < item.EndFrame)
                {
                    Debug.Log("不可插入到其他Clip中");
                    correctionDuration = 0;
                    return false;
                }
                //情况二:插入Clip的结束点位于另一个Clip中
                if (startFrame < item.StartFrame && startFrame + duration >= item.StartFrame)
                {
                    int offset = item.StartFrame - startFrame;
                    if (offset < correctionDuration)
                    {
                        correctionDuration = offset;
                    }
                }
            }
            
            //情况三:插入Clip的结束点位于Track长度之外
            if (startFrame + duration > skillConfig.FrameCount)
            {
                int offset = skillConfig.FrameCount - startFrame;
                if (offset < correctionDuration)
                {
                    correctionDuration = offset;
                }
            }
            
            return true;
        }
        
        
        
        //是否可以恢复原长度
        public static void ResetClipDuration(this Skill.SkillTrack skillTrack,SkillClip skillClip)
        {
            //TODO 重置Clip的长度为原始长度 如果可以的话
            if (skillTrack.clips.Contains(skillClip))
            {
                if (CanInsertClipAtFrame(skillTrack,skillClip.StartFrame, skillClip.OriginalDuration, out int correctionDuration, skillClip))
                {
                    if (skillClip.OriginalDuration == correctionDuration)
                    {
                        skillClip.Duration = skillClip.OriginalDuration;
                    }
                }
            }
        }
        
        
        
        
        //在轨道中移动Clip
        public static void MoveClipToFrame(this Skill.SkillTrack skillTrack,SkillClip skillClip, int startFrame)
        {
            var clips = skillTrack.clips;
            
            //类型验证，权限范围验证
            if (skillClip is not SkillAnimationSkillClip animationClip || !clips.Contains(animationClip)) return;
            //判断是否可以移动到该为止
            if (!CanInsertClipAtFrame(skillTrack,startFrame, skillClip.Duration, out int correctionDuration, skillClip)) return;
            //判断插入时长度是否被修正，如果被修正则不可以移动
            if (skillClip.Duration != correctionDuration) return;
            
            skillClip.StartFrame = startFrame;
            
            clips = clips.OrderBy(clip => clip.StartFrame).ToList();

        }
        
        
        
        
        //插入轨道
        public static SkillClip InsertClipAtFrame<T>(this Skill.SkillTrack skillTrack, SkillClip skillClip) where T:SkillClip
        {
            var clips = skillTrack.clips;

            int duration = skillClip.OriginalDuration;
            if (CanInsertClipAtFrame(skillTrack,skillClip.StartFrame, duration, out int correctionDuration))
            {
                //AnimationClip clip = Clip.CreatClip<AnimationClip>(track,startFrame, animationClip.UnityClip ,correctionDuration); 
                Debug.Log($"插入一个动画片段{skillClip.ClipName}，起始帧为{skillClip.StartFrame}，原始长度为{duration}，修正长度为{correctionDuration},轨道:{skillTrack}");
                clips.Add(skillClip);
                clips = clips.OrderBy(clip => clip.StartFrame).ToList();
                return skillClip;
            }
            return null;
        }
        
        
        //移除片段
        public static SkillClip RemoveClip(this Skill.SkillTrack skillTrack,SkillClip skillClip)
        {
            if (skillClip is SkillAnimationSkillClip animationClip)
            {
                skillTrack.clips.Remove(animationClip);
            }
            
            return skillClip;    
        }

        
        // public static Clip InsertClipAtFrame(this Track track,int startFrame, object obj)
        // {
        //     if (obj is UnityEngine.AnimationClip animationClip)
        //     {
        //         return InsertClipAtFrame(track,startFrame, animationClip);
        //     }
        //
        //     return null;
        // }


        public static SkillClip UnityAnimationClipToSKillAnimationClip(this AnimationClip clip, Skill.SkillTrack skillTrack,int startFrame)
        {
            return SkillClip.CreatClip<SkillAnimationSkillClip>(skillTrack, startFrame, clip, Mathf.CeilToInt(clip.length * clip.frameRate));
        }
    }
}