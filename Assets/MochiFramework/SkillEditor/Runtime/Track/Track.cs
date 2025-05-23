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
        //技能配置
        [SerializeReference, HideInInspector] protected SkillConfig skillConfig;
        public SkillConfig SkillConfig => skillConfig;

        //当前帧
        protected int currentClip;
        public int CurrentClip => currentClip;

        //轨道名称
        public abstract string TrackName { get; }
        public abstract int ClipCount { get; }


        [SerializeReference] public List<Clip> clips = new List<Clip>();

        public virtual Clip GetClipAt(int index)
        {
            return clips[index];
        }

        public virtual IEnumerator<Clip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }





        public abstract void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,
            bool isPlaying);

        public abstract void PreviewStop(GameObject previewObject);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
