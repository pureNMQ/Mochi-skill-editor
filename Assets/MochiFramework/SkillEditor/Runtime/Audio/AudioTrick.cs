using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill.Audio
{
    public class AudioTrick:Track
    {
        public override string TrackName => "音频轨道";
        public override int ClipCount => clips.Count;
        public override void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject, bool isPlaying)
        {
            throw new System.NotImplementedException();
        }

        public override void PreviewStop(GameObject previewObject)
        {
            throw new System.NotImplementedException();
        }
    }
}