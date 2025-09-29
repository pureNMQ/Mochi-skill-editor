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

        public string TrackName
        {
            get => _trackName;
            set => _trackName = value;
        }
        public int ClipCount => clips?.Count ?? 0;
        
        [SerializeReference] public List<Clip> clips = new List<Clip>();
        
        private string _trackName;

        public abstract void Initialize();
        public abstract bool CanConvertToClip(object obj);
        public abstract Clip InsertClipAtFrame(int startFrame, object obj);
        public abstract TrackHandler CreateTrackHandler(GameObject gameObject);
        
        public virtual Clip GetClipAt(int index)
        {
            return clips[index];
        }
        
        public virtual bool CanInsertClipAtFrame(int startFrame,int duration, out int correctionDuration,Clip ignoreClip = null)
        {
            correctionDuration = duration;
            foreach (var item in clips)
            {
                if(item == ignoreClip) continue;
                
                //不允许插入到另一个Clip中间
                //情况一:插入Clip的起始点位于另一个Clip中
                if (startFrame >= item.startFrame && startFrame < item.EndFrame)
                {
                    correctionDuration = 0;
                    return false;
                }
                //情况二:插入Clip的结束点位于另一个Clip中
                if (startFrame < item.startFrame && startFrame + duration >= item.startFrame)
                {
                    int offset = item.startFrame - startFrame;
                    if (offset < correctionDuration)
                    {
                        correctionDuration = offset;
                    }
                }
            }
            
            //情况三:插入Clip的结束点位于Track长度之外
            if (startFrame + duration > skillConfig.frameCount)
            {
                int offset = skillConfig.frameCount - startFrame;
                if (offset < correctionDuration)
                {
                    correctionDuration = offset;
                }
            }
            
            return true;
        }
        
        public virtual void ResetClipDuration(Clip clip)
        {
            if (clips.Contains(clip))
            {
                if (CanInsertClipAtFrame(clip.startFrame, clip.OriginalDuration, out int correctionDuration, clip))
                {
                    if (clip.OriginalDuration == correctionDuration)
                    {
                        clip.duration = clip.OriginalDuration;
                    }
                }
                else
                {
                    Debug.LogWarning($"{this}的长度无法被重置,因为空余长度不足");
                }
            }
        }

        public virtual IEnumerator<Clip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }
        

        public virtual bool MoveClipToFrame(Clip clip, int startFrame)
        {
            //类型验证，权限范围验证
            if (!clips.Contains(clip)) return false;
            //判断是否可以移动到该为止
            if (!CanInsertClipAtFrame(startFrame, clip.duration, out int correctionDuration, clip)) return false;
            //判断插入时长度是否被修正，如果被修正则不可以移动
            if (clip.duration != correctionDuration) return false;
            
            clip.startFrame = startFrame;
            
            clips = clips.OrderBy(clip => clip.startFrame).ToList();
            
            return true;
        }

        public virtual Clip RemoveClip(Clip clip)
        {
            clips.Remove(clip);
            return clip;    
        }
        

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
