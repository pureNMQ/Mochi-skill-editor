using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class PrefabTrack : Track<PrefabClip>
    {
        public override void Initialize()
        {
            
        }

        public override bool CanConvertToClip(object obj)
        {
            return obj is GameObject;
        }

        public override PrefabClip ConvertToClip(object obj)
        {
            return null;
        }

        private PrefabTrack InsertPrefabClipAtFrame(int startFrame, GameObject prefab)
        {
            return this;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            return null;
        }
    }
}
