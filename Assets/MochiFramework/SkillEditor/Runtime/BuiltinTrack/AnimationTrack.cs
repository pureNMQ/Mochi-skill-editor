using UnityEngine;
using System.Linq;

namespace MochiFramework.Skill
{
    [CustomTrack(DefaultName = "动画轨道",IsUnique = true)]
    public sealed class AnimationTrack : Track
    {
        public override void Initialize()
        {
            
        }

        public override bool CanConvertToClip(object obj)
        {
            return obj is UnityEngine.AnimationClip;
        }
        public override Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AnimationClip animationClip)
            {
                return InsertAnimationClipAtFrame(startFrame, animationClip);
            }

            return null;
        }
        private Clip InsertAnimationClipAtFrame(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            int duration = Mathf.CeilToInt(animationClip.length * animationClip.frameRate);
            if (CanInsertClipAtFrame(startFrame, duration, out int correctionDuration))
            {
                AnimationClip clip = AnimationClip.CreateAnimationClip(this,startFrame, animationClip,correctionDuration); 
                Debug.Log($"插入一个动画片段{animationClip.name}，起始帧为{startFrame}，原始长度为{duration}，修正长度为{correctionDuration},轨道:{clip.Track}");
                clips.Add(clip);
                clips = clips.OrderBy(clip => clip.startFrame).ToList();
                return clip;
            }

            return null;
        }

        public override TrackHandler CreateTrackHandler(GameObject gameObject)
        {
            return new AnimationTrackHandler(this,gameObject);
        }
    }
}
