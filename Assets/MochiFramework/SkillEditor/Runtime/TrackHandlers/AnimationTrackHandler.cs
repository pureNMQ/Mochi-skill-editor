using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MochiFramework.Skill
{
    public class AnimationTrackHandler : TrackHandler
    {
        private SkillAnimationSkillTrack _skillAnimationSkillTrack;
        private Animator animator;
        private int currentClipIndex;
        private AnimationPlayableOutput playableOutput;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable animationClipPlayable;
        
        public AnimationTrackHandler(SkillAnimationSkillTrack skillTrack,Animator animator) : base(skillTrack)
        {
            _skillAnimationSkillTrack = skillTrack;
            this.animator = animator;
            
            playableGraph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationTrack", animator);
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            animationClipPlayable = AnimationClipPlayable.Create(playableGraph, null);
            currentClipIndex = -1;
            
            lateFrame = 0;
            lateTime = 0;
        }

        public override void Play(int currentFrame = 0, float currentTime = 0)
        {
            lateFrame = currentFrame;
            lateTime = currentTime;
            //前面几帧可能没有动画
            currentClipIndex = -1;

            ChangeAnimationClipIndex(currentFrame);
            
            Debug.Log($"播放开始{currentFrame}，初始化ClipIndex:{currentClipIndex}");
        }

        public override void Update(float deltaTime, int currentFrame, float currentTime)
        {
            ChangeAnimationClipIndex(currentFrame);
            playableGraph.Evaluate(deltaTime);

            lateFrame = currentFrame;
            lateTime = currentTime;

        }

        public override void Evaluate(int currentFrame, float currentTime)
        {
            ChangeAnimationClipIndex(currentFrame);
            if (currentClipIndex >= 0)
            {
                //后续重构
                var temp = (_skillAnimationSkillTrack.clips[currentClipIndex].StartFrame+1)*SkillTrack.SkillConfig.frameTime;
                animationClipPlayable.SetTime(currentTime - temp);
            }
            
            playableGraph.Evaluate();
            
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
        
        private void ChangeAnimationClipIndex(int currentFrame)
        {
            //TODO 可能在其他trackHandler中也经常使用，待重构
            if(lateFrame == currentFrame) return;
            
            bool isRebuild = false;
            if (lateFrame < currentFrame)
            {
                while(currentClipIndex + 1 < _skillAnimationSkillTrack.ClipCount &&
                      _skillAnimationSkillTrack.clips[currentClipIndex + 1].StartFrame < currentFrame)
                {
                    currentClipIndex++;
                    isRebuild = true;
                }
            }
            else 
            {
                while (currentClipIndex >= 0 && _skillAnimationSkillTrack.clips[currentClipIndex].StartFrame > currentFrame)
                {
                    currentClipIndex--;
                    isRebuild = true;
                }
                
            }

            if (isRebuild)
            {
                CreateAnimationClipPlayable();
            }
        }

        private void CreateAnimationClipPlayable()
        {
            if (currentClipIndex < 0)
            {
                animationClipPlayable = AnimationClipPlayable.Create(playableGraph,null);
            }
            else if (_skillAnimationSkillTrack.clips[currentClipIndex] is SkillAnimationSkillClip clip)
            {
                animationClipPlayable = AnimationClipPlayable.Create(playableGraph,clip.AnimationAsset);
                //后续重构
                var temp = (_skillAnimationSkillTrack.clips[currentClipIndex].StartFrame+1)*SkillTrack.SkillConfig.frameTime;
                animationClipPlayable.SetTime(lateTime - temp);
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