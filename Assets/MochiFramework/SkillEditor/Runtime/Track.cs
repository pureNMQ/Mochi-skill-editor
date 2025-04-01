using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    //TODO 应该继承ScriptableObject,以便持久化存储
    public abstract class Track : IEnumerable<Clip>
    {
        public abstract bool CanConvertToClip(object obj);
        public abstract bool CanInsertClipAtFrame(int startFrame,int duration,out int correctionDuration);
        public abstract void InsertClipAtFrame(int startFrame, object obj);
        public abstract IEnumerator<Clip> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
