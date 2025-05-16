using System;
using System.Collections;
using System.Collections.Generic;
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
        public abstract Clip GetClipAt(int index);
        public abstract bool CanConvertToClip(object obj);
        public abstract bool CanInsertClipAtFrame(int startFrame,int duration,out int correctionDuration,Clip ignoreClip = null);
        public abstract Clip InsertClipAtFrame(int startFrame, object obj);
        public abstract Clip RemoveClip(Clip clip);
        public abstract void MoveClipToFrame(Clip clip, int startFrame);
        public abstract void ResetClipDuration(Clip clip);

        public abstract void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,bool isPlaying);
        public abstract void PreviewStop(GameObject previewObject);
        
        public abstract IEnumerator<Clip> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
