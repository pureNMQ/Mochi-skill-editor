using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class SkillConfig : ScriptableObject
    {
        public string SkillName;
        public int FrameCount;
        public List<Track> tracks;
    }
}
