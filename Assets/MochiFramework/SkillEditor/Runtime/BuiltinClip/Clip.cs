using System;
using UnityEngine;

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
        

        public int EndFrame
        {
            get => startFrame + duration;
        }
        
        public float StartTime => startFrame * SkillConfig.frameTime;
        
        [SerializeReference,HideInInspector] protected Track track;
        
        /// <summary>
        /// 该字段的起始帧为0
        /// </summary>
        public int startFrame;
        public int duration;
        

    }
}
