using UnityEngine;
using System.Linq;

namespace MochiFramework.Skill
{
    [CustomTrack(DefaultName = "音频轨道")]
    public class AudioTrick : Track
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
            return false;
        }

        public override Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AudioClip audioClip)
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
        
        
    }
}