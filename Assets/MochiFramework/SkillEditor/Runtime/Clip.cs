using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class Clip
    {
        public abstract string ClipName { get; }
        public abstract int OriginalDuration { get; }
        
        public Track Track => track;
        public SkillConfig SkillConfig => track.SkillConfig;
        
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
        
        public float StartTime => StartFrame * SkillConfig.frameTime;
        
        [SerializeReference,HideInInspector] protected Track track;
        /// <summary>
        /// 该字段的起始帧为1，如果想要使用起始帧为0的模式请使用StartFrame属性
        /// </summary>
        [SerializeField] protected int startFrame;
        [SerializeField] protected int duration;
        

    }
}
