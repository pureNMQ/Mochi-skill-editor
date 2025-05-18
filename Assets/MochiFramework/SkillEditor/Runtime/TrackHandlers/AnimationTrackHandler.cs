using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MochiFramework.Skill
{
    public class AnimationTrackHandler : TrackHandler
    {
        private AnimationTrack animationTrack;
        private Animator animator;
        private int currentClipIndex;
        private AnimationPlayableOutput playableOutput;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable animationClipPlayable;
        
        public AnimationTrackHandler(AnimationTrack track,Animator animator) : base(track)
        {
            animationTrack = track;
            this.animator = animator;
            
            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationTrack", animator);
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        }

        public override void Play(int currentFrame = 0, float currentTime = 0)
        {
            lateFrame = currentFrame;
            lateTime = currentTime;

            for (int i = 0; i < animationTrack.ClipCount - 1; i++)
            {
                if (animationTrack.clips[i].StartFrame <= currentFrame && currentFrame < animationTrack.clips[i + 1].StartFrame)
                {
                    currentClipIndex = i;
                    break;
                }
            }
            
            CreateAnimationClipPlayable();
        }

        public override void Update(float deltaTime, int currentFrame, float currentTime)
        {
            if (lateTime <= currentTime)
            {
                if (currentClipIndex + 1 < animationTrack.ClipCount &&
                    animationTrack.clips[currentClipIndex + 1].StartFrame < currentFrame)
                {
                    currentClipIndex++;
                    CreateAnimationClipPlayable();
                }
            }
            else 
            {
                if (currentClipIndex - 1 >= 0 && animationTrack.clips[currentClipIndex].StartFrame > currentFrame)
                {
                    currentClipIndex--;
                    CreateAnimationClipPlayable();
                }
            }
            
            Debug.Log($"更新动画轨道:{currentFrame}");
            playableGraph.Evaluate(deltaTime);

            lateFrame = currentFrame;
            lateTime = currentTime;

        }

        public override void Stop()
        {
            playableGraph.Stop();
        }

        public override void Dispose()
        {
            playableGraph.Destroy();
        }

        private void CreateAnimationClipPlayable()
        {
            if (animationTrack.clips[currentClipIndex] is AnimationClip clip)
            {
                animationClipPlayable = AnimationClipPlayable.Create(playableGraph,clip.AnimationAsset);
                animationClipPlayable.SetTime(lateTime - animationTrack.clips[currentClipIndex].StartTime);
                playableOutput.SetSourcePlayable(animationClipPlayable);
                playableGraph.Play();
            }
            else
            {
                Debug.LogWarning("AnimationTrackHandler.Play: AnimationTrack clips is not AnimationClip");
            }
        }
    }
}