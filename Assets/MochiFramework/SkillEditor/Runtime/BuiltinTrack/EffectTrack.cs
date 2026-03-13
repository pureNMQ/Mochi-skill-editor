using UnityEngine;

namespace MochiFramework.Skill
{
    [CustomTrack(DefaultName = "特效轨道")]
    public class EffectTrack : Track<EffectClip>
    {
        public override void Initialize()
        {

        }

        public override bool CanConvertToClip(object obj)
        {
            return obj is GameObject || obj is EffectClip;
        }

        public override EffectClip ConvertToClip(object obj)
        {
            EffectClip clip = null;
            if (obj is GameObject go)
            {
                clip = new EffectClip();
                clip.EffectPrefab = go;
                clip.duration = clip.OriginalDuration;
            }
            else if (obj is EffectClip ec)
            {
                clip = ec;
            }

            return clip;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            return new EffectTrackHandler(this, gameObject);
        }
    }
}