using System;
using UnityEditor;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine;


namespace MochiFramework.Skill
{
    public class AudioTrackHandler : TrackHandler
    {
        public static event EventHandler<UnityEngine.AudioClip> PlayPreviewClip;
        public static event EventHandler StopPreviewClip;

        private GameObject _gameObject;
        private Transform _transform;
        
        public AudioTrackHandler(ITrack track,GameObject gameObject) : base(track)
        {
            _gameObject = gameObject;
            _transform = gameObject.transform;
        }

        public override void Play(int currentFrame = 0)
        {
           
        }

        public override void Update(int currentFrame)
        {
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (track[i].startFrame == currentFrame && track[i] is AudioClip audioClip)
                {
                    PlayAudioClip(audioClip);
                }
            }
        }

        public override void Evaluate(int currentFrame)
        {
            
        }

        public override void Stop()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                StopPreviewClip?.Invoke(this, null);
            }
#endif
        }


        private void PlayAudioClip(AudioClip clip)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                PlayPreviewClip?.Invoke(this, clip.AudioAsset);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip.AudioAsset,_transform.position);
            }
#else
            //运行时播放的内容
            AudioSource.PlayClipAtPoint(clip.AudioAsset,_transform.position);
#endif
            
        }
    }
}