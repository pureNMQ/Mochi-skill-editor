using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [CustomTrack(DefaultName = "预制体轨道")]
    public class PrefabTrack : Track<PrefabClip>
    {
        public override void Initialize()
        {

        }

        public override bool CanConvertToClip(object obj)
        {
            return obj is GameObject || obj is PrefabClip;
        }

        public override PrefabClip ConvertToClip(object obj)
        {
            PrefabClip clip = null;
            if (obj is GameObject go)
            {
                clip = new PrefabClip();
                clip.Prefab = go;
                clip.duration = clip.OriginalDuration;
            }
            else if (obj is PrefabClip pc)
            {
                clip = pc;
            }

            return clip;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            return new PrefabTrackHandler(this, gameObject);
        }
    }
}
