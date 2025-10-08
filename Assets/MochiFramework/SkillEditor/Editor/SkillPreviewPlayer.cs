using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace MochiFramework.Skill.Editor
{
    public class SkillPreviewPlayer : ISkillPlayer
    {
        public enum PreviewPlayerState
        {
            Play,
            Pause,
            Stop
        }
        
        public SkillConfig CurrentSkill => skillEditor.SkillConfig;
        public float CurrentTime => (float)currentTime;
        public int CurrentFrame => Convert.ToInt32(currentTime * skillEditor.SkillConfig.frameRate);
        public bool IsPlaying => state == PreviewPlayerState.Play;
        public Animator Animator => animator;
        
        public Action OnPlay;
        public Action OnPause;
        public Action OnStop;
        
        private double currentTime = 0;
        private double lastTime = 0;
        private double detleTime = 0;
        private int lastFrame = 0;
        private PreviewPlayerState state = PreviewPlayerState.Stop;
        private bool IsNotPreview => !skillEditor.IsPreview || skillEditor.SkillConfig == null || skillEditor.PreviewPrefab == null;

        private List<TrackHandler> trackHandlers;
        
        private SkillEditor skillEditor;
        private GameObject previewObject;
        private Animator animator;

        public SkillPreviewPlayer(SkillEditor skillEditor)
        {
            this.skillEditor = skillEditor;
        }
        
        public void ShowPreviewCharacter()
        {
            if(!skillEditor.IsPreview) return;
            
            //在预览场景中 寻找 或 创建 PreviewObject
            if (previewObject == null)
            {
                previewObject = GameObject.Find("PreviewObject") ?? new GameObject("PreviewObject");
            }
            
            //清空原来的预览对象
            while (previewObject.transform.childCount > 0)
            {
                GameObject.DestroyImmediate(previewObject.transform.GetChild(0).gameObject);   
            }
            
            //创建新的预览对象
            if (skillEditor.PreviewPrefab != null)
            {
                GameObject.Instantiate(skillEditor.PreviewPrefab,previewObject.transform);
            }
            
            animator = previewObject.GetComponentInChildren<Animator>();
            
            Rebuild();
        }
        
        public void PlaySkill(SkillConfig skill)
        {
            if(IsNotPreview) return;
            skillEditor.SetSkillConfig(skill);
            PlaySkill();
        }

        public void PlaySkill(int startFrame)
        {
            currentTime = startFrame * skillEditor.SkillConfig.frameTime;
            Debug.Log($"起始帧:{startFrame},起始时间：{currentTime}");
            PlaySkill();
        }

        public void PlaySkill()
        {
            if (IsNotPreview)
            {
               StopCurrentSkill(); 
               return;
            }
            
            lastTime = EditorApplication.timeSinceStartup;
            state = PreviewPlayerState.Play;
            
            //每次开始播放，重新构建Handler
            Rebuild();
            foreach (var handler in trackHandlers)
            {
                handler.Play(CurrentFrame);
            }

            lastFrame = CurrentFrame - 1;
            
            OnPlay?.Invoke();
        }
        public void Update()
        {
            if(IsNotPreview) return;
            if(!IsPlaying) return;
            
            detleTime = EditorApplication.timeSinceStartup - lastTime;
            currentTime += detleTime;

            if (CurrentFrame != lastFrame)
            {
                foreach (var handler in trackHandlers)
                {
                    handler.Update(CurrentFrame);
                }
                lastFrame = CurrentFrame;
            }

            lastTime = EditorApplication.timeSinceStartup;
            
            if (CurrentFrame >= skillEditor.SkillConfig.frameCount)
            {
                StopCurrentSkill();
            }
        }
        
        public void Evaluate(int frame, bool isPause = false)
        {
            if (trackHandlers != null)
            {
                foreach (var handler in trackHandlers) 
                {
                    handler.Evaluate(frame);
                }
            }
            lastFrame = CurrentFrame;
        }

        public void StopCurrentSkill()
        {
            state = PreviewPlayerState.Stop;
            if (trackHandlers != null)
            {
                foreach (var handler in trackHandlers)
                {
                    handler.Stop();
                }
            }
            OnStop?.Invoke();
        }

        public void PlayOrPause()
        {
            if (IsNotPreview)
            {
                StopCurrentSkill();
                return;
            }

            if (state == PreviewPlayerState.Play)
            {
                state = PreviewPlayerState.Pause;
                foreach (var handler in trackHandlers)
                {
                    handler.Stop();
                }
                OnPause?.Invoke();
            }
            else
            {
                PlaySkill(skillEditor.SelectFrame);
            }
        }
        
        
        public void Rebuild()
        {
            DestroyTrackHandlers();
            if(IsNotPreview) return;
            
            trackHandlers = new List<TrackHandler>();
            foreach (var track in skillEditor.SkillConfig.tracks)
            {
                TrackHandler handler = track.CreateTrackHandler(previewObject);
                if (handler is not null)
                {
                    trackHandlers.Add(handler);
                }
            }
            
            Debug.Log("构建TrackHandler,总数:" + trackHandlers.Count);
        }
        
        private void DestroyTrackHandlers()
        {
            if(trackHandlers is null) return;
            foreach (var handler in trackHandlers)
            {
                handler.Dispose();
            }
            trackHandlers = null;
        }
    }
}
