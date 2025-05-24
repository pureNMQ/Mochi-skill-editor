using UnityEngine;

namespace MochiFramework.Skill.Audio
{
    public class SkillAudioSkillClip:SkillClip
    {
        public UnityEngine.AudioClip AudioAsset
        {
            get => audioAsset;
            set =>  audioAsset = value;
        }

        [SerializeField] protected UnityEngine.AudioClip audioAsset;

        public override string ClipName => audioAsset == null ? "空Audio" : audioAsset.name;
        public override int OriginalDuration => Mathf.CeilToInt(audioAsset.length * audioAsset.frequency);
        public override Object UnityClip { get; set; }

        public static SkillAudioSkillClip CreateClip(SkillTrack skillTrack,int startFrame, UnityEngine.AudioClip Clip,int d)
        {
            return null;
        }
    }
}