using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public abstract class Track
    {
        public abstract bool CanConvertToClip(object obj);
        public abstract bool CanInsertClipAtFrame(int index);
        public abstract void InsertClipAtFrame(int startFrame, object obj);
    }


}
