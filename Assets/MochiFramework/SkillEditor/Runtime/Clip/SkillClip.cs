using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class SkillClip
    {
        public abstract string ClipName { get; }
        public abstract int OriginalDuration { get; }
        
        public abstract Object UnityClip { get; set; }
        
        //public SkillTrack SkillTrack => skillTrack;
        //public SkillConfig SkillConfig => skillTrack.SkillConfig;
        
        /// <summary>
        /// 该属性的起始帧为0，受保护的字段startFrame的起始帧为1
        /// </summary>
        public int StartFrame
        {
            get => startFrame - 1;
            set => startFrame = value + 1;
        }

        public int EndFrame
        {
            get => startFrame + duration - 1;
        }

        public int Duration
        {
            get => duration;
            set => duration = value;
        }
        
        //public float StartTime => startFrame * SkillConfig.frameTime;
        
        // [FormerlySerializedAs("track")] [SerializeReference,HideInInspector] 
        // protected SkillTrack skillTrack;
        /// <summary>
        /// 该字段的起始帧为1，如果想要使用起始帧为0的模式请使用StartFrame属性
        /// </summary>
        [SerializeField] protected int startFrame;
        [SerializeField] protected int duration;

        public static T CreatClip<T>(SkillTrack skillTrack,int startFrame, Object unityClip,int duration) where T : SkillClip
        {
            T clip =  Activator.CreateInstance<T>();
           // clip.skillTrack = skillTrack;
            clip.StartFrame = startFrame;
            clip.UnityClip = unityClip;
            clip.duration = duration;
            return clip;
        }
        
    }
}
