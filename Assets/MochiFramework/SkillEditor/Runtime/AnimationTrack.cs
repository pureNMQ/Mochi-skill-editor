using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class AnimationTrack : Track
    {
        protected List<AnimationClip> clips;
        public AnimationTrack()
        {
            clips = new List<AnimationClip>();
        }

        public void Add(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            clips.Add(new AnimationClip(startFrame, animationClip));
        }

        public override bool CanConvertToClip(object obj)
        {
            return obj is UnityEngine.AnimationClip;
        }

        public override bool CanInsertClipAtFrame(int index)
        {
            return true;
        }

        public override void InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AnimationClip animationClip)
            {
                InsertClipAtFrame(startFrame, animationClip);
            }
        }

        public void InsertClipAtFrame(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            AnimationClip clip = new AnimationClip(startFrame, animationClip);
            clips.Add(clip);
        }
    }
}
