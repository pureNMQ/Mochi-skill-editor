using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    public interface ITrack
    {
        public SkillConfig SkillConfig { get; }
        public string TrackName { get; set; }
        public int ClipCount { get; }
        public void Initialize();
        public bool CanConvertToClip(object obj);
        public void ResetClipDuration(Clip clip);
        // public  bool CanInsertClipAtFrame(int startFrame, int duration, out int correctionDuration,
        //     Clip ignoreClip = null);
        public int CalculateCorrectionDuration(int startFrame, int duration, params Clip[] ignoreClips);
        public Clip InsertObjectAtFrame(int startFrame, object obj);
        public void InsertClipAtFrame(int startFrame, Clip clip);
        public bool MoveClipToFrame(Clip clip, int startFrame);
        public void RemoveClip(Clip clip);
        public TrackHandler CreateTrackHandler(GameObject gameObject);
        public Clip this[int index]{ get; }
        public IEnumerator<Clip> GetEnumerator();
    }
    
    [Serializable]
    public abstract class Track<TClip> : ITrack where TClip : Clip
    {
        public SkillConfig SkillConfig => skillConfig;
        [SerializeReference,HideInInspector] protected SkillConfig skillConfig;

        public string TrackName
        {
            get => _trackName;
            set => _trackName = value;
        }
        public int ClipCount => clips?.Count ?? 0;
        
        [SerializeReference] public List<TClip> clips = new List<TClip>();
        
        private string _trackName;

        public abstract void Initialize();
        //TODO 删除原有转换Clip功能，替换为新版转换Clip
        public abstract bool CanConvertToClip(object obj);
        public abstract TClip ConvertToClip(object obj);
        public abstract TrackHandler CreateTrackHandler(GameObject gameObject);
        
        public TClip this[int index] => clips[index];
        
        
        //TODO 插入Clip的逻辑需要修改
        public virtual TClip InsertObjectAtFrame(int startFrame, object obj)
        {
            //TODO 实现插入可转换为Clip的对象
            TClip clip = ConvertToClip(obj);
            if (clip is null) return null;
            InsertClipAtFrame(startFrame,clip.duration,clip);
            return clip;
        }

        public virtual void InsertClipAtFrame(int startFrame,int duration, TClip clip)
        {
            if(clip is null) return;
            if (clip.Track != null)
            {
                clip.Track.RemoveClip(clip);
            }

            int correctionDuration = CalculateCorrectionDuration(startFrame, duration);
            if (correctionDuration < 1)
            {
                Debug.LogWarning("插入一个片段失败，因为空余长度不足");
                return;
            }

            clip.Track = this;
            clip.duration = correctionDuration;
            clips.Add(clip);
            clips = clips.OrderBy(c => c.startFrame).ToList();
        }
        
        public void ResetClipDuration(TClip clip)
        {
            if (clips.Contains(clip))
            {
                int correctionDuration = CalculateCorrectionDuration(clip.startFrame, clip.OriginalDuration, clip);
                if (clip.OriginalDuration == correctionDuration)
                {
                    clip.duration = clip.OriginalDuration;
                }
                else
                {
                    Debug.LogWarning($"{this}的长度无法被重置,因为空余长度不足");
                }
            }
        }
        
        public virtual IEnumerator<TClip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }
        
        public virtual bool MoveClipToFrame(TClip clip, int startFrame)
        {
            //类型验证，权限范围验证
            if (!clips.Contains(clip)) return false;
            
            //判断插入时长度是否被修正，如果被修正则不可以移动
            int correctionDuration = CalculateCorrectionDuration(startFrame, clip.duration, clip);
            if (clip.duration != correctionDuration) return false;
            
            clip.startFrame = startFrame;
            clips = clips.OrderBy(clip => clip.startFrame).ToList();
            return true;
        }

        public virtual void RemoveClip(TClip clip)
        {
            clips.Remove(clip);
        }

        public virtual int CalculateCorrectionDuration(int startFrame, int duration,params Clip[] ignoreClips)
        {
            int correctionDuration = duration;
            foreach (var item in clips)
            {
                if(ignoreClips is not null && ignoreClips.Contains(item)) continue;
                
                //不允许插入到另一个Clip中间
                //情况一:插入Clip的起始点位于另一个Clip中
                if (startFrame >= item.startFrame && startFrame < item.EndFrame)
                {
                    correctionDuration = 0;
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

            return correctionDuration;
        }
        
        
        //显式接口
        Clip ITrack.this[int index] => this[index];
        IEnumerator<Clip> ITrack.GetEnumerator()
        {
            return GetEnumerator();
        }
        Clip ITrack.InsertObjectAtFrame(int startFrame, object obj)
        {
            return InsertObjectAtFrame(startFrame,obj);
        }

        void ITrack.InsertClipAtFrame(int startFrame, Clip clip)
        {
            if (clip is not TClip tClip) return;
            InsertClipAtFrame(startFrame,tClip.duration,tClip);
        }

        bool ITrack.MoveClipToFrame(Clip clip, int startFrame)
        {
            if (clip is TClip)
            {
                return MoveClipToFrame(clip as TClip, startFrame);
            }

            return false;
        }

        void ITrack.ResetClipDuration(Clip clip)
        {
            if (clip is TClip tClip)
            {
                ResetClipDuration(tClip);
            }
        }

        void ITrack.RemoveClip(Clip clip)
        {
            if (clip is TClip)
            {
                RemoveClip(clip as TClip);
            }
        }

    }
}
