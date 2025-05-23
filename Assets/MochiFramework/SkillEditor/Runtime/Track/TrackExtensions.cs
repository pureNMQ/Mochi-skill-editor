using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    public static class TrackExtensions
    {
        //判断是否能转换
        //后续重构
        public static bool  CanConvertToClip<T>(this Skill.Track track,object obj)// where T:Clip
        {
            return obj is T;
        }
        
        
        //判断是否可以插入进去
        public static bool CanInsertClipAtFrame(this Skill.Track track,int startFrame,int duration, out int correctionDuration,Clip ignoreClip = null)
        {
            var clips = track.clips;
            var skillConfig = track.SkillConfig;
            
            correctionDuration = duration;
            foreach (var item in clips)
            {
                if(item == ignoreClip) continue;
                
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
        public static void ResetClipDuration(this Skill.Track track,Clip clip)
        {
            //TODO 重置Clip的长度为原始长度 如果可以的话
            if (track.clips.Contains(clip))
            {
                if (CanInsertClipAtFrame(track,clip.StartFrame, clip.OriginalDuration, out int correctionDuration, clip))
                {
                    if (clip.OriginalDuration == correctionDuration)
                    {
                        clip.Duration = clip.OriginalDuration;
                    }
                }
            }
        }
        
        
        
        
        //在轨道中移动Clip
        public static void MoveClipToFrame(this Skill.Track track,Clip clip, int startFrame)
        {
            var clips = track.clips;
            
            //类型验证，权限范围验证
            if (clip is not AnimationClip animationClip || !clips.Contains(animationClip)) return;
            //判断是否可以移动到该为止
            if (!CanInsertClipAtFrame(track,startFrame, clip.Duration, out int correctionDuration, clip)) return;
            //判断插入时长度是否被修正，如果被修正则不可以移动
            if (clip.Duration != correctionDuration) return;
            
            clip.StartFrame = startFrame;
            
            clips = clips.OrderBy(clip => clip.StartFrame).ToList();

        }
        
        
        
        
        //插入轨道
        public static Clip InsertClipAtFrame<T>(this Skill.Track track, Clip clip) where T:Clip
        {
            var clips = track.clips;

            int duration = clip.OriginalDuration;
            if (CanInsertClipAtFrame(track,clip.StartFrame, duration, out int correctionDuration))
            {
                //AnimationClip clip = Clip.CreatClip<AnimationClip>(track,startFrame, animationClip.UnityClip ,correctionDuration); 
                Debug.Log($"插入一个动画片段{clip.ClipName}，起始帧为{clip.StartFrame}，原始长度为{duration}，修正长度为{correctionDuration},轨道:{clip.Track}");
                clips.Add(clip);
                clips = clips.OrderBy(clip => clip.StartFrame).ToList();
                return clip;
            }
            return null;
        }
        
        
        //移除片段
        public static Clip RemoveClip(this Skill.Track track,Clip clip)
        {
            if (clip is AnimationClip animationClip)
            {
                track.clips.Remove(animationClip);
            }
            
            return clip;    
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


        public static Clip UnityAnimationClipToSKillAnimationClip(this UnityEngine.AnimationClip clip, Skill.Track track,int startFrame)
        {
            return Clip.CreatClip<AnimationClip>(track, startFrame, clip, Mathf.CeilToInt(clip.length * clip.frameRate));
        }
    }
}