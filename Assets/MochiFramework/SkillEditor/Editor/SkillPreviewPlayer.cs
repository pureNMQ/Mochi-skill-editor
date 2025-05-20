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
        private PreviewPlayerState state = PreviewPlayerState.Stop;
        private bool IsNotPreview => !skillEditor.IsPreview || skillEditor.SkillConfig == null;

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
            if (trackHandlers != null)
            {
                foreach (var handler in trackHandlers) 
                {
                    handler.Play(CurrentFrame,(float)currentTime);
                }
            }
            OnPlay?.Invoke();
        }
        public void Update()
        {
            if(IsNotPreview) return;
            if(!IsPlaying) return;
            
            detleTime = EditorApplication.timeSinceStartup - lastTime;
            currentTime += detleTime;
            
            if (trackHandlers != null)
            {
                foreach (var handler in trackHandlers) 
                {
                    handler.Update((float)detleTime,CurrentFrame,(float)currentTime);
                }
            }
            
            lastTime = EditorApplication.timeSinceStartup;
            
            if (CurrentFrame >= skillEditor.SkillConfig.FrameCount)
            {
                StopCurrentSkill();
            }
        }

        public void StopCurrentSkill()
        {
            state = PreviewPlayerState.Stop;
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
                OnPause?.Invoke();
            }
            else
            {
                PlaySkill(skillEditor.SelectFrame);
            }
        }

        public void Evaluate(int frame)
        {
            currentTime = frame * skillEditor.SkillConfig.frameTime;
            if (trackHandlers != null)
            {
                foreach (var handler in trackHandlers) 
                {
                    handler.Evaluate(CurrentFrame,(float)currentTime);
                }
            }
        }
        
        public void Rebuild()
        {
            DestroyTrackHandlers();
            if(IsNotPreview) return;
            
            trackHandlers = new List<TrackHandler>();
            foreach (var track in skillEditor.SkillConfig.tracks)
            {
                TrackHandler handler = TrackHandlerFactory.Create(track,this);
                if (handler is not null)
                {
                    trackHandlers.Add(handler);
                }
            }
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
