using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MochiFramework.Skill
{
    [CreateAssetMenu(menuName = "Skill")]
    public class SkillConfig : ScriptableObject
    {
        public string SkillName;
        //TODO 帧率应该整个项目使用的同一帧率，不需要单独设置
        public int frameRate = 30;
        [FormerlySerializedAs("FrameCount")] public int frameCount = 120;
        [SerializeReference] public List<Track> tracks;
        public float totalTime => frameCount * frameTime;
        public float frameTime => 1f / frameRate;

#if UNITY_EDITOR
        public Action onValidateAction;
        private void OnValidate()
        {
            onValidateAction?.Invoke();
        }  
#endif
    }
}
