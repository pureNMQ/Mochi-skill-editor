using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class AnimationTrack : Track
    {
        public override string TrackName => "动画轨道";
        protected List<AnimationClip> clips;
        public AnimationTrack()
        {
            clips = new List<AnimationClip>();
        }
        
        public override bool CanConvertToClip(object obj)
        {
            return obj is UnityEngine.AnimationClip;
        }

        public override bool CanInsertClipAtFrame(int startFrame,int duration, out int correctionDuration,Clip ignoreClip = null)
        {
            correctionDuration = duration;
            foreach (var item in clips)
            {
                if(item == ignoreClip) continue;
                
                //不允许插入到另一个Clip中间
                //情况一:插入Clip的起始点位于另一个Clip中
                if (startFrame >= item.StartFrame && startFrame < item.EndFrame)
                {
                    Debug.Log("不可插入到其他Clip中");
                    correctionDuration = 0;
                    return false;
                }
                //情况二:插入Clip的结束点位于另一个Clip中
                if (startFrame < item.StartFrame && startFrame + duration >= item.StartFrame)
                {
                    int offset = item.StartFrame - startFrame;
                    if (offset < correctionDuration)
                    {
                        correctionDuration = offset;
                    }
                }
                
                //情况三:插入Clip的结束点位于Track长度之外
            }
            
            return true;
        }

        public override void InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AnimationClip animationClip)
            {
                InsertClipAtFrame(startFrame, animationClip);
            }
        }

        public override IEnumerator<Clip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }

        public void InsertClipAtFrame(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            int duration = Mathf.CeilToInt(animationClip.length * animationClip.frameRate);
            if (CanInsertClipAtFrame(startFrame, duration, out int correctionDuration))
            {
                AnimationClip clip = new AnimationClip(startFrame, animationClip,correctionDuration); 
                Debug.Log($"插入一个动画片段{animationClip.name}，起始帧为{startFrame}，原始长度为{duration}，修正长度为{correctionDuration}");
                clips.Add(clip);

                foreach (var item in clips)
                {
                    Debug.Log(item);
                }
            }
        }

        public override void MoveClipToFrame(Clip clip, int startFrame)
        {
            if (clip is AnimationClip animationClip && clips.Contains(animationClip))
            {
                if (CanInsertClipAtFrame(startFrame, clip.Duration, out int correctionDuration, clip))
                {
                    clip.StartFrame = startFrame;
                    clip.Duration = correctionDuration;
                }
            }
        }
    }
}
