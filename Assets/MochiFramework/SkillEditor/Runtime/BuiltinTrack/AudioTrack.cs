using UnityEngine;
using System.Linq;

namespace MochiFramework.Skill
{
    [CustomTrack(DefaultName = "音频轨道")]
    public class AudioTrack : Track
    {
        public override void Initialize()
        {
            
        }

        public override bool CanConvertToClip(object obj)
        {
            if (obj is UnityEngine.AudioClip)
            {
                return true;
            }
            else if (obj is AudioClip)
            {
                return true;
            }
            return false;
        }

        public override Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AudioClip unityAudioClip)
            {
                return InsertAudioClipAtFrame(startFrame, unityAudioClip);
            }
            else if (obj is AudioClip audioClip)
            {
                return InsertAudioClipAtFrame(startFrame, audioClip);
            }

            return null;
        }

        private Clip InsertAudioClipAtFrame(int startFrame, UnityEngine.AudioClip unityAudioClip)
        {
             int duration = Mathf.CeilToInt(unityAudioClip.length * skillConfig.frameRate);
             if (CanInsertClipAtFrame(startFrame, duration, out int correctionDuration))
             {
                 AudioClip clip = AudioClip.CreateAudioClip(this,startFrame, unityAudioClip,correctionDuration); 
                 Debug.Log($"插入一个音频片段{unityAudioClip.name}，起始帧为{startFrame}，原始长度为{duration}，修正长度为{correctionDuration},轨道:{clip.Track}");
                 clips.Add(clip);
                 clips = clips.OrderBy(clip => clip.startFrame).ToList();
                 return clip;
             }
             return null;
        }
        
        private Clip InsertAudioClipAtFrame(int startFrame, AudioClip audioClip)
        {
            if (CanInsertClipAtFrame(startFrame, audioClip.duration, out int correctionDuration))
            {
                if (correctionDuration != audioClip.duration)
                {
                    Debug.LogWarning("轨道空间太少");
                    return null;
                }
                
                audioClip.Track.RemoveClip(audioClip);
                audioClip.Track = this;
                audioClip.startFrame = startFrame;
                clips.Add(audioClip);
            }
            
            
            return null;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            return new AudioTrackHandler(this,gameObject);
        }
    }
}