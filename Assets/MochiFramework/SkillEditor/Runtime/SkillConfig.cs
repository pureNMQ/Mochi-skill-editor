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
        [SerializeReference] public List<Track> tracks;
        public float totalTime => FrameCount * frameTime;
        
        
    }
}
