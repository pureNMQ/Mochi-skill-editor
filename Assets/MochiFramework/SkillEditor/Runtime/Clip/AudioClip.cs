using UnityEngine;

namespace MochiFramework.Skill.Audio
{
    public class AudioClip:Clip
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

        public static AudioClip CreateClip(Track track,int startFrame, UnityEngine.AudioClip Clip,int d)
        {
            return null;
        }
    }
}