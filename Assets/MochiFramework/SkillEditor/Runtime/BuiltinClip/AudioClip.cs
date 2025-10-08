using UnityEngine;
using System;

namespace MochiFramework.Skill
{
    [Serializable]
    public class AudioClip : Clip
    {
        public UnityEngine.AudioClip AudioAsset
        {
            get => audioAsset;
            set =>  audioAsset = value;
        }

        [SerializeField] protected UnityEngine.AudioClip audioAsset;

        public override string ClipName => audioAsset == null ? "空Audio" : audioAsset.name;
        public override int OriginalDuration => Mathf.CeilToInt(audioAsset.length * audioAsset.frequency);
        

        public static AudioClip CreateAudioClip(ITrack track,int startFrame, UnityEngine.AudioClip unityAudioClip,int duration)
        {
            AudioClip clip = new AudioClip();
            
            clip.track = track;
            clip.startFrame = startFrame;
            clip.audioAsset = unityAudioClip;
            clip.duration = duration;
            return clip;
        }
    }
}