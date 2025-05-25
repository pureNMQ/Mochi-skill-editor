using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class Track : IEnumerable<Clip> 
    {
        public SkillConfig SkillConfig => skillConfig;
        [SerializeReference,HideInInspector] protected SkillConfig skillConfig;
        public abstract string TrackName { get; }
        public abstract int ClipCount { get; }
        
        [SerializeReference] public List<Clip> clips = new List<Clip>();
        public virtual Clip GetClipAt(int index)
        {
            return clips[index];
        }
        
        public virtual bool CanConvertToClip(object obj)
        {
            return obj is UnityEngine.AnimationClip;
        }

        public virtual bool CanInsertClipAtFrame(int startFrame,int duration, out int correctionDuration,Clip ignoreClip = null)
        {
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

        public virtual Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AnimationClip animationClip)
            {
                return InsertClipAtFrame(startFrame, animationClip);
            }

            return null;
        }

        public virtual void ResetClipDuration(Clip clip)
        {
            //TODO 重置Clip的长度为原始长度 如果可以的话
            if (clips.Contains(clip))
            {
                if (CanInsertClipAtFrame(clip.StartFrame, clip.OriginalDuration, out int correctionDuration, clip))
                {
                    if (clip.OriginalDuration == correctionDuration)
                    {
                        clip.Duration = clip.OriginalDuration;
                    }
                }
            }
        }

        public virtual IEnumerator<Clip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }

        public Clip InsertClipAtFrame(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            int duration = Mathf.CeilToInt(animationClip.length * animationClip.frameRate);
            if (CanInsertClipAtFrame(startFrame, duration, out int correctionDuration))
            {
                AnimationClip clip = AnimationClip.CreateAnimationClip(this,startFrame, animationClip,correctionDuration); 
                Debug.Log($"插入一个动画片段{animationClip.name}，起始帧为{startFrame}，原始长度为{duration}，修正长度为{correctionDuration},轨道:{clip.Track}");
                clips.Add(clip);
                clips = clips.OrderBy(clip => clip.StartFrame).ToList();
                return clip;
            }

            return null;
        }

        public virtual bool MoveClipToFrame(Clip clip, int startFrame)
        {
            //类型验证，权限范围验证
            if (clip is not AnimationClip animationClip || !clips.Contains(animationClip)) return false;
            //判断是否可以移动到该为止
            if (!CanInsertClipAtFrame(startFrame, clip.Duration, out int correctionDuration, clip)) return false;
            //判断插入时长度是否被修正，如果被修正则不可以移动
            if (clip.Duration != correctionDuration) return false;
            
            clip.StartFrame = startFrame;
            
            clips = clips.OrderBy(clip => clip.StartFrame).ToList();
            
            return true;
        }

        public virtual Clip RemoveClip(Clip clip)
        {
            if (clip is AnimationClip animationClip)
            {
                clips.Remove(animationClip);
            }
            
            return clip;    
        }


        public abstract void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,bool isPlaying);
        public abstract void PreviewStop(GameObject previewObject);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
