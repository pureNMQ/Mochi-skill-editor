using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MochiFramework.Skill
{
    public class SkillPlayer : MonoBehaviour,ISkillPlayer
    {
        public SkillConfig CurrentSkill => currentSkill;
        public float CurrentTime => currentTime;
        public int CurrentFrame { get; }
        public bool IsPlaying => isPlaying;

        [SerializeField] private SkillConfig currentSkill;

        private float currentTime = 0;
        private bool isPlaying = false;
        private List<TrackHandler> trackHandlers;
        
        private Animator animator;
        
        
        public void PlaySkill(SkillConfig skill)
        {
            currentSkill = skill;
            PlaySkill();
        }
        
        [ContextMenu("播放技能")]
        public void PlaySkill()
        {
            if (currentSkill != null)
            {
                currentTime = 0;
                Debug.Log($"开始播放技能:{currentTime}");
                InitTrackHandler();

                foreach (var handler in trackHandlers)
                {
                    handler.Play();
                }
                isPlaying = true;
            }
            else
            {
                isPlaying = false;
                
            }
            
            Debug.Log($"初始化结束:{currentTime}");
        }

        public void StopCurrentSkill()
        {
            foreach (var handler in trackHandlers)
            {
                handler.Stop();
            }

            isPlaying = false;
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            isPlaying = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                PlaySkill();
            }
            
            
            if(!IsPlaying) return;
            
            currentTime += Time.deltaTime;
            int currentFrame = Convert.ToInt32(currentTime * currentSkill.frameRate);
            foreach (var handler in trackHandlers)
            {
                handler.Update(Time.deltaTime,currentFrame,currentTime);
                Debug.Log($"轨道更新:{currentFrame},{currentTime}");
            }

            if (currentFrame >= currentSkill.FrameCount)
            {
                StopCurrentSkill();
                Debug.Log("技能播放结束");
            }
        }

        private void OnDestroy()
        {
            DestroyTrackHandlers();
        }


        private void InitTrackHandler()
        {
            DestroyTrackHandlers();
            trackHandlers = new List<TrackHandler>();
            foreach (var track in currentSkill.tracks)
            {
                switch (track)
                {
                    case AnimationTrack animationTrack:
                        if(animator == null) return;
                        trackHandlers.Add(new AnimationTrackHandler(animationTrack,animator));
                        Debug.Log("创建动画轨道处理器");
                        break;
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

    public interface ISkillPlayer
    {
        public SkillConfig CurrentSkill { get; }
        public float CurrentTime { get; }
        public int CurrentFrame { get; }
        public bool IsPlaying { get; }
        
        public void PlaySkill(SkillConfig skill);
        public void PlaySkill();
        public void StopCurrentSkill();
    }
}
