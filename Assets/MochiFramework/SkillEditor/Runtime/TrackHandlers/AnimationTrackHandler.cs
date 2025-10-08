using System;
using UnityEngine;
using UnityEngine.Animations;
using Animancer;

namespace MochiFramework.Skill
{
    public sealed class AnimationTrackHandler : TrackHandler
    {
        private AnimationTrack animationTrack;
        private AnimancerComponent animancer;
        
        public AnimationTrackHandler(AnimationTrack track,GameObject gameObject) : base(track)
        {
            animationTrack = track;
            animancer = gameObject.GetComponentInChildren<AnimancerComponent>();
            if (animancer == null)
            {
                animancer = gameObject.AddComponent<AnimancerComponent>();
            }

            if (animancer.Animator == null)
            {
                Animator animator = gameObject.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    animator = gameObject.AddComponent<Animator>();
                }
                animancer.Animator = animator;
            }

            animancer.UpdateMode = AnimatorUpdateMode.Normal;
            animancer.Graph.PauseGraph();
        }

        public override void Play(int currentFrame = 0)
        {
            animancer.Graph.UnpauseGraph();
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (currentFrame >= track[i].startFrame && currentFrame <= track[i].EndFrame)
                {
                    UnityEngine.AnimationClip asset = FromClipGetAnimationAsset(track[i]);
                    var state = animancer.Play(asset);
                    state.Time = (currentFrame - track[i].startFrame) * track.SkillConfig.frameTime;
                    break;
                }
            }
        }

        public override void Update(int currentFrame)
        {
            if(animancer == null) return;
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (currentFrame == track[i].startFrame)
                {
                    UnityEngine.AnimationClip asset = FromClipGetAnimationAsset(track[i]);
                    animancer.Play(asset,0,FadeMode.FromStart);
                }
            }
        }

        public override void Evaluate(int currentFrame)
        {
            if(animancer == null) return;
            for (int i = 0; i < track.ClipCount; i++)
            {
                if (currentFrame >= track[i].startFrame && currentFrame <= track[i].EndFrame)
                {
                    UnityEngine.AnimationClip asset = FromClipGetAnimationAsset(track[i]);
                    var state = animancer.Play(asset);
                    state.Time = (currentFrame - track[i].startFrame) * track.SkillConfig.frameTime;
                    animancer.Evaluate();
                    return;
                }
            }
            
        }

        public override void Stop()
        {
            animancer.Graph.PauseGraph();
        }
        
        private UnityEngine.AnimationClip FromClipGetAnimationAsset(Clip clip)
        {
            if (clip is AnimationClip animationClip)
            {
                return animationClip.AnimationAsset;
            }
            
            return null;
        }
        
        
    }
    
}