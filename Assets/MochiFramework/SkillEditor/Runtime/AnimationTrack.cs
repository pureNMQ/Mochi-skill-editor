using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class AnimationTrack : Track
    {
        public override string TrackName => "动画轨道";
        [SerializeField] protected List<AnimationClip> clips = new List<AnimationClip>();
        
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

        public override Clip InsertClipAtFrame(int startFrame, object obj)
        {
            if (obj is UnityEngine.AnimationClip animationClip)
            {
                return InsertClipAtFrame(startFrame, animationClip);
            }

            return null;
        }

        public override IEnumerator<Clip> GetEnumerator()
        {
            return clips.GetEnumerator();
        }

        public static AnimationTrack CreateAnimationTrack()
        {
            AnimationTrack animationTrack = CreateInstance<AnimationTrack>();
            animationTrack.name = "AnimationTrack";
            return animationTrack;
        }

        public Clip InsertClipAtFrame(int startFrame, UnityEngine.AnimationClip animationClip)
        {
            int duration = Mathf.CeilToInt(animationClip.length * animationClip.frameRate);
            if (CanInsertClipAtFrame(startFrame, duration, out int correctionDuration))
            {
                //TODO 使用ScriptableObject创建对象，并且使用AssetDatabase.AddObjectToAsset将对象附加到SkillConfig上
                AnimationClip clip = AnimationClip.CreateAnimationClip(startFrame, animationClip,correctionDuration); 
                Debug.Log($"插入一个动画片段{animationClip.name}，起始帧为{startFrame}，原始长度为{duration}，修正长度为{correctionDuration}");
                clips.Add(clip);
                clips = clips.OrderBy(clip => clip.StartFrame).ToList();
                return clip;
            }

            return null;
        }

        public override void MoveClipToFrame(Clip clip, int startFrame)
        {
            //类型验证，权限范围验证
            if (clip is not AnimationClip animationClip || !clips.Contains(animationClip)) return;
            //判断是否可以移动到该为止
            if (!CanInsertClipAtFrame(startFrame, clip.Duration, out int correctionDuration, clip)) return;
            //判断插入时长度是否被修正，如果被修正则不可以移动
            if (clip.Duration != correctionDuration) return;
            
            clip.StartFrame = startFrame;
            
            clips = clips.OrderBy(clip => clip.StartFrame).ToList();

        }

        public override Clip RemoveClip(Clip clip)
        {
            if (clip is AnimationClip animationClip)
            {
                clips.Remove(animationClip);
            }
            
            return clip;    
        }
    }
}
