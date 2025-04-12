using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    [CreateAssetMenu(menuName = "Skill")]
    public class SkillConfig : ScriptableObject
    {
        public string SkillName;
        public int FrameCount;
        public int frameRate = 30;
        public float frameTime = 1.0f / 30.0f;
        public float totalTime => FrameCount * frameTime;
        public List<Track> tracks;

        public void PreviewUpdate(float currentTime, int currentFrame, GameObject previewObject,bool isPlaying)
        {
            foreach (var track in tracks)
            {
                track.PreviewUpdate(currentTime,currentFrame,previewObject,isPlaying);
            }
        }

        public void PreviewStop(GameObject previewObject)
        {
            foreach (var track in tracks)
            {
                track.PreviewStop(previewObject);
            }
        }
    }
}
