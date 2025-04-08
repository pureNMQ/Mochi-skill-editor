using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    [Serializable]
    public abstract class Track : ScriptableObject, IEnumerable<Clip>
    {
        public abstract string TrackName { get; }
        public abstract bool CanConvertToClip(object obj);
        public abstract bool CanInsertClipAtFrame(int startFrame,int duration,out int correctionDuration,Clip ignoreClip = null);
        public abstract Clip InsertClipAtFrame(int startFrame, object obj);
        public abstract Clip RemoveClip(Clip clip);
        public abstract void MoveClipToFrame(Clip clip, int startFrame);
        public abstract void ResetClipDuration(Clip clip);
        public abstract IEnumerator<Clip> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
