using UnityEngine.Audio;
using UnityEngine.Playables;


namespace MochiFramework.Skill
{
    public class AudioTrackHandler : TrackHandler
    {
        private readonly AudioTrack _audioTrack;
        
        private PlayableGraph _playableGraph;
        private AudioPlayableOutput _audioOutput;
        private AudioClipPlayable _clipPlayable;
        
        public AudioTrackHandler(AudioTrack track) : base(track)
        {
            this._audioTrack = track;
            _playableGraph = PlayableGraph.Create();
            _audioOutput = AudioPlayableOutput.Create(_playableGraph, "AudioOutput", null);
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.DSPClock);
            _audioOutput.SetSourcePlayable(_clipPlayable);
        }

        public override void Play(int currentFrame = 0, float currentTime = 0)
        {
            lateFrame = currentFrame;
            lateTime = currentTime;
        }

        public override void Update(float deltaTime, int currentFrame, float currentTime)
        {
            Evaluate(currentFrame,currentTime);
        }

        public override void Evaluate(int currentFrame, float currentTime)
        {
            if (lateFrame != currentFrame)
            {
                foreach (var clip in _audioTrack)
                {
                    if(clip.startFrame != currentFrame) continue;
                    
                    AudioClip audioClip = clip as AudioClip;
                    if (audioClip != null)
                    {
                        _clipPlayable = AudioClipPlayable.Create(_playableGraph, audioClip.AudioAsset, false);
                        _audioOutput.SetSourcePlayable(_clipPlayable);
                        _playableGraph.Play();
                    }
                }
            }
            
        }

        public override void Stop()
        {
            _playableGraph.Stop();
        }
        
        public override void Dispose()
        {
            _playableGraph.Destroy();
        }
    }
}