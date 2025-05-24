using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class SkillTrack : IEnumerable<SkillClip>
    {
        //技能配置
        [SerializeReference, HideInInspector] protected SkillConfig skillConfig;
        public SkillConfig SkillConfig => skillConfig;

        //当前帧
        protected int currentClip;
        public int CurrentClip => currentClip;

        //轨道名称
        public abstract string TrackName { get; }
        public abstract int ClipCount { get; }


        [SerializeReference] public List<SkillClip> clips = new List<SkillClip>();

        public virtual SkillClip GetClipAt(int index)
        {
            return clips[index];
        }

        public virtual IEnumerator<SkillClip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }





        public abstract void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject, bool isPlaying);

        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
