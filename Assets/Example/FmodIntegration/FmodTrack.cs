using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace MochiFramework.Skill
{
    public class FmodTrack : Track<FmodClip>
    {
        public class EventArgs : System.EventArgs
        {
            public FMOD.Studio.EventInstance eventInstance { get; set; }
        }
        
        public delegate bool Condition(object obj);
        
        //public static event System.EventHandler<EventArgs> OnEventInstanceCreated;

        public static event Condition ConvertCondition;
        
        public override void Initialize()
        {
            
        }

        public override bool CanConvertToClip(object obj)
        {
            //return obj is FMODUnity.EditorEventRef;
            // Debug.Log("CanConvertToClip");
            // Debug.Log(ConvertCondition == null);
            Debug.Log(ConvertCondition?.Invoke(obj) ?? false);
            return ConvertCondition?.Invoke(obj) ?? false;
        }

        public override FmodClip InsertClipAtFrame(int startFrame, object obj)
        {
            //TODO 实现Fmod Clip插入功能
            return null;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            //TODO 实现Fmod Track Handler的构建
            return null;
        }
    }
}
